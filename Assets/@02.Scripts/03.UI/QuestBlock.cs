using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestBlock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mTitleText;
    [SerializeField] private TextMeshProUGUI mRequestInfoText;
    [SerializeField] private TextMeshProUGUI mRewardText;
    [SerializeField] private Button mAcceptButton;
    [SerializeField] private GameObject mAcceptTag;

    private string mQuestId;
    public string QuestId => mQuestId;

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

    public void SetAccepted(bool accepted)
    {
        mAcceptTag.SetActive(accepted);
        mAcceptButton.interactable = !accepted;
    }
}
