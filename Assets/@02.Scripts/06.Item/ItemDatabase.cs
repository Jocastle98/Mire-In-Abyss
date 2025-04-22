using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemEnums;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDatabase : MonoBehaviour
{
    public TextAsset itemCsvFile;

    private Dictionary<string, Item> itemDatabase = new Dictionary<string, Item>();

    private void Awake()
    {
        if (itemCsvFile != null)
        {
            LoadItemsFromCSV();
        }
        else
        {
            Debug.LogError("csv파일 할당 안됨");
        }
    }

    private void LoadItemsFromCSV()
    {
        string[] lines = itemCsvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if(string.IsNullOrEmpty(line))
                continue;

            Item newItem = ParseItemFromCSV(line);
            if (newItem != null)
            {
                itemDatabase[newItem.name] = newItem;
                Debug.Log($"아이템 로드 : {newItem}");
            }
        }

        Debug.Log($"{itemDatabase.Count}개의 아이템 로드");
    }

    private Item ParseItemFromCSV(string line)
    {
        try
        {
            List<string> values = CSVParser(line);

            if (values.Count < 9)
            {
                Debug.LogWarning($"CSV 라인 형식이 올바르지 않음 {line}");
            }

            Item item = new Item
            {
                name = values[0],
                tier = values[1],
                description = values[2],
                effectType = values[3],
                value = float.Parse(values[4]),
                valueType = values[5]
            };

            string dropSourcesStr = values[6].Replace(" ", "");
            if (!string.IsNullOrEmpty(dropSourcesStr))
            {
                string[] sources = dropSourcesStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var source in sources)
                {
                    item.dropSources.Add(source);
                }

            }

            item.dropRateMonster = float.Parse(values[7]);
            item.dropRateShop = float.Parse(values[8]);

            return item;
        }
        catch (Exception e)
        {
            Debug.LogError($"아이템 파싱 중 오류 발생 {e.Message} Line {line}");
            return null;
        }
    }

    private List<string> CSVParser(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentValue = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentValue);
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }
        
        result.Add(currentValue);
        return result;
    }

    public Item GetItemByName(string itemName)
    {
        if (itemDatabase.TryGetValue(itemName, out Item item))
        {
            return item;
        }

        Debug.LogWarning($"아이템을 찾을 수 없음 {itemName}");
        return null;
    }

    public List<Item> GetItemsByTier(string tier)
    {
        List<Item> result = new List<Item>();

        foreach (var item in itemDatabase.Values)
        {
            if (item.tier.Equals(tier, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public List<Item> GetItemsByDropSource(string source)
    {
        List<Item> result = new List<Item>();
        foreach (var item in itemDatabase.Values)
        {
            if (item.dropSources.Contains(source))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public Item GetRandomItemFromMonster()
    {
        return GetRandomItem(item => item.dropSources.Contains("monster"), item => item.dropRateMonster);
    }

    public Item GetRandomItemFromShop()
    {
        return GetRandomItem(item => item.dropSources.Contains("shop"), item => item.dropRateShop);
    }

    private Item GetRandomItem(Func<Item, bool> filter, Func<Item, float> rateSelector)
    {
        List<Item> filteredItems = new List<Item>();
        List<float> dropRates = new List<float>();
        float totalWeight = 0f;

        foreach (var item in itemDatabase.Values)
        {
            if (filter(item))
            {
                filteredItems.Add(item);
                float rate = rateSelector(item);
                dropRates.Add(rate);
                totalWeight += rate;
            }
        }

        if (filteredItems.Count == 0)
            return null;

        float randomValue = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        for (int i = 0; i < filteredItems.Count; i++)
        {
            currentSum += dropRates[i];
            if (randomValue <= currentSum)
            {
                return filteredItems[i];
            }
        }

        return filteredItems[filteredItems.Count - 1];
    }
}
