using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FieldController : Abyss
{
    private FieldDataSO mFieldData;
    //스폰 주기
    //역수 기반 감소 (감소 곡선) 스폰주기 = 플레이타임 /((1f+난이도*스폰난이도))
    private float mCurrentTime;
    private float mSpawnInterval;
    private bool CanSpawn = true;

    //스폰 구역
    private GameObject mPlayer;
    private GameObject mBoss;
    private GameObject mField;
    private GameObject mPlayerSpawner;
    private GameObject mBossSpawner;
    private GameObject mMonsterSpawner;
    private SpawnController mSpawnController;
    private List<NavMeshModifierVolume> mNavMeshes = new List<NavMeshModifierVolume>();
    //private List<GameObject> mMonsterSections = new List<GameObject>();

    //레벨디자인
    private int mLevelDesign = 1;

    //몬스터 관리
    private int mMonsterCurrentField;
    private int mMonsterMaxFieldCount = 10;
    private int mMonsterKillCurrentCount;
    private bool IsBossSpawn = false;

    //생성된 게임오브젝트들을 하이어라키상 정리용 폴더
    private GameObject mFieldMonsterFolder;
    private GameObject mRandomTreasureFolder;

    private void Start()
    {
        FieldInit();
    }

    private void FixedUpdate()
    {
        mCurrentTime += Time.fixedDeltaTime;
        //Debug.Log("Current Time: " + mCurrentTime + " / " + spawnInterval);
        if (mCurrentTime > mSpawnInterval && CanSpawn)
        {
            if (SpawnCoolDownUpdate())
            {
                mCurrentTime = 0;
                SpawnMonsters();
            }
        }
    }

    public override void BattleAreaClear()
    {
        ClearField();
        OnClearBattleArea.Invoke();
        Destroy(mField);
    }

    public override void SetPortal()
    {
        portal = Instantiate(AbyssManager.Instance.portalPrefab);
        DeActivatePortal();
        AbyssMoveController moveCon = portal.GetComponent<AbyssMoveController>();
        moveCon.battleAreaMoveDelegate = BattleAreaClear;
    }

    /// <summary>
    /// 제작된 맵 프리팹의 필요한 오브젝트들을 불러옴.
    /// 플레이어 스폰.
    /// 보물 상자 스폰.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="levelDesign"></param>
    public void FieldInit()
    {
        mPlayer = AbyssManager.Instance.player;
        mLevelDesign = AbyssManager.Instance.levelDesign;
        OnClearBattleArea -= AbyssManager.Instance.BattleAreaClear;
        OnClearBattleArea += AbyssManager.Instance.BattleAreaClear;
        mFieldData = AbyssManager.Instance.abyssFields[Random.Range(0, AbyssManager.Instance.abyssFields.Count)];
        
        mMonsterMaxFieldCount += Mathf.FloorToInt(mLevelDesign * .1f);
        mField = Instantiate(mFieldData.battleFields);
        mFieldMonsterFolder = new GameObject("FieldMonsterFolder");
        mRandomTreasureFolder = new GameObject("RandomTreasureFolder");

        GetNavGrounds();
        mPlayerSpawner = FindChildrenWithName(mFieldData.playerSpawnZoneName, mField);
        mBossSpawner = FindChildrenWithName(mFieldData.bossSpawnZoneName, mField);
        mMonsterSpawner = GetMonsterSpawnZone();
        mSpawnController = mMonsterSpawner.GetComponent<SpawnController>();

        SetPortal();
        SpawnTreasure();
        PlayerSpawn();

    }

    /// <summary>
    /// 보스를 잡고 맵을 이동할 때 필드의 모든 요소들을 정리
    /// </summary>
    public void ClearField()
    {
        ClearObjs(mFieldMonsterFolder);
        ClearObjs(mRandomTreasureFolder);
    }

    /// <summary>
    /// 몬스터 스폰 구역과 자식 오브젝트들을 가져옴
    /// </summary>
    GameObject GetMonsterSpawnZone()
    {
        GameObject obj = new GameObject("MonsterSpawner");
        obj.transform.SetParent(transform);
        obj.AddComponent<SpawnController>();
        return obj;

        // return Type void
        // GameObject spawnZone = FindChildrenWithName(monsterSpawnZoneName,gameObject);
        // foreach (Transform section in spawnZone.transform)
        // {
        //     if (section.name == monsterSpawnZoneName) continue;
        //     mMonsterSections.Add(section.gameObject);
        // }
    }

    /// <summary>
    /// 베이크 된 혹은 베이크할 땅 오브젝트들을 가져옴
    /// </summary>
    void GetNavGrounds()
    {
        GameObject environment = FindChildrenWithName(mFieldData.environmentName, mField);
        GameObject navGround = FindChildrenWithName(mFieldData.navGroundsName, environment);


        foreach (Transform navs in navGround.transform)
        {
            NavMeshModifierVolume navMeshModifier = navs.gameObject.GetComponent<NavMeshModifierVolume>();
            if (navMeshModifier != null)
            {
                mNavMeshes.Add(navMeshModifier);
            }
        }

    }

    void PlayerSpawn()
    {
        // Debug.Log("PlayerSpawner1 : " + mPlayerSpawner.transform.position + Vector3.up * 2f);
        // mPlayer.transform.position = mPlayerSpawner.transform.position + Vector3.up * 2f;
        // Debug.Log("PlayerSpawner2 : " + mPlayerSpawner.transform.position + Vector3.up * 2f);
        if (mPlayerSpawner == null)
        {
            Debug.LogError("PlayerSpawner is null!");
            return;
        }

        Vector3 spawnPosition = mPlayerSpawner.transform.position + Vector3.up * 2f;
        Debug.Log("Set player position to: " + spawnPosition);

        mPlayer.transform.position = spawnPosition;

        Debug.Log("Actual player position after assign: " + mPlayer.transform.position);
    }

    /// <summary>
    /// 몬스터를 스폰하는 함수
    /// 근처 가까운 스폰 구역을 찾고 없으면 비행 본스터를 소환
    /// </summary>
    void SpawnMonsters()
    {
        //스폰 수 최신화
        int spawnCount = (int)(mFieldData.spawnAmount +
                               Random.Range(0, mLevelDesign + mFieldData.spawnAmountDifficult * 0.1f));
        
        spawnCount = mMonsterMaxFieldCount < mMonsterCurrentField + spawnCount ?
            mMonsterMaxFieldCount - mMonsterCurrentField : spawnCount;
        if (spawnCount == 0) return;
        
        mMonsterCurrentField += spawnCount;
        
        int unique = Mathf.Min(mLevelDesign / mFieldData.uniqueSpawnMaxChance, mFieldData.uniqueSpawnMaxChance);

        //일반 , 유니크 확률 소환
        SOSpawnTypeList monsterLists = Random.Range(0, 100) > unique
            ? mFieldData.commonMonsters
            : mFieldData.eliteMonsters;

        mMonsterSpawner.transform.position = mPlayer.transform.position + Vector3.up * 2f;
        mSpawnController.SpawnObjWithSoGroupList(monsterLists, spawnCount, mFieldMonsterFolder.transform,
            MonsterDeadCounting);
    }

    bool SpawnCoolDownUpdate()
    {
        //스폰 주기 최신화
        float newSpawnTime = mFieldData.playTime /
                         ((mFieldData.mMinSpawnCount + mLevelDesign * mFieldData.spawnTimeDifficult));
        if (Mathf.Abs(mSpawnInterval - newSpawnTime) > .1f)
        {
            mSpawnInterval = newSpawnTime;
            return false;
        }

        return true;
    }

    /// <summary>
    /// 베이크된 땅 오브젝트 위로 베이크 된 부분에만 보물상자를 스폰
    /// </summary>
    void SpawnTreasure()
    {
        int treasureAmount = mFieldData.treasureSpawnAmount; //+ Random.Range(-5, 5)
        int whileLoop = 0;
        int whileMaxLoop = 0;

        int totalWeight = mNavMeshes.Count * (mNavMeshes.Count + 1) / 2;
        int rest = treasureAmount;
        List<int> distributed = new List<int>();

        for (int i = 0; i < mNavMeshes.Count; i++)
        {
            int weight = mNavMeshes.Count - i;
            int amount = treasureAmount * weight / totalWeight;
            distributed.Add(amount);
            rest -= amount;
        }
        
        for (int i = 0; rest > 0; i = (i + 1) % distributed.Count)
        {
            distributed[i]++;
            rest--;
        }

        while (whileMaxLoop < 5)
        {
            for (int i = 0; i < mNavMeshes.Count; i++)
            {
                GameObject meshGameObject = mNavMeshes[i].gameObject;
                NavMeshModifierVolume navMesh = mNavMeshes[i].GetComponent<NavMeshModifierVolume>();

                Vector2 navSize = new Vector2(navMesh.size.x, navMesh.size.z);
                PoissonGenerator poissonGenerator = new PoissonGenerator(navSize, mFieldData.treasureDistance + whileLoop * 2);
                List<Vector2> points =
                    poissonGenerator.GeneratePoissonList(mFieldData.poissonResearchLimit);

                for (int j = 0; j < points.Count; j++)
                {
                    Vector3 localPoint = new Vector3(points[j].x, 0, points[j].y);
                    Vector3 worldPoint = meshGameObject.transform.TransformPoint(localPoint);

                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(worldPoint, out navHit, 5f, 1 << navMesh.area))
                    {
                        int setChance = Random.Range(0, 100);
                        if (setChance > 20 - whileLoop * 5 && distributed[i] > 0)
                        {
                            GameObject treasure = Instantiate(
                                mFieldData.treasures.monsters[Random.Range(0, mFieldData.treasures.monsters.Count)],
                                navHit.position, Quaternion.identity);
                            treasure.transform.SetParent(mRandomTreasureFolder.transform);

                            distributed[i]--;
                        }
                    }
                }

                rest += distributed[i];
            }

            whileMaxLoop++;
            if (rest <= 0) break;
        }
    }

    void BossSpawn()
    {
        mBoss = mFieldData.bossMonsters.monsters[Random.Range(0, mFieldData.bossMonsters.monsters.Count)];
        SpawnController spawnController = mBossSpawner.GetComponent<SpawnController>();
        spawnController.SpawnObj(mBoss, mFieldMonsterFolder.transform,SetPortalAtBoss);
    }

    void SetPortalAtBoss()
    {
        Debug.Log("Boss is Dead!");
        ActivatePortal(portal,mBossSpawner);
    }

    void MonsterDeadCounting()
    {
        mMonsterKillCurrentCount++;
        mMonsterCurrentField--;
        if (mMonsterKillCurrentCount >= mFieldData.monsterKillMaxCount && !IsBossSpawn)
        {
            BossSpawn();
            CanSpawn = false;
            IsBossSpawn = true;
            mMonsterKillCurrentCount = 0;
        }
    }

    /// <summary>
    /// 가까운 스폰 구역을 찾고 반환함
    /// </summary>
    /// <returns></returns>
    GameObject FindClosestSpawnController()
    {
        // GameObject closestSpawnController = null;
        // foreach (GameObject spawner in mMonsterSections)
        // {
        //     float dist = Vector3.Distance(spawner.transform.position, mPlayer.transform.position);
        //     if (dist < 10)
        //     {
        //         closestSpawnController = spawner.gameObject;
        //         return closestSpawnController;
        //     }
        // }
        //
        // return closestSpawnController;
        return null;
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
            if (child.name.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                obj = child.gameObject;
                return obj;
            }
        }

        return null;
    }
}
