using System;
using System.Collections;
using System.Collections.Generic;
using EnemyEnums;
using ItemEnums;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 적이 죽었을 때 아이템을 생성하고 드랍하는 기능을 담당하는 클래스
/// </summary>
public class ItemDropper : MonoBehaviour
{
    /// <summary>
    /// 개별 아이템의 드랍 정보를 담는 내부 클래스
    /// </summary>
    [System.Serializable]
    public class DropItem
    {
        public GameObject itemPrefab;                   //드랍될 아이템 프리팹
    }

    /// <summary>
    /// 아이템 등급별 드랍 확률을 정의하는 내부 클래스
    /// </summary>
    [System.Serializable]
    public class TierDropRate
    {
        public ItemTier tier;                       //아이템 등급 (common, special, epic)
        [Range(0, 1)] public float dropRate = 0.3f; //해당 등급이 선택될 확률
    }

    [Header("아이템 드랍 설정")] 
    [SerializeField] private float itemDropChance = 0.5f; //몬스터가 아이템을 드랍할 기본 확률

    [Header("적 타입별 등급 드랍 확률")] 
    [SerializeField] private List<TierDropRate> commonEnemyDropRates = new List<TierDropRate>();    //일반 몬스터의 등급별 드랍 확률
    [SerializeField] private List<TierDropRate> eliteEnemyDropRates = new List<TierDropRate>();     //엘리트 몬스터의 등급별 드랍 확률
    [SerializeField] private List<TierDropRate> bossEnemyDropRates = new List<TierDropRate>();      //보스 몬스터의 등급별 드랍 확률

    [Header("등급별 아이템 목록")] 
    [SerializeField] private List<DropItem> commonItems = new List<DropItem>();     //일반 등급 아이템 목록
    [SerializeField] private List<DropItem> specialItems = new List<DropItem>();    //스페셜 등급 아이템 목록
    [SerializeField] private List<DropItem> epicItems = new List<DropItem>();       //에픽 등급 아이템 목록

    [Header("아이템 데이터베이스 참조")] 
    [SerializeField] private ItemDatabase itemDatabase;

    private EnemyType currentEnemyType; //현재 몬스터 타입(common, elite, boss)
    private EnemyBTController enemyBtController;


    /// <summary>
    /// 컴포넌트 초기화 및 기본 드랍 확률 설정
    /// </summary>
    private void Start()
    {
        enemyBtController = GetComponent<EnemyBTController>();
        SetEnemyType(enemyBtController.EnemyType);
        SetupDefaultDrops(currentEnemyType);
        
        //인스펙터에 등급별 드랍 확률이 설정되지 않았다면 기본값 설정
        if(commonEnemyDropRates.Count == 0) SetupDefaultRates(ref commonEnemyDropRates, EnemyType.Common);
        if(eliteEnemyDropRates.Count == 0) SetupDefaultRates(ref eliteEnemyDropRates, EnemyType.Elite);
        if(bossEnemyDropRates.Count == 0) SetupDefaultRates(ref bossEnemyDropRates, EnemyType.Boss);

        if (itemDatabase == null)
        {
            itemDatabase = GameObject.FindObjectOfType<ItemDatabase>(); //임시 코드
        }
    }

    /// <summary>
    /// 몬스터가 죽을 때 호출되는 아이템 드랍 메서드
    /// </summary>
    public void DropItemOnDeadth()
    {
        //1. 아이템을 드랍할지 여부 결정
        if (Random.value > itemDropChance) return;

        //2. 몬스터 타입에 따라 어떤 등급의 아이템을 드랍할지 결정
        ItemTier selectedTier = DetermineItemTier();

        //3. 선택된 등급에서 특정 아이템 선택
        GameObject itemPrefab = SelectItemFromTier(selectedTier);

        //4. 선택된 아이템이 있으면 드랍 실행
        if (itemPrefab != null)
        {
            ItemDrop(itemPrefab);
        }
    }

    /// <summary>
    /// 몬스터 타입에 따라 아이템 등급 결정
    /// </summary>
    /// <returns>선택된 아이템 등급</returns>
    private ItemTier DetermineItemTier()
    {
        float random = Random.value;
        float cumulativeProbability = 0f;

        //현재 몬스터 타입에 맞는 등급 확률 테이블 선택
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

        //누적 확률 계산으로 등급 선택
        foreach (var tierRate in tierRates)
        {
            cumulativeProbability += tierRate.dropRate;
            if (random <= cumulativeProbability)
            {
                return tierRate.tier;
            }
        }

        //기본값
        return ItemTier.Common;
    }

    /// <summary>
    /// 선택된 등급 내에서 특정 아이템을 동일한 확률로 선택
    /// </summary>
    /// <param name="tier"></param>
    /// <returns></returns>
    private GameObject SelectItemFromTier(ItemTier tier)
    {
        //선택된 등급에 해당하는 아이템 목록 가져오기
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

        //해당 등급의 아이템이 없을 경우 데이터베이스에서 찾기
        if (itemsOfTier == null || itemsOfTier.Count == 0)
        {
            if (itemDatabase != null)
            {
                return GetRandomItemPrefabFromDatabase(tier);
            }

            return null;
        }

        //동일한 확률로 아이템 선택
        int randomIndex = Random.Range(0, itemsOfTier.Count);
        return itemsOfTier[randomIndex].itemPrefab;
    }

