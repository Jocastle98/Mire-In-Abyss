using System.Collections;
using System.Collections.Generic;
using ItemEnums;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string name;
    public ItemTier tier;
    public string description;
    public string effectType;
    public float value;
    public ValueType valueType;
    public string[] dropSources;
    public float dropRateMonster;
    public float dropRateShop;
}
