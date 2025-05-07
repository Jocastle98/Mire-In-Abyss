using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using BattleAreaEnums;

public class DungeonRoomController : MonoBehaviour
{
    public bool IsClear;
    public SOSpawnTypeList spawnTypeList;
    public SOMonsters bossMonsters;
    public DungeonRoomType roomType;
    
    public delegate void CloseDoors();
    CloseDoors closeDoors;
    public delegate void OpenDoors();
    OpenDoors openDoors;
    
    private bool IsFight;
    private int mSpwanMonsterAmount;
    private int mMonsterKillCurrentCount;
    private GameObject mPortal;
    private Vector3 mRoomCenterPos;
    private List<SoMonsterCount> mMonsterLists;
    private List<GameObject> mEntranceList = new List<GameObject>();
    private List<SpawnController> mSpawnControllers = new List<SpawnController>();
    
    public void DungeonRoomInit(GameObject portal,Vector3 roomCenterPos)
    {
        if (roomType == DungeonRoomType.BossRoom || roomType == DungeonRoomType.MonsterRoom)
        {
            IsClear = false;
            mPortal = portal;
            mRoomCenterPos = roomCenterPos;
        }
        else
        {
            IsClear = true;
        }
        IsFight = false;

        SearchSpawner();
        mMonsterLists = spawnTypeList.monsterSpawnTypeList;
    }
    
    public void RegisterEntrancePrefab(GameObject prefab,DungeonEntranceController controller,
        CloseDoors entranceClose,OpenDoors openDoor)
    {
        mEntranceList.Add(prefab);
        
        controller.RegisterSpawnFunc(DungeonSpawn);
        
        openDoors -= openDoor;
        openDoors += openDoor;
        closeDoors -= entranceClose;
        closeDoors += entranceClose;
        
    }
    
    private void SearchSpawner()
    {
        foreach (Transform tf in gameObject.transform)
        {
            SpawnController spawnController = tf.GetComponent<SpawnController>();
            if (spawnController != null)
            {
                mSpawnControllers.Add(spawnController);
            }
        }
    }

    private void DungeonSpawn(Transform playerTransform)
    {
        if (IsFight||IsClear) return;
        IsFight = true;

        closeDoors.Invoke();

        switch (roomType)
        {
            case DungeonRoomType.BossRoom:
                BossSpawn();
                break;
            case DungeonRoomType.MonsterRoom:
                CommonMonsterSpawn(playerTransform);
                break;
        }
        
    }

    void BossSpawn()
    {
        GameObject boss = bossMonsters.monsters[Random.Range(0, bossMonsters.monsters.Count)];
        mSpawnControllers[0].SpawnObj(boss, transform,BossDead);
    }

    void CommonMonsterSpawn(Transform playerTransform)
    {
        ConfigureSpawnerDistance(playerTransform);

        int spawnTypeIndex = Random.Range(0, mMonsterLists.Count);
        SoMonsterCount spawnType = mMonsterLists[spawnTypeIndex];

        List<SOMonsters> meleeMonsters = spawnType.meleeMonster;
        List<SOMonsters> rangedMonsters = spawnType.rangedMonster;

        int spawnerIndex = 0;

        // 근거리 몬스터 소환
        for (int i = 0; i < meleeMonsters.Count; i++)
        {
            for (int j = 0; j < spawnType.meleeMonsterCount[i]; j++)
            {
                if (spawnerIndex >= mSpawnControllers.Count * 0.5f)
                    spawnerIndex = 0;

                var monster = meleeMonsters[i].monsters[Random.Range(0, meleeMonsters[i].monsters.Count)];
                mSpawnControllers[spawnerIndex].SpawnObj(monster, transform, MonsterDeadCounting);
                mSpwanMonsterAmount++;
                spawnerIndex++;
            }
        }

        // 원거리 몬스터 소환
        for (int i = 0; i < rangedMonsters.Count; i++)
        {
            for (int j = 0; j < spawnType.rangedMonsterCount[i]; j++)
            {
                if (spawnerIndex >= mSpawnControllers.Count * 0.5f)
                    spawnerIndex = 0;

                var monster = rangedMonsters[i].monsters[Random.Range(0, rangedMonsters[i].monsters.Count)];
                mSpawnControllers[spawnerIndex].SpawnObj(monster, transform, MonsterDeadCounting);
                mSpwanMonsterAmount++;
                spawnerIndex++;
            }
        }
    }

    private void ConfigureSpawnerDistance(Transform playerTransform)
    {
        //거리계산
        Dictionary<float, SpawnController> spawnersDict = new Dictionary<float, SpawnController>();
        List<float> spawnersIndex = new List<float>();
        foreach (SpawnController spawner in mSpawnControllers)
        {
            float newDist = ((spawner.transform.position - playerTransform.position).sqrMagnitude)/10000f;

            while (spawnersDict.ContainsKey(newDist))
            {
                newDist++;
            }
            spawnersDict.Add(newDist, spawner);
            spawnersIndex.Add(newDist);
        }
        
        //거리정렬
        spawnersIndex.Sort();
        
        //거리에 따른 컨트롤러 재정렬
        List<SpawnController> spawners = new List<SpawnController>();
        foreach (float dist in spawnersIndex)
        {
            if(spawnersDict.TryGetValue(dist, out SpawnController spawner))
            {
                spawners.Add(spawner);
            }
        }

        mSpawnControllers = spawners;
    }
    
    void MonsterDeadCounting()
    {
        mMonsterKillCurrentCount++;
        if (mMonsterKillCurrentCount >= mSpwanMonsterAmount )
        {
            mMonsterKillCurrentCount = 0;
            mSpwanMonsterAmount = 0;
            DungeonClear(DungeonRoomType.MonsterRoom);
        }
    }

    void BossDead()
    {
        DungeonClear(DungeonRoomType.BossRoom);
    }

    
    private void DungeonClear(DungeonRoomType roomType)
    {
        openDoors.Invoke();
        if (roomType == DungeonRoomType.BossRoom)
        {
            mPortal.SetActive(true);
            mPortal.transform.position = mRoomCenterPos;
        }
    }
}
