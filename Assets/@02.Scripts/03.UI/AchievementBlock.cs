using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementBlock : MonoBehaviour
{
    [Header("UI 요소")] 
    [SerializeField] private Image achievementImage;
    [SerializeField] private TextMeshProUGUI achievementTitleText;
    [SerializeField] private TextMeshProUGUI achievementInfoText;

    private string mAchievementId;
    private bool mIsUnlocked;

    public void Initialize(Achievement achievement)
    {
        mAchievementId = achievement.Id;
        mIsUnlocked = achievement.isUnlocked;

        achievementTitleText.text = achievement.Title;

        achievementInfoText.text = achievement.isUnlocked ? achievement.IllustrationComment : achievement.Info;

        if (achievement.isUnlocked)
        {
            //해금 시 이미지 Sprite unlockedSprite 
        }
        else
        {
            //잠김 이미지
        }
    }

    public string GetAchievementId()
    {
        return mAchievementId;
    }

    public bool IsUnlocked()
    {
        return mIsUnlocked;
    }
}
