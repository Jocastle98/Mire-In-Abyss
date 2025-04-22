using System.Collections;
using System.Collections.Generic;
using ItemEnums;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string name;
    public string tier;
    public string description;
    public string effectType;
    public float value;
    public string valueType;
    public List<string> dropSources = new List<string>();
    public float dropRateMonster;
    public float dropRateShop;

    public override string ToString()
    {
        return $"{name}({tier}):{description}";
    }
}
