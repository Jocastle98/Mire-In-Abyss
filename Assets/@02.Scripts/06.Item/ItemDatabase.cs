using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDatabase : MonoBehaviour
{
    public TextAsset itemCsvFile;//item_list.csv를 인스펙터에 할당
    private string itemCsvFilePath = "Item/item_list"; //Resources 폴더 내 경로

    private Dictionary<string, Item> itemDatabase = new Dictionary<string, Item>();
    public int ItemCount => itemDatabase.Count;
    public List<string> ItemNames => itemDatabase.Keys.ToList();
    
    private void Awake()
    {
        if (itemCsvFile != null) //인스펙터에 item_list.csv가 할당이 되어 있을 때
        {
            LoadItemsFromCSV();
        }
        else                        //인스펙터에 item_list.csv가 없을때
        {
            LoadItemsFromResources();
        }
    }

    #region CSV 파싱관련
    
    /// <summary>
    /// Resource/Item 폴더에서 로드
    /// </summary>
    private void LoadItemsFromResources()
    {
        TextAsset csvAsset = Resources.Load<TextAsset>(itemCsvFilePath);

        if (csvAsset == null)
        {
            Debug.LogError("Resources 폴더에서 CSV 파일을 찾을 수 없음");
            return;
        }
        
        ParseCsvContent(csvAsset);
    }

    /// <summary>
    /// 인스펙터에 할당된 csv파일 사용
    /// </summary>
    private void LoadItemsFromCSV()
    {
        ParseCsvContent(itemCsvFile);
    }

    /// <summary>
    /// csv파일을 줄별로 파싱하는 메서드
    /// </summary>
    /// <param name="csvAsset"></param>
    private void ParseCsvContent(TextAsset csvAsset)
    {
        string[] lines = csvAsset.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) 
        {
            string line = lines[i].Trim();
            if(string.IsNullOrEmpty(line))
                continue;

            Item newItem = ParseItemFromCSV(line);
            if (newItem != null)
            {
                itemDatabase[newItem.name] = newItem;
            }
        }
    }

    /// <summary>
    /// csv 한줄을 파싱하여 Item객체로 변환
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private Item ParseItemFromCSV(string line)
    {
        try
        {
            List<string> values = CSVParser(line);

            if (values.Count < 9)
            {
                Debug.LogWarning($"CSV 라인 형식이 올바르지 않음: {line}");
                return null;
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
            Debug.LogError($"아이템 파싱 중 오류 발생: {e.Message}, 라인: {line}");
            return null;
        }
    }

    /// <summary>
    /// CSV 라인에서 쉼표로 구분된 값 목록으로 변환하고 따옴표로 묶인 내용 처리
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
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

    #endregion

    #region 아이템 조회 관련
    
    /// <summary>
    /// 아이템을 이름을 통해 찾을 때 사용(사용예시 : 도감)
    /// </summary>
    /// <param name="itemName"></param>
    /// <returns></returns>
    public Item GetItemByName(string itemName)
    {
        if (itemDatabase.TryGetValue(itemName, out Item item))
        {
            return item;
        }

        Debug.LogWarning($"아이템을 찾을 수 없음: {itemName}");
        return null;
    }

    /// <summary>
    /// 아이템을 등급을 통해 찾을 때 사용(사용예시 : 도감 등급별 아이템 표시)
    /// </summary>
    /// <param name="tier"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 아이템을 얻을 수 있는 수단을 통해 찾을 때 사용(사용예시 : 아이템 획득처 표시)
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
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
    
    #endregion

    #region 랜덤 아이템 생성 관련
    
    /// <summary>
    /// 몬스터에서 랜덤으로 아이템 나오게
    /// </summary>
    /// <returns></returns>
    public Item GetRandomItemFromMonster()
    {
        return GetRandomItem(item => item.dropSources.Contains("monster"), item => item.dropRateMonster);
    }

    /// <summary>
    /// 상점에서 랜덤으로 아이템 나오게
    /// </summary>
    /// <returns></returns>
    public Item GetRandomItemFromShop()
    {
        return GetRandomItem(item => item.dropSources.Contains("shop"), item => item.dropRateShop);
    }

    /// <summary>
    /// 가중치 기반확률로 필터링된 아이템 중 랜덤 아이템 반환
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="rateSelector"></param>
    /// <returns></returns>
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
    #endregion

    #region 디버깅 관련
    // 디버깅이 필요할 때만 사용하는 메서드
    public void DebugPrintAllItems()
    {
        Debug.Log($"===== 아이템 데이터베이스 총 {itemDatabase.Count}개 아이템 =====");
        foreach (var item in itemDatabase.Values)
        {
            Debug.Log($"이름: {item.name}, 티어: {item.tier}, 효과: {item.effectType}, 값: {item.value}, 유형: {item.valueType}");
            Debug.Log($"드롭소스: {string.Join(", ", item.dropSources)}, 몬스터확률: {item.dropRateMonster}, 상점확률: {item.dropRateShop}");
            Debug.Log($"설명: {item.description}");
            Debug.Log("-------------------------------------");
        }
    }
    #endregion
}