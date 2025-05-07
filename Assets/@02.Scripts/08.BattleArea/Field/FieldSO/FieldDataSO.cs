using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "BattleAreaSo/FieldDataSO", fileName = "New FieldDataSo")]
public class FieldDataSO : ScriptableObject
{
    //스폰 수
    [Header("몬스터 스폰 관리")] 
    public int playTime = 30;
    public int mMinSpawnCount = 5;
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
    [Space(10)] [Header("보물상자 스폰 관리")]
    public int treasureSpawnAmount = 10;
    public int treasureDistance = 150;
    [Range(5, 25)] public int poissonResearchLimit = 10;

    //몬스터 프리팹
    [Space(10)] [Header("프리팹")] 
    public SOSpawnTypeList commonMonsters;
    public SOSpawnTypeList eliteMonsters;
    public SOMonsters bossMonsters;
    
    [Space(10)] 
    public GameObject battleFields;

    //보물상자 프리팹
    [Space(10)] 
    public SOMonsters treasures;

    //필드의 자식 오브젝트들이 이름과 일치시킬 변수
    [Space(10)] 
    public string playerSpawnZoneName = "PlayerSpawnZone";
    public string bossSpawnZoneName = "BossSpawnZone";
    public string environmentName = "Environment";
    public string navGroundsName = "NavGrounds";
    //public string monsterSpawnZoneName = "MonsterSpawnZone";
}
