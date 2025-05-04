using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleAreaEnums;

[CreateAssetMenu(menuName = "BattleAreaSo/SODungeon Tile", fileName = "New SODungeon Tile")]
public class SODungeonTile : ScriptableObject
{
    public string name;
    
    public List<GameObject> floorPrefabs;
    public List<GameObject> wallPrefabs;
    public List<GameObject> entrancePrefabs;
    public List<GameObject> trapPrefabs;
    
    public DungeonCellType prefabType;
}
