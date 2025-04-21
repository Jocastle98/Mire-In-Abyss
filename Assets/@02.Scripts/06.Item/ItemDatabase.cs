using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemEnums;
using UnityEngine;
using UnityEngine.SceneManagement;
using ValueType = ItemEnums.ValueType;

public class ItemDatabase : Singleton<ItemDatabase>
{
    public List<ItemData> Items { get; private set; } = new();
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

    private void Start()
    {
        LoadCSV();
    }

    private void LoadCSV()
    {
        TextAsset itemCSV = Resources.Load<TextAsset>("Items/item_list");
        var lines = itemCSV.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if(string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = lines[i].Trim().Split(',').Select(cell=>cell.Trim()).ToArray();

            var item = new ItemData
            {
                name = row[0],
                tier = Enum.Parse<ItemTier>(row[1], true),
                description = row[2],
                effectType = row[3],
                value = float.Parse(row[4]),
                valueType = Enum.Parse<ValueType>(row[5], true),
                dropSources = row[6].Split(new[] { "", "," }, StringSplitOptions.RemoveEmptyEntries),
                dropRateMonster = float.Parse(row[7]),
                dropRateShop = float.Parse(row[8])
            };
            
            Items.Add(item);
        }
    }
}
