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
    public void Bind(in Achievement achievement, in UserAchievementData userAchievementData)
    {
        if(userAchievementData.IsUnlocked)
        {
            mIconImage.sprite = GetAchievementIconSprite(achievement.Id);
            mTitleText.text = achievement.Title;
            mDescText.text  = achievement.Description;
            mClearDateText.text = $"Clear\n{userAchievementData.ClearDate:yyyy-MM-dd}";
            mClearOverlay.alpha = 1f;
        }
        else
        {
            mTitleText.text = "???";
            mDescText.text  = "???";
            mClearOverlay.alpha = 0f;
        }
    }

    private Sprite GetAchievementIconSprite(string id)
    {
        // DataBase에서 업적 아이콘 Sprite 가져오기
        return null;
    }
}
