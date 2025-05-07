using System.Linq;
using Cysharp.Threading.Tasks;
using Events.Common;
using TMPro;
using UIEnums;
using UnityEngine;
using UnityEngine.UI;
using R3;
using UnityEngine.EventSystems;

public sealed class ItemCodexPresenter : MonoBehaviour
{
    [SerializeField] RectTransform mContent;
    [SerializeField] ItemIconView mIconPrefab;
    [SerializeField] Image mLargeIcon;
    [SerializeField] TMP_Text mNameText, mDescText;

    void Start()
    {
        R3EventBus.Instance.Receive<Preloaded>()
            .Subscribe(OnPreloaded)
            .AddTo(this);
    }

    void OnPreloaded(Preloaded e)
    {
        if (e.IsPreloaded)
        {
            buildGrid();
        }
    }

    void buildGrid()
    {
        var dict = SpriteCache.Instance.GetDict(SpriteType.Item);
        foreach (var rec in dict)
        {
            var v = Instantiate(mIconPrefab, mContent);
            v.Bind(rec.Key, rec.Value, ShowDetail);
        }

        // 첫 아이템 미리 표시
        if (dict.Count > 0)
        {
            ShowDetail(dict.Keys.First());
        }
    }

    void ShowDetail(int id)
    {
        mLargeIcon.sprite = SpriteCache.Instance.GetSprite(SpriteType.Item, id);

        //TODO: 아이템 정보 가져오기
        //var rec = GameDatabase.Instance.Item.Get(id);
        // 임시 아이템 정보 생성
        var rec = new Item
        {
            ID = id,
            ItemName = "아이템 이름",
            Description = "아이템 설명",
        };
        
        mNameText.text    = rec.ItemName;
        mDescText.text    = rec.Description;
    }
}
