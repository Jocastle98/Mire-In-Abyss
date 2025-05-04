using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName = "BattleAreaSo/SOMonster", fileName = "New SOMonster")]
public class SOMonsters : ScriptableObject
{
    public List<GameObject> monsters = new List<GameObject>();
}
