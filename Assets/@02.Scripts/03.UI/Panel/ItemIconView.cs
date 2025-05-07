using UIEnums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ItemIconView : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image mIcon;
    System.Action<int> mOnHover; 
    int mItemID;

    public void Bind(int itemID, System.Action<int> onHover)
    {
        mItemID = itemID;
        var item = GameDB.Instance.SpriteCache.GetSprite(SpriteType.Item, itemID);
        mIcon.sprite = item;
    }

    public void Bind(int itemID, Sprite sprite, System.Action<int> onHover)
    {
        mItemID = itemID;
        mIcon.sprite = sprite;
        mOnHover = onHover;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        mOnHover?.Invoke(mItemID);
    }
}

