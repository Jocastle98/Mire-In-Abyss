using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using BattleAreaEnum;

public class DungeonRoomController : MonoBehaviour
{
    public bool IsClear;
    public SOSpawnTypeList spawnTypeList;
    public DungeonRoomType roomType;
    
    public delegate void CloseDoors();
    CloseDoors closeDoors;
    public delegate void OpenDoors();
    OpenDoors openDoors;
    
    private bool fighting;
    private int spwanMonsterAmount;
    private List<SoMonsterCount> monsterLists;
    private List<GameObject> entranceList = new List<GameObject>();
    private List<SpawnController> spawnControllers = new List<SpawnController>();
    
    public void DungeonRoomInit()
    {
        if (roomType == DungeonRoomType.BossRoom || roomType == DungeonRoomType.MonsterRoom)
        {
            IsClear = false;
        }
        else
        {
            IsClear = true;
        }
        fighting = false;

        SearchSpawner();
        monsterLists = spawnTypeList.monsterSpawnTypeList;
    }
    
    public void RegisterEntrancePrefab(GameObject prefab,DungeonEntranceController controller,
        CloseDoors entranceClose,OpenDoors openDoor)
    {
        entranceList.Add(prefab);
        
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
                spawnControllers.Add(spawnController);
            }
        }
    }

    private void DungeonSpawn(Transform playerTransform)
    {
        if (fighting||IsClear) return;
        fighting = true;

        closeDoors.Invoke();

        ConfigureSpawnerDistance(playerTransform);
        
        SoMonsterCount spawnType = monsterLists[Random.Range(0, monsterLists.Count)];
        List<SOMonsters> meleeMonster = spawnType.meleeMonster;
        List<SOMonsters> rangedMonster = spawnType.rangedMonster;
        spwanMonsterAmount = GetTotalSpawnCount(spawnType);

        int index = 0;
        int spawnedCount = 0;

        while (spawnedCount < spwanMonsterAmount)
        {
            if (index >= spawnControllers.Count)
                index = 0;

            int rand = Random.Range(0, 10);

            if (meleeMonster.Count > 0)
            {
                SOMonsters meleeObj = meleeMonster[rand % meleeMonster.Count];
                spawnControllers[index].SpawnObj(meleeObj.monsters[rand % meleeObj.monsters.Count], gameObject);
                spawnedCount++;
            }

            if (rangedMonster.Count > 0)
            {
                SOMonsters rangedObj = rangedMonster[rand % rangedMonster.Count];
                spawnControllers[spawnControllers.Count - index - 1]
                    .SpawnObj(rangedObj.monsters[rand % rangedObj.monsters.Count], gameObject);
                spawnedCount++;
            }

            index++;
        }
    }
    
    private int GetTotalSpawnCount(SoMonsterCount spawnType)
    {
        int total = 0;
        foreach (int count in spawnType.meleeMonsterCount)
            total += count;
        foreach (int count in spawnType.rangedMonsterCount)
            total += count;
        return total;
    }

    private void ConfigureSpawnerDistance(Transform playerTransform)
    {
        //거리계산
        Dictionary<float, SpawnController> spawnersDict = new Dictionary<float, SpawnController>();
        List<float> spawnersIndex = new List<float>();
        foreach (SpawnController spawner in spawnControllers)
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

        spawnControllers = spawners;
    }

    
    private void DungeonClear()
    {
        openDoors.Invoke();
    }
}
