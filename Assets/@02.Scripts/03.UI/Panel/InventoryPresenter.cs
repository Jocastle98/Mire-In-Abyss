using System.Linq;
using Cysharp.Threading.Tasks;
using Events.Data;
using TMPro;
using UIEnums;
using UnityEngine;
using UnityEngine.UI;
using R3;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TempStat
{
    public string Name { get; set; }
    public float Value { get; set; }
    public TempStat(string name, float value)
    {
        Name = name;
        Value = value;
    }
}

public sealed class InventoryPresenter : TabPresenterBase
{
    [Header("아이템 목록")]
    [SerializeField] RectTransform mItemContent;
    [SerializeField] ItemIconViewWithAmount mItemIconPrefab;
    [SerializeField] Image mLargeIcon;
    [SerializeField] TMP_Text mItemDetailNameText, mItemDetailDescText;

    [Header("스탯 목록")]
    [SerializeField] RectTransform mStatContent;
    [SerializeField] StatView mStatViewPrefab;
    private PlayerStats mPlayerStats;

    private bool mIsInit = false;


    public override void Initialize()
    {
        if (!mIsInit)
        {
            mPlayerStats = TempRefManager.Instance.PlayerStats;
            mItemDetailNameText.text = "";
            mItemDetailDescText.text = "";
            buildItemArea();
            buildStatArea();
            mIsInit = true;
        }
    }

    void buildItemArea()
    {
        var dict = PlayerHub.Instance.Inventory.Items;
        foreach (var kv in dict)
        {
            var v = Instantiate(mItemIconPrefab, mItemContent);
            v.Bind(kv.Key, kv.Value, showDetail);
        }

        // 첫 아이템 미리 표시
        if (dict.Count > 0)
        {
            showDetail(dict.Keys.First());
        }
    }

    void buildStatArea()
    {
        //TODO: 스탯 목록 가져오기
        //var stats = PlayerHub.Instance.Stats;

        // 임시 스탯 목록 생성
        var stats = new List<TempStat>
        {
            new TempStat("최대 체력", mPlayerStats.GetMaxHP()),
            new TempStat("현재 체력", mPlayerStats.GetCurrentHP()),
            new TempStat("이동 속도", mPlayerStats.GetMoveSpeed()),
            new TempStat("공격력", mPlayerStats.GetAttackPower()),
            new TempStat("방어력", mPlayerStats.GetDefence()),
            new TempStat("치명타 확률", mPlayerStats.GetCritChance() * 100),
            new TempStat("데미지 감소", mPlayerStats.GetDamageReduction() * 100),
            new TempStat("흡혈 퍼센트", mPlayerStats.GetLifeStealPercentage() * 100),
        };

        foreach (var stat in stats)
        {
            var v = Instantiate(mStatViewPrefab, mStatContent);
            v.Bind(stat.Name, stat.Value);
        }
    }

    void showDetail(int id)
    {
        mLargeIcon.sprite = GameDB.Instance.SpriteCache.GetSprite(SpriteType.Item, id);
        var rec = GameDB.Instance.ItemDatabase.GetItemByID(id);

        mItemDetailNameText.text = rec.ItemName;
        mItemDetailDescText.text = rec.Description;
    }
}
