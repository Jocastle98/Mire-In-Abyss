using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateSO/SOSpawnList", fileName = "New SOSpawnTypeList")]
public class SOSpawnTypeList : ScriptableObject
{
    public List<SoMonsterCount> monsterSpawnTypeList = new List<SoMonsterCount>();
}

[System.Serializable]
public class SoMonsterCount
{
    public List<SOMonsters> meleeMonster;
    public List<SOMonsters> rangedMonster;
    public List<int> meleeMonsterCount;
    public List<int> rangedMonsterCount;
}
