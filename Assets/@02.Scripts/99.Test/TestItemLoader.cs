using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestItemLoader : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("총 아이템 수: " + ItemDatabase.Instance.Items.Count);

        foreach (var item in ItemDatabase.Instance.Items)
        {
            Debug.Log($"이름: {item.name}, 티어: {item.tier}, 효과: {item.effectType}, 값: {item.value}, 타입: {item.valueType}");
        }

        var dropItem = GetRandomDropFromMonster();
        if (dropItem != null)
        {
            Debug.Log($"랜덤 드랍 아이템: {dropItem.name} ({dropItem.tier})");
        }
        else
        {
            Debug.Log("드랍될 아이템 없음!");
        }
    }
    
    ItemData GetRandomDropFromMonster()
    {
        var pool = ItemDatabase.Instance.Items
            .FindAll(i => System.Array.Exists(i.dropSources, s => s == "monster"))
            .FindAll(i => Random.value <= i.dropRateMonster);

        return pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : null;
    }
}
