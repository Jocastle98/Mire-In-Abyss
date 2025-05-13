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
}

public sealed class InventoryPresenter : MonoBehaviour
{
    [Header("아이템 목록")]
    [SerializeField] RectTransform mItemContent;
    [SerializeField] ItemIconViewWithAmount mItemIconPrefab;
    [SerializeField] Image mLargeIcon;
    [SerializeField] TMP_Text mItemDetailNameText, mItemDetailDescText;

    [Header("스탯 목록")]
    [SerializeField] RectTransform mStatContent;
    [SerializeField] StatView mStatViewPrefab;

    private bool mIsInit = false;


    void OnEnable()
    {
        if (!mIsInit)
        {
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
            new TempStat { Name = "체력", Value = 100 },
            new TempStat { Name = "공격력", Value = 10 },
            new TempStat { Name = "방어력", Value = 5 },
            new TempStat { Name = "치명타 확률", Value = 0.1f },
            new TempStat { Name = "치명타 데미지", Value = 1.5f },
            new TempStat { Name = "생명력 흡수", Value = 0.1f },
            new TempStat { Name = "이동속도", Value = 1.5f },
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