    /// <summary>
    /// 아이템 데이터베이스에서 특정 등급의 랜덤 아이템 가져오기 (비상용)
    /// </summary>
    /// <param name="tier">찾을 아이템 등급</param>
    /// <returns>아이템 프리팹</returns>
    private GameObject GetRandomItemPrefabFromDatabase(ItemTier tier)
    {
        if (itemDatabase == null) return null;

        //해당 등급의 아이템 목록 가져오기
        List<Item> itemsOfTier = itemDatabase.GetItemsByTier(tier.ToString());
        
        if (itemsOfTier == null || itemsOfTier.Count == 0) return null;

        //랜덤 아이템 선택
        Item selectedItem = itemsOfTier[Random.Range(0, itemsOfTier.Count)];

        //아이템 ID로 프리팹 로드 (Resource에서 가져오게 되어 있는데 해당 프리팹 위치로 바꿔야함, 비상용 메서드기 때문에 사용 안할듯)
        string prefabPath = $"Items/Item_{selectedItem.ID}";
        GameObject prefab = Resources.Load<GameObject>(prefabPath);

        return prefab;
    }
    
    /// <summary>
    /// 선택된 아이템 프리팹을 월드에 생성하고 드랍 애니메이션 시작
    /// </summary>
    /// <param name="itemPrefab">드랍할 아이템 프리팹</param>
    private void ItemDrop(GameObject itemPrefab)
    {
        if (itemPrefab == null) return;

        //몬스터 위치에서 약간 위로 생성
        Vector3 position = transform.position + Vector3.up * 0.5f;
        GameObject droppedItem = Instantiate(itemPrefab, position, Quaternion.identity);

        //드랍 애니메이션
        StartCoroutine(AnimateItemDrop(droppedItem, position));
    }

    /// <summary>
    /// 아이템 드랍 애니메이션 (위로 솟았다가 밑으로 떨어지는 애니메이션)
    /// </summary>
    /// <param name="item">움직일 아이템</param>
    /// <param name="startPos">시작 위치</param>
    /// <returns></returns>
    private IEnumerator AnimateItemDrop(GameObject item, Vector3 startPos)
    {
        float duration = 0.5f;  //애니메이션 총 시간
        float height = 1.5f;    //최대 높이
        float elapsed = 0;      //경과 시간

        //1. 위로 올라가는 애니메이션
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / (duration / 2);

            Vector3 newPos = startPos;
            newPos.y = startPos.y + Mathf.Sin(ratio * Mathf.PI * 0.5f) * height;

            item.transform.position = newPos;
            yield return null;
        }

        //최고점 위치 저장
        Vector3 peakPos = item.transform.position;
        elapsed = 0;

        //2. 아래로 떨어지는 애니메이션
        while (elapsed < duration* 0.5f)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / (duration * 0.5f) ;

            Vector3 newPos = peakPos;
            newPos.y = peakPos.y - Mathf.Sin(ratio * Mathf.PI * 0.5f) * height;

            item.transform.position = newPos;
            yield return null;
        }

        //3. 바닥 위치 조정
        Vector3 finalPos = item.transform.position;
        RaycastHit hit;
        if (Physics.Raycast(finalPos + Vector3.up, Vector3.down, out hit, 2f))
        {
            item.transform.position = new Vector3(finalPos.x, hit.point.y + 0.05f, finalPos.z);
        }
    }

    /// <summary>
    /// 현재 몬스터의 타입 설정
    /// </summary>
    /// <param name="enemyType">설정할 몬스터 타입</param>
    public void SetEnemyType(EnemyType enemyType)
    {
        currentEnemyType = enemyType;
    }

    /// <summary>
    /// 적 타입별 아이템 등급 드랍 확률 기본값 설정
    /// </summary>
    /// <param name="rates"></param>
    /// <param name="enemyType"></param>
    private void SetupDefaultRates(ref List<TierDropRate> rates, EnemyType enemyType)
    {
        rates.Clear();

        switch (enemyType)
        {
            case EnemyType.Common: //일반 몬스터 (커먼 아이템 70%, 스페셜 아이템 20%, 에픽 아이템 10%)
                rates.Add(new TierDropRate { tier = ItemTier.Common, dropRate = 0.7f });
                rates.Add(new TierDropRate { tier = ItemTier.Special, dropRate = 0.2f });
                rates.Add(new TierDropRate { tier = ItemTier.Epic, dropRate = 0.1f });
                break;
                
            case EnemyType.Elite: //엘리트 몬스터 (커먼 아이템 40%, 스페셜 아이템 40%, 에픽 아이템 20%)
                rates.Add(new TierDropRate { tier = ItemTier.Common, dropRate = 0.4f });
                rates.Add(new TierDropRate { tier = ItemTier.Special, dropRate = 0.4f });
                rates.Add(new TierDropRate { tier = ItemTier.Epic, dropRate = 0.2f });
                break;
                
            case EnemyType.Boss: //보스 몬스터 (커먼 아이템 20%, 스페셜 아이템 50%, 에픽 아이템 30%)
                rates.Add(new TierDropRate { tier = ItemTier.Common, dropRate = 0.2f });
                rates.Add(new TierDropRate { tier = ItemTier.Special, dropRate = 0.5f });
                rates.Add(new TierDropRate { tier = ItemTier.Epic, dropRate = 0.3f });
                break;
        }
    }
    
    /// <summary>
    /// 몬스터 타입에 따른 아이템 드랍 확률 설정
    /// </summary>
    /// <param name="enemyType">몬스터 타입</param>
    public void SetupDefaultDrops(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.Common:
                itemDropChance = 0.25f; // 일반 몬스터는 25% 확률로 아이템 드랍
                break;
            case EnemyType.Elite:
                itemDropChance = 0.5f; // 엘리트 몬스터는 50% 확률로 아이템 드랍
                break;
            case EnemyType.Boss:
                itemDropChance = 1.0f; // 보스는 100% 확률로 아이템 드랍
                break;
        }
    }
}
