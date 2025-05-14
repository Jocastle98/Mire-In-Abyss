using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "BattleAreaSo/SODungeonList", fileName = "New SODungeonList")]
public class SODungeonList : ScriptableObject
{
    public SODungeonTile dungeonTileSO;
    
    [Space(10)]
    public SODungeon safeDungeon;
    public SODungeon shopRoom;
    public List<SODungeon> monsterDungeonList = new List<SODungeon>();
    public List<SODungeon> eventDungeonList = new List<SODungeon>();
    public List<SODungeon> bossDungeonList = new List<SODungeon>();
}
