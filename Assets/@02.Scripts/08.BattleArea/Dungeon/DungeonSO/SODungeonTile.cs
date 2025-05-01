using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleAreaEnum;

[CreateAssetMenu(menuName = "CreateSO/SODungeon Tile", fileName = "New SODungeon Tile")]
public class SODungeonTile : ScriptableObject
{
    public string name;
    
    public List<GameObject> floorPrefabs;
    public List<GameObject> wallPrefabs;
    public List<GameObject> entrancePrefabs;
    public List<GameObject> trapPrefabs;
    
    public Vector3 floorPrefabsScale;
    public Vector3 wallPrefabsScale;
    public Vector3 entrancePrefabsScale;
    public Vector3 trapPrefabsScale;
    
    public DungeonCellType prefabType;
}
