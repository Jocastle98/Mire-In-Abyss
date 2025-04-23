using System.Collections;
using System.Collections.Generic;
using ItemEnums;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string ItemName;
    public string Tier;
    public string Description;
    public string EffectType;
    public float Value;
    public string ValueType;
    public List<string> DropSources = new List<string>();
    public float DropRateMonster;
    public float DropRateShop;

    public override string ToString()
    {
        return $"{ItemName}({Tier}):{Description}";
    }
}
