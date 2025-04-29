using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 게시판에 표시되는 개별 퀘스트 UI 요소를 관리하는 클래스
/// </summary>
public class QuestBlock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mTitleText;        //퀘스트 제목을 표시하는 텍스트
    [SerializeField] private TextMeshProUGUI mRequestInfoText;  //퀘스트 요청 정보를 표시하는 텍스트
    [SerializeField] private TextMeshProUGUI mRewardText;       //퀘스트 보상을 표시하는 텍스트
    [SerializeField] private Button mAcceptButton;              //퀘스트 수락 버튼
    [SerializeField] private GameObject mAcceptTag;             //퀘스트 수락 완료 표시 아이콘

    private string mQuestId;    //퀘스트 고유 아이디
    public string QuestId => mQuestId;  //외부에서 퀘스트 ID에 접근하기 위한 프로퍼티

    /// <summary>
    /// 퀘스트 정보로 UI요소를 초기화하는 메서드
    /// </summary>
    /// <param name="quest">표시할 퀘스트 정보</param>
    /// <param name="onClickCallback">퀘스트 수락 버튼 클릭 시 호출될 콜백 함수</param>
    public void Initialize(Quest quest, Action<Quest> onClickCallback)
    {
        mQuestId = quest.Id;

        mTitleText.text = quest.Title;
        mRequestInfoText.text = quest.RequestInformation;
        mRewardText.text = $"{quest.RewardSoul} 영혼석";
        mAcceptTag.SetActive(false);
        mAcceptButton.onClick.RemoveAllListeners();
        mAcceptButton.onClick.AddListener(()=> onClickCallback?.Invoke(quest));
    }

    /// <summary>
    /// 퀘스트 수락 상태를 설정하는 메서드
    /// </summary>
    /// <param name="accepted">퀘스트 수락 여부</param>
    public void SetAccepted(bool accepted)
    {
        mAcceptTag.SetActive(accepted);
        mAcceptButton.interactable = !accepted;
    }
}
