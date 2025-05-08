using TMPro;
using UIEnums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ItemIconViewWithAmount : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image mIcon;
    [SerializeField] private TMP_Text mAmount;
    System.Action<int> mOnHover; 
    int mItemID;

    public void Bind(int itemID, int amount, System.Action<int> onHover)
    {
        mItemID = itemID;
        mAmount.text = $"x{amount}";
        var item = GameDB.Instance.SpriteCache.GetSprite(SpriteType.Item, itemID);
        mIcon.sprite = item;
        mOnHover = onHover;
    }

    public void Bind(int itemID, Sprite sprite, int amount, System.Action<int> onHover)
    {
        mItemID = itemID;
        mAmount.text = $"x{amount}";
        mIcon.sprite = sprite;
        mOnHover = onHover;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        mOnHover?.Invoke(mItemID);
    }
}

