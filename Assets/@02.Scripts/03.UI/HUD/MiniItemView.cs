using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniItemView : MonoBehaviour
{
    [SerializeField] private Image mIcon;
    [SerializeField] private TMP_Text mCount;
    public int ItemCount;

    public void Bind(Sprite icon, int count = 1)
    {
        mIcon.sprite = icon;
        ItemCount = count;
        SetCount(ItemCount);
    }

    public void SetCount(int count)
    {
        ItemCount = count;
        mCount.text = $"x{ItemCount}";
    }

    public void IncreaseCount(int count = 1)
    {
        SetCount(ItemCount + count);
    }

    public void SubTrack(int count = 1)
    {
        if (ItemCount - count < 0)
        {
            SetCount(0);
        }
        else
        {
            SetCount(ItemCount - count);
        }
    }
}