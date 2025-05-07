using TMPro;
using UIEnums;
using UnityEngine;
using UnityEngine.UI;

public class MiniItemView : MonoBehaviour
{
    [SerializeField] private Image mIcon;
    [SerializeField] private TMP_Text mCount;
    public int ItemCount;

    public void Bind(int itemID, int total = 1)
    {
        mIcon.sprite = SpriteCache.Instance.GetSprite(SpriteType.Item, itemID);
        ItemCount = total;
        SetCount(ItemCount);
    }

    public void SetCount(int total)
    {
        ItemCount = total;
        mCount.text = $"x{ItemCount}";
    }

    public void IncreaseCount(int addedAmount = 1)
    {
        SetCount(ItemCount + addedAmount);
    }

    public void SubTrack(int removedAmount = 1)
    {
        if (ItemCount - removedAmount < 0)
        {
            SetCount(0);
        }
        else
        {
            SetCount(ItemCount - removedAmount);
        }
    }
}