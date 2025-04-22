using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FieldController : MonoBehaviour
{
    public int playTime = 180;
    
    //스폰 수
    [Header("몬스터 스폰 관리")]
    public int spawnAmount = 5;
    public int monsterMaxField = 10;
    public int spawnAmountDifficult = 0;
    public float spawnTimeDifficult = 0.1f;
    public int monsterKillMaxCount = 30;
    
    //유니크 스폰
    [Header("유니크 몬스터")]
    public int uniqueSpawnAmount = 0;
    public int uniqueSpawnMaxChance = 40;
    
    //보물상자 스폰
    [Space(10)]
    [Header("보물상자 스폰 관리")]
    public int treasureMaxField = 10;
    public int treasureDist = 10;
    public int poissonResearchLimit = 10;
    
    //몬스터 프리팹
    [Space(10)] 
    public List<GameObject> commonMonsters = new List<GameObject>();
    public List<GameObject> flyingMonsters = new List<GameObject>();
    public List<GameObject> uniqueMonsters = new List<GameObject>();
    public List<GameObject> bossMonsters = new List<GameObject>();

    //보물상자 프리팹
    [Space(10)] 
    public List<GameObject> treasures = new List<GameObject>();
    
    //스폰 주기
    //역수 기반 감소 (감소 곡선) 스폰주기 = 플레이타임 /((1f+난이도*스폰난이도))
    private float mCurrentTime;
    public float spawnInterval;
    
    public GameObject player;//임시 퍼블릭

    //스폰 구역
    private List<GameObject> mMonsterSections = new List<GameObject>();
    private List<NavMeshModifierVolume> mNavMeshes = new List<NavMeshModifierVolume>();
    
    //레벨디자인
    private int mLevelDesign = 1;

    //몬스터 관리
    private int mMonsterCurrentField;
    private int mMonsterKillCurrentCount;
    
    //생성된 게임오브젝트들을 하이어라키상 정리용 폴더
    private GameObject mPlayerSpawnerParent;
    private GameObject mFieldMonsterParent;
    private GameObject mRandomTreasureParent;

    //임시 Init함수는 BattleAreaManager에 의해 사용됨
    private void Start()
    {
        FieldInit(null, 1);
        SpawnTreasure();
    }

    private void FixedUpdate()
    {
        mCurrentTime += Time.fixedDeltaTime;
        if (mCurrentTime > spawnInterval)
        {
            mCurrentTime = 0;
            SpawnMonsters();
        }
    }

    /// <summary>
    /// 제작된 맵 프리팹의 필요한 오브젝트들을 불러옴.
    /// 플레이어 스폰.
    /// 보물 상자 스폰.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="levelDesign"></param>
    public void FieldInit(GameObject player, int levelDesign)
    {
        //this.player = player;
        mLevelDesign = levelDesign;
        GetBattleSections();
        GetNavGrounds();
        mFieldMonsterParent = FindChildrenWithName("FieldMonsters",this.gameObject);
        mRandomTreasureParent = FindChildrenWithName("RandomTreasure",this.gameObject);
    }

    /// <summary>
    /// 보스를 잡고 맵을 이동할 때 필드의 모든 요소들을 정리
    /// </summary>
    public void ClearField()
    {
        ClearObjs(mFieldMonsterParent);
        ClearObjs(mRandomTreasureParent);
    }
    
    /// <summary>
    /// 몬스터 스폰 구역과 자식 오브젝트들을 가져옴
    /// </summary>
    void GetBattleSections()
    {
        GameObject battleSection = FindChildrenWithName("BattleSections",this.gameObject);
        foreach (Transform section in battleSection.transform)
        {
            if (section.name == "BattleSections") continue;
            mMonsterSections.Add(section.gameObject);
        }
    }
    
    /// <summary>
    /// 베이크 된 혹은 베이크할 땅 오브젝트들을 가져옴
    /// </summary>
    void GetNavGrounds()
    {
        GameObject environment = FindChildrenWithName("Environment",this.gameObject);
        GameObject navGround = FindChildrenWithName("NavGrounds",environment);
        
        
        foreach (Transform navs in navGround.transform)
        {
            NavMeshModifierVolume navMeshModifier = navs.gameObject.GetComponent<NavMeshModifierVolume>();
            if (navMeshModifier != null)
            {
                mNavMeshes.Add(navMeshModifier);
            }
        }
        
    }
    
    /// <summary>
    /// 몬스터를 스폰하는 함수
    /// 근처 가까운 스폰 구역을 찾고 없으면 비행 본스터를 소환
    /// </summary>
    void SpawnMonsters()
    {
        //스폰 주기 최신화
        spawnInterval = playTime / ((1f + mLevelDesign * spawnTimeDifficult));
        //스폰 수 최신화
        int spawnCount = (int)(spawnAmount + Random.Range(0, (mLevelDesign + spawnAmountDifficult * 0.1f)));

        for (int i = 0; i < spawnCount; i++)
        {
            if (monsterMaxField <= mMonsterCurrentField) return;

            int unique = Mathf.Min(mLevelDesign / uniqueSpawnMaxChance, uniqueSpawnMaxChance);
            
            //일반 , 유니크 확률 소환
            GameObject monsterPrefab = Random.Range(0, 100) > unique
                ? commonMonsters[Random.Range(0, commonMonsters.Count)]
                : uniqueMonsters[Random.Range(0, uniqueMonsters.Count)];
            
            //가까운 스폰장소를 찾음
            GameObject spawner = FindClosestSpawnController();
            if (spawner != null)
            {
                SpawnController spawnController = spawner.GetComponent<SpawnController>();
                spawnController.SpawnObj(monsterPrefab, mFieldMonsterParent);
            }
            else //없으면 플레이어 주위로 비행 몬스터 소환
            {
                Vector3 randomPointOnCircle = Random.insideUnitSphere;
                randomPointOnCircle.Normalize(); // 방향만 남김 (길이 1)
                randomPointOnCircle *= Random.Range(5, 10); // 원하는 반지름으로 스케일 조정

                GameObject monster = Instantiate(flyingMonsters[Random.Range(0, flyingMonsters.Count)],
                    player.transform.position + randomPointOnCircle,
                    Quaternion.identity);
                monster.transform.SetParent(mFieldMonsterParent.transform);
            }
        }
    }
    
    /// <summary>
    /// 베이크된 땅 오브젝트 위로 베이크 된 부분에만 보물상자를 스폰
    /// </summary>
    void SpawnTreasure()
    {
        int treasureAmount = treasureMaxField + Random.Range(-5, treasureMaxField);
        int whileLoop = 0;
        int whileMaxLoop = 5;
        
        while (whileMaxLoop>0)
        {
            for (int i = 0; i < mNavMeshes.Count; i++)
            {
                GameObject meshGameObject = mNavMeshes[i].gameObject;
                NavMeshModifierVolume navMesh = mNavMeshes[i].GetComponent<NavMeshModifierVolume>();

                Vector2 navSize = new Vector2(navMesh.size.x, navMesh.size.z);
                PoissonGenerator poissonGenerator = new PoissonGenerator(navSize, treasureDist + whileLoop * 2);
                List<Vector2> points =
                    poissonGenerator.GeneratePoissonList(poissonResearchLimit);

                for (int j = 0; j < points.Count; j++)
                {
                    Vector3 localPoint = new Vector3(points[j].x, 0, points[j].y);
                    Vector3 worldPoint = meshGameObject.transform.TransformPoint(localPoint);

                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(worldPoint, out navHit, 5f, NavMesh.AllAreas))
                    {
                        int setChance = Random.Range(0, 100);
                        if (setChance > 20 - whileLoop * 5 && treasureAmount > 0)
                        {
                            GameObject treasure = Instantiate(treasures[Random.Range(0, treasures.Count)],
                                navHit.position, Quaternion.identity);
                            treasure.transform.SetParent(mRandomTreasureParent.transform);
                    
                            treasureAmount--;
                        }
                    }
                    
                    // GameObject treasure = Instantiate(treasures[Random.Range(0, treasures.Count)],
                    //     worldPoint, Quaternion.identity);
                    // treasure.transform.SetParent(mRandomTreasureParent.transform);
                    

                }
            }
            //whileMaxLoop--;
            break;
        }
    }

    /// <summary>
    /// 가까운 스폰 구역을 찾고 반환함
    /// </summary>
    /// <returns></returns>
    GameObject FindClosestSpawnController()
    {
        GameObject closestSpawnController = null;
        foreach (GameObject spawner in mMonsterSections)
        {
            float dist = Vector3.Distance(spawner.transform.position, player.transform.position);
            if(dist < 10) closestSpawnController = spawner.gameObject;
        }
        
        return closestSpawnController;
    }

    /// <summary>
    /// 부모 오브젝트의 이름과 같은 오브젝트를 반환함
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    GameObject FindChildrenWithName(string name, GameObject parent)
    {
        GameObject obj = null;
        foreach (Transform child in parent.transform)
        {
            if (child.name == name)
            {
                obj = child.gameObject;
                return obj;
            }
        }

        return null;
    }

    /// <summary>
    /// 해당 오브젝트의 자식들을 파괴
    /// </summary>
    /// <param name="obj"></param>
    void ClearObjs(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
