using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using BattleAreaEnums;

public class SpawnController : MonoBehaviour
{
    public float radius = 10;

    public void SpawnObj(GameObject obj, Transform parent, System.Action monsterDead)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(0, radius);
            Vector3 randomPointOnCircle = new Vector3(circle.x, 0f, circle.y);

            RaycastHit hit;
            if (Physics.Raycast(transform.position + randomPointOnCircle, Vector3.down,
                    out hit,100f, LayerMask.GetMask("Ground")))
            {
                GameObject spawnObj = Instantiate(obj, hit.point, Quaternion.identity);
                spawnObj.transform.parent = parent;
                EnemyBTController monsterDeSpawnTest = spawnObj.GetComponent<EnemyBTController>();
                if (monsterDeSpawnTest != null)
                {
                    monsterDeSpawnTest.monsterDead = monsterDead;
                }

                return;
            }
        }

        Debug.Log(" Spawn Failed.. / Monster name : " + obj.name);
    }

    public void SpawnObjWithSoGroupList(SOSpawnTypeList monsterLists, int spawnMonsterAmount, Transform parent,
        System.Action monsterDead)
    {
        int spawnedCount = 0;
        for (int i = 0; i < 10; i++)
        {
            int spawnTypeIndex = Random.Range(0, monsterLists.monsterSpawnTypeList.Count);
            SoMonsterCount spawnType = monsterLists.monsterSpawnTypeList[spawnTypeIndex];

            List<SOMonsters> meleeMonster = spawnType.meleeMonster;
            List<SOMonsters> rangedMonster = spawnType.rangedMonster;
            int meleeCount = spawnType.meleeMonsterCount.Count;
            int rangedCount = spawnType.rangedMonsterCount.Count;

            if (meleeCount == 0) continue;
            spawnedCount += SpawnObjWithSoList(meleeMonster, spawnMonsterAmount - spawnedCount,
                spawnType.meleeMonsterCount[spawnTypeIndex % meleeCount], parent, monsterDead);
            if (spawnedCount >= spawnMonsterAmount) break;

            if (rangedCount == 0) continue;
            spawnedCount += SpawnObjWithSoList(rangedMonster, spawnMonsterAmount - spawnedCount,
                spawnType.rangedMonsterCount[spawnTypeIndex % meleeCount], parent, monsterDead);
            if (spawnedCount >= spawnMonsterAmount) break;

        }
    }

    public int SpawnObjWithSoList(List<SOMonsters> monstersList, int spawnedLimit, int spawnCount, Transform parent, System.Action monsterDead)
    {
        int spawnedCount = 0;
        if (monstersList.Count < 0) return spawnedCount;

        while (true)
        {
            int rand = Random.Range(0, 10);

            SOMonsters obj = monstersList[rand % monstersList.Count];
            SpawnObj(obj.monsters[rand % obj.monsters.Count], parent, monsterDead);

            spawnedCount++;
            if (spawnedCount >= spawnedLimit || spawnedCount >= spawnCount) return spawnedCount;
        }
    }

    void OnDrawGizmos()
    {
        Color gizmoColor = Color.white;
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(radius * 2, 1, radius * 2));
    }
}
