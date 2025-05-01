using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using BattleAreaEnums;

public class SpawnController : MonoBehaviour
{
    public float radius;
    public SpawnType spawnType = SpawnType.None;
    
    [Space(10)]
    [InspectorName("Common")]
    public SOMonsters mageList;
    public SOMonsters rangerList;
    public SOMonsters warriorList;
    
    [Space(10)]
    [InspectorName("Elite")]
    public SOMonsters eliteMageList;
    public SOMonsters eliteRangerList;
    public SOMonsters eliteWarriorList;
    
    [Space(10)]
    [InspectorName("Boss")]
    public SOMonsters bossList;

    private Color gizmoColor = Color.white;

    public void SpawnObj(GameObject obj, GameObject parent)
    {
        Vector3 randomPointOnCircle = Random.insideUnitSphere;
        randomPointOnCircle.Normalize(); // 방향만 남김 (길이 1)
        randomPointOnCircle *= Random.Range(5,radius);   // 원하는 반지름으로 스케일 조정
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position + randomPointOnCircle, Vector3.down, out hit))
        {
            GameObject spawnObj = Instantiate(obj, hit.point, Quaternion.identity);
            spawnObj.transform.parent = parent.transform;
        }
    }

    public void SpawnByType(int eliteMaxChance,int levelDesign,GameObject parent)
    {
        int unique = Mathf.Min(levelDesign / eliteMaxChance, eliteMaxChance);
        GameObject monsterPrefab = null;
        switch (spawnType)
        {
            case SpawnType.None:
                //일반 , 유니크 확률 소환
                spawnType = 
                    (SpawnType)Random.Range(1, System.Enum.GetValues(typeof(SpawnType)).Length - 1);
                SpawnByType(unique, levelDesign, parent);
                spawnType = SpawnType.None;
                return;
            case SpawnType.Warrior:
                monsterPrefab = Random.Range(0, 100) > unique
                    ? warriorList.monsters[Random.Range(0, warriorList.monsters.Count)]
                    : eliteWarriorList.monsters[Random.Range(0, eliteWarriorList.monsters.Count)];
                break;
            case SpawnType.Ranger:
                monsterPrefab = Random.Range(0, 100) > unique
                    ? rangerList.monsters[Random.Range(0, rangerList.monsters.Count)]
                    : eliteRangerList.monsters[Random.Range(0, eliteRangerList.monsters.Count)];
                break;
            case SpawnType.Mage:
                monsterPrefab = Random.Range(0, 100) > unique
                    ? mageList.monsters[Random.Range(0, mageList.monsters.Count)]
                    : eliteMageList.monsters[Random.Range(0, eliteMageList.monsters.Count)];
                break;
            case SpawnType.Boss:
                monsterPrefab = bossList.monsters[Random.Range(0, bossList.monsters.Count)];
                break;
        }

        SpawnObj(monsterPrefab, parent);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(radius * 2, 1, radius * 2));
    }
}
