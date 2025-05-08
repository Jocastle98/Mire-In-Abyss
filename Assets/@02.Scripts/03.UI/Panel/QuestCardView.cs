using System;
using System.Collections;
using System.Collections.Generic;
using QuestEnums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 게시판에 표시되는 개별 퀘스트 UI 요소를 관리하는 클래스
/// </summary>
public class QuestCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text mTitleText;   
    [SerializeField] private TMP_Text mQuestTypeText;
    [SerializeField] private TMP_Text mRewardAmountText;
    [SerializeField] private Image mQuestStateImage;
    [SerializeField] private Image mRewardImage;        // 현재 영혼석 sprite 고정

    [Header("State Icons")]
    [SerializeField] private Sprite mInactiveSprite;
    [SerializeField] private Sprite mActiveSprite;
    [SerializeField] private Sprite mCompletedSprite;
    [SerializeField] private Sprite mRewardedSprite;

    private Button mShowDetailButton;
    private string mQuestId;    //퀘스트 고유 아이디
    public string QuestId => mQuestId;  //외부에서 퀘스트 ID에 접근하기 위한 프로퍼티

    private void Awake()
    {
        mShowDetailButton = GetComponent<Button>();
    }

    public void Bind(Quest quest, Action<string> onClickCallback)
    {
        mQuestId = quest.Id;

        mTitleText.text = quest.Title;
        mQuestTypeText.text = quest.Description;
        mRewardAmountText.text = $"x{quest.RewardSoul}";
        SetQuestState(QuestOfferService.Instance.GetQuestState(quest.Id));

        mShowDetailButton.onClick.AddListener(() => onClickCallback?.Invoke(quest.Id));
    }

    public void Bind(String questId, Action<string> onClickCallback)
    {
        Bind(GameDB.Instance.QuestDatabase.GetQuestById(questId), onClickCallback);
    }

    /// <summary>
    /// 퀘스트 수락 상태를 설정하는 메서드
    /// </summary>
    /// <param name="accepted">퀘스트 수락 여부</param>
    public void SetQuestState(QuestState state)
    {
        mQuestStateImage.sprite = state switch
        {
            QuestState.Inactive => mInactiveSprite,
            QuestState.Active => mActiveSprite,
            QuestState.Completed => mCompletedSprite,
            QuestState.Rewarded => mRewardedSprite,
            _ => null
        };
    }
}
