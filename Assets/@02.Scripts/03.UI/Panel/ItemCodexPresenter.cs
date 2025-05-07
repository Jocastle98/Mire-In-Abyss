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
    [SerializeField] ItemIconView mItemIconPrefab;
    [SerializeField] Image mLargeIcon;
    [SerializeField] TMP_Text mNameText, mDescText;
    [SerializeField] Sprite mSealedSprite;

    void OnEnable()
    {
        buildGrid();
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
        var dict = GameDB.Instance.SpriteCache.GetDict(SpriteType.Item);
        foreach (var rec in dict)
        {
            var v = Instantiate(mItemIconPrefab, mContent);
            if(isUnsealed(rec.Key))
            {
                v.Bind(rec.Key, rec.Value, showDetail);
            }
            else
            {
                v.Bind(rec.Key, mSealedSprite, showDetail);
            }
        }

        // 첫 아이템 미리 표시
        if (dict.Count > 0)
        {
            showDetail(dict.Keys.First());
        }
    }

    void showDetail(int id)
    {
        if (isUnsealed(id))
        {
            mLargeIcon.sprite = GameDB.Instance.SpriteCache.GetSprite(SpriteType.Item, id);
            //TODO: 아이템 정보 가져오기
            //var rec = GameDatabase.Instance.Item.Get(id);
            // 임시 아이템 정보 생성
            var rec = new Item
            {
                ID = id,
                ItemName = "아이템 이름",
                Description = "아이템 설명",
            };

            mNameText.text = rec.ItemName;
            mDescText.text = rec.Description;
        }
        else
        {
            mLargeIcon.sprite = mSealedSprite;
            mNameText.text = "???";
            mDescText.text = "???";
        }
    }

    private bool isUnsealed(int id)
    {
        //TODO: 아이템 획득 경험 여부 확인
        // 임시로 짝수만 true 처리
        return (id & 1) == 0;
    }
}
