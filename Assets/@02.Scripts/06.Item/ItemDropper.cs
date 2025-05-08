using System;
using System.Collections;
using System.Collections.Generic;
using EnemyEnums;
using ItemEnums;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDropper : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public GameObject itemPrefab;
        [Range(0, 1)] public float dropChance = 1f; //몬스터 자체에서 아이템을 떨어뜨릴 확률
    }

    [System.Serializable]
    public class TierDropRate
    {
        public ItemTier tier;
        [Range(0, 1)] public float dropRate = 0.3f;
    }

    [Header("아이템 드랍 설정")] 
    [SerializeField] private float itemDropChance = 0.5f;

    [Header("적 타입별 등급 드랍 확률")] 
    [SerializeField] private List<TierDropRate> commonEnemyDropRates = new List<TierDropRate>();
    [SerializeField] private List<TierDropRate> eliteEnemyDropRates = new List<TierDropRate>();
    [SerializeField] private List<TierDropRate> bossEnemyDropRates = new List<TierDropRate>();

    [Header("등급별 아이템 목록")] 
    [SerializeField] private List<DropItem> commonItems = new List<DropItem>();
    [SerializeField] private List<DropItem> specialItems = new List<DropItem>();
    [SerializeField] private List<DropItem> epicItems = new List<DropItem>();

    [Header("아이템 데이터베이스 참조")] 
    [SerializeField] private ItemDatabase itemDatabase;

    private EnemyType currentEnemyType;


    private void Start()
    {
        if(commonEnemyDropRates.Count == 0) SetupDefaultRates(ref commonEnemyDropRates, EnemyType.Common);
        if(eliteEnemyDropRates.Count == 0) SetupDefaultRates(ref eliteEnemyDropRates, EnemyType.Elite);
        if(bossEnemyDropRates.Count == 0) SetupDefaultRates(ref bossEnemyDropRates, EnemyType.Boss);

        if (itemDatabase == null)
        {
            itemDatabase = GameObject.FindObjectOfType<ItemDatabase>(); //임시 코드
        }
    }

    public void DropItemOnDeadth()
    {
        if (Random.value > itemDropChance) return;

        ItemTier selectedTier = DetermineItemTier();

        GameObject itemPrefab = SelectItemFromTier(selectedTier);

        if (itemPrefab != null)
        {
            ItemDrop(itemPrefab);
        }
    }

    private ItemTier DetermineItemTier()
    {
        float random = Random.value;
        float cumulativeProbability = 0f;

        List<TierDropRate> tierRates = null;

        switch (currentEnemyType)
        {
            case EnemyType.Common:
                tierRates = commonEnemyDropRates;
                break;
            case EnemyType.Elite:
                tierRates = eliteEnemyDropRates;
                break;
            case EnemyType.Boss:
                tierRates = bossEnemyDropRates;
                break;
        }

        foreach (var tierRate in tierRates)
        {
            cumulativeProbability += tierRate.dropRate;
            if (random <= cumulativeProbability)
            {
                return tierRate.tier;
            }
        }

        return ItemTier.Common;
    }

    private GameObject SelectItemFromTier(ItemTier tier)
    {
        List<DropItem> itemsOfTier = null;

        switch (tier)
        {
            case ItemTier.Common:
                itemsOfTier = commonItems;
                break;
            case ItemTier.Special:
                itemsOfTier = specialItems;
                break;
            case ItemTier.Epic:
                itemsOfTier = epicItems;
                break;
        }

        if (itemsOfTier == null || itemsOfTier.Count == 0)
        {
            if (itemDatabase != null)
            {
                return GetRandomItemPrefabFromDatabase(tier);
            }

            return null;
        }

        float totalWeight = 0f;
        foreach (var item in itemsOfTier)
        {
            totalWeight += item.dropChance;
        }

        float random = Random.value * totalWeight;
        float weight = 0f;

        foreach (var item in itemsOfTier)
        {
            weight += item.dropChance;
            if (random <= weight)
            {
                return item.itemPrefab;
            }
        }

        return itemsOfTier.Count > 0 ? itemsOfTier[0].itemPrefab : null;
    }

    private GameObject GetRandomItemPrefabFromDatabase(ItemTier tier)
    {
        if (itemDatabase == null) return null;

        List<Item> itemsOfTier = itemDatabase.GetItemsByTier(tier.ToString());

        if (itemsOfTier == null || itemsOfTier.Count == 0) return null;

        Item selectedItem = itemsOfTier[Random.Range(0, itemsOfTier.Count)];

        string prefabPath = $"Items/Item_{selectedItem.ID}";
        GameObject prefab = Resources.Load<GameObject>(prefabPath);

        return prefab;
    }
    
    private void ItemDrop(GameObject itemPrefab)
    {
        if (itemPrefab == null) return;

        Vector3 position = transform.position + Vector3.up * 0.5f;
        GameObject droppedItem = Instantiate(itemPrefab, position, Quaternion.identity);

        StartCoroutine(AnimateItemDrop(droppedItem, position));
    }

    private IEnumerator AnimateItemDrop(GameObject item, Vector3 startPos)
    {
        float duration = 0.5f;
        float height = 1.5f;
        float elapsed = 0;

        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / (duration / 2);

            Vector3 newPos = startPos;
            newPos.y = startPos.y + Mathf.Sin(ratio * Mathf.PI * 0.5f) * height;

            item.transform.position = newPos;
            yield return null;
        }

        Vector3 peakPos = item.transform.position;
        elapsed = 0;

        while (elapsed < duration* 0.5f)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / (duration * 0.5f) ;

            Vector3 newPos = peakPos;
            newPos.y = peakPos.y - Mathf.Sin(ratio * Mathf.PI * 0.5f) * height;

            item.transform.position = newPos;
            yield return null;
        }

        Vector3 finalPos = item.transform.position;
        RaycastHit hit;
        if (Physics.Raycast(finalPos + Vector3.up, Vector3.down, out hit, 2f))
        {
            item.transform.position = new Vector3(finalPos.x, hit.point.y + 0.05f, finalPos.z);
        }
    }

    public void SetEnemyType(EnemyType enemyType)
    {
        currentEnemyType = enemyType;
    }

    private void SetupDefaultRates(ref List<TierDropRate> rates, EnemyType enemyType)
    {
        rates.Clear();

        switch (enemyType)
        {
            case EnemyType.Common:
                rates.Add(new TierDropRate { tier = ItemTier.Common, dropRate = 0.7f });
                rates.Add(new TierDropRate { tier = ItemTier.Special, dropRate = 0.2f });
                rates.Add(new TierDropRate { tier = ItemTier.Epic, dropRate = 0.1f });
                break;
                
            case EnemyType.Elite:
                rates.Add(new TierDropRate { tier = ItemTier.Common, dropRate = 0.4f });
                rates.Add(new TierDropRate { tier = ItemTier.Special, dropRate = 0.4f });
                rates.Add(new TierDropRate { tier = ItemTier.Epic, dropRate = 0.2f });
                break;
                
            case EnemyType.Boss:
                rates.Add(new TierDropRate { tier = ItemTier.Common, dropRate = 0.2f });
                rates.Add(new TierDropRate { tier = ItemTier.Special, dropRate = 0.5f });
                rates.Add(new TierDropRate { tier = ItemTier.Epic, dropRate = 0.3f });
                break;
        }
    }
    
    public void SetupDefaultDrops(EnemyType enemyType)
    {
        currentEnemyType = enemyType;
        
        // 기존 코드와의 호환성 유지를 위해 필요한 경우 추가 설정
        switch (enemyType)
        {
            case EnemyType.Common:
                itemDropChance = 0.3f; // 일반 몬스터는 30% 확률로 아이템 드랍
                break;
            case EnemyType.Elite:
                itemDropChance = 0.6f; // 엘리트 몬스터는 60% 확률로 아이템 드랍
                break;
            case EnemyType.Boss:
                itemDropChance = 1.0f; // 보스는 100% 확률로 아이템 드랍
                break;
        }
    }
}
