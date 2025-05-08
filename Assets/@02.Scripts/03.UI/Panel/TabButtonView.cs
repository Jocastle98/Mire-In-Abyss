using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public sealed class TabButtonView : MonoBehaviour,
                                    IPointerEnterHandler,
                                    IPointerExitHandler
{
    [SerializeField] Image mBG;
    [SerializeField] TMP_Text mLabel;
    [SerializeField] Image mUnderline;
    [SerializeField] bool mbUseBG = false;

    static readonly Color COLOR_SELECTED = new(1f, 0.88f, 0.5f, 1f);
    static readonly Color COLOR_UNSELECT = new(1f, 1f, 1f, 0.3f);
    bool mbSelected;
    bool mbHover;       // 커서가 올라와 있는지

    /* ─────────── API ─────────── */
    public void SetSelected(bool sel)
    {
        mbSelected = sel;
        RefreshVisual();
    }

    /* ─────────── IPointer ─────────── */
    public void OnPointerEnter(PointerEventData _) { mbHover = true; RefreshVisual(); }
    public void OnPointerExit(PointerEventData _) { mbHover = false; RefreshVisual(); }

    /* ─────────── Helpers ─────────── */
    void RefreshVisual()
    {
        mLabel.color = mbSelected ? COLOR_SELECTED : COLOR_UNSELECT;

        if (mbUseBG)
        {
            mBG.color = mbSelected ? COLOR_SELECTED : Color.clear;
        }

        var underlineColor = mbSelected ? COLOR_SELECTED : (mbHover ? COLOR_UNSELECT : Color.clear);
        mUnderline.color = underlineColor;
    }
}
