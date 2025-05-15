using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleAreaEnums;

[CreateAssetMenu(menuName = "BattleAreaSo/SODungeon", fileName = "New SODungeon")]
public class SODungeon : ScriptableObject
{
    public string name;
    public int width;
    public int height;

    //up,right,down,left
    public int[] entranceYPos;
    
    public GameObject dungeonPrefab;
    public DungeonCellType prefabType;
}
