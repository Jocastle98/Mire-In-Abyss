using AchievementStructs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AchievementCardView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text     mTitleText;
    [SerializeField] TMP_Text     mDescText;
    [SerializeField] Image        mIconImage;
    [SerializeField] CanvasGroup  mClearOverlay;
    [SerializeField] TMP_Text     mClearDateText;


    /* ────────── Bind API ────────── */
    public void Bind(in TempAchievementInfo info)
    {
        if(info.IsClear)
        {
            mIconImage.sprite = GetAchievementIconSprite(info.Id);
            mTitleText.text = info.Title;
            mDescText.text  = info.Description;
            mClearDateText.text = $"Clear\n{info.ClearDate:yyyy-MM-dd}";
            mClearOverlay.alpha = 1f;
        }
        else
        {
            mTitleText.text = "???";
            mDescText.text  = "???";
            mClearOverlay.alpha = 0f;
        }
    }

    private Sprite GetAchievementIconSprite(int id)
    {
        // DataBase에서 업적 아이콘 Sprite 가져오기
        return null;
    }
}
