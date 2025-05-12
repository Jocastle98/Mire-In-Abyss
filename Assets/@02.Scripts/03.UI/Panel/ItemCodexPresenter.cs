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

public sealed class ItemCodexPresenter : MonoBehaviour
{
    [SerializeField] RectTransform mContent;
    [SerializeField] ItemIconView mItemIconPrefab;
    [SerializeField] Image mLargeIcon;
    [SerializeField] TMP_Text mNameText, mDescText;
    [SerializeField] Sprite mSealedSprite;

    void Start()
    {
        buildGrid();
    }

    void buildGrid()
    {
        var dict = GameDB.Instance.SpriteCache.GetDict(SpriteType.Item);
        foreach (var rec in dict)
        {
            var v = Instantiate(mItemIconPrefab, mContent);
            if(UserData.Instance.GetItemData(rec.Key).IsUnlocked)
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
        if (UserData.Instance.GetItemData(id).IsUnlocked)
        {
            mLargeIcon.sprite = GameDB.Instance.SpriteCache.GetSprite(SpriteType.Item, id);
            //TODO: 아이템 정보 가져오기
            var rec = GameDB.Instance.ItemDatabase.GetItemByID(id);

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
}
