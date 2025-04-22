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
    
    [Header("몬스터 스폰 관리")]//스폰 수
    public int spawnAmount = 5;
    public int monsterMaxField = 10;
    public int spawnAmountDifficult = 0;
    public float spawnTimeDifficult = 0.1f;
    public int monsterKillMaxCount = 30;
    
    [Header("유니크 몬스터")]//유니크 스폰
    public int uniqueSpawnAmount = 0;
    public int uniqueSpawnMaxChance = 40;
    
    [Space(10)]
    [Header("보물 스폰 관리")]
    public int treasureMaxField = 10;
    public int treasureDist = 10;
    public int poissonResearchLimit = 30;
    
    [Space(10)]
    public List<GameObject> commonMonsters = new List<GameObject>();
    public List<GameObject> flyingMonsters = new List<GameObject>();
    public List<GameObject> uniqueMonsters = new List<GameObject>();
    public List<GameObject> bossMonsters = new List<GameObject>();

    [Space(10)]
    public List<GameObject> treasures = new List<GameObject>();
    
    public GameObject player;//임시 퍼블릭
    private GameObject mFieldMonsterParent;
    private GameObject mRandomTreasureParent;

    private List<GameObject> mMonsterSections = new List<GameObject>();
    private List<NavMeshModifierVolume> mNavMeshes = new List<NavMeshModifierVolume>();
    
    //레벨디자인
    private int mLevelDesign = 1;
    
    //스폰주 기
    //역수 기반 감소 (감소 곡선) 스폰주기 = 플레이타임 /((1f+난이도*스폰난이도))
    private float currentTime;
    private float spawnInterval;

    //몬스터 관리
    private int mMonsterCurrentField;
    private int mMonsterKillCurrentCount;

    //임시
    private void Start()
    {
        Init(null, 1);
        SpawnTreasure();
    }

    private void FixedUpdate()
    {
        currentTime += Time.fixedDeltaTime;
        if (currentTime > spawnInterval)
        {
            currentTime = 0;
            SpawnMonsters();
        }
    }

    public void Init(GameObject player, int levelDesign)
    {
        //this.player = player;
        mLevelDesign = levelDesign;
        GetBattleSections();
        GetNavGrounds();
        mFieldMonsterParent = FindChildrenWithName("FieldMonsters",this.gameObject);
        mRandomTreasureParent = FindChildrenWithName("RandomTreasure",this.gameObject);
    }

    public void ClearField()
    {
        ClearObjs(mFieldMonsterParent);
        ClearObjs(mRandomTreasureParent);
    }

    void GetBattleSections()
    {
        GameObject battleSection = FindChildrenWithName("BattleSections",this.gameObject);
        foreach (Transform section in battleSection.transform)
        {
            if (section.name == "BattleSections") continue;
            mMonsterSections.Add(section.gameObject);
        }
    }

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

    void ClearObjs(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
