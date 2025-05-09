using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Events.Player.Modules;
using R3;
using Cysharp.Threading.Tasks;
using TMPro;
using QuestEnums;

public class PlayerQuestBoardPanel : MonoBehaviour
{
    [Header("퀘스트 목록")]
    [SerializeField] private Transform mContent;            //퀘스트 목록이 표시될 parent Transform
    [SerializeField] private QuestCardView mQuestViewPrefab;   //퀘스트 항목 UI 프리팹

    [Header("퀘스트 상세")]
    [SerializeField] private TMP_Text mQuestTitleText;
    [SerializeField] private TMP_Text mQuestDescriptionText;
    [SerializeField] private TMP_Text mQuestRewardText;
    [SerializeField] private TMP_Text mQuestProgressText;

    private Dictionary<string, QuestCardView> mQuestViews = new();    // id, questView
    private string mNowDetailQuestId;
    private bool mIsInit = false;

    void OnEnable()
    {
        if (mQuestViewPrefab == null || mContent == null)
        {
            Debug.LogError("퀘스트 목록 UI 프리팹이나 컨테이너가 설정되지 않았습니다.");
            return;
        }

        if (!mIsInit)
        {
            initQuestList();
        }
        else
        {
            if (mQuestViews.Count > 0)
            {
                showDetail(mQuestViews.Keys.First());
            }
        }
    }

    private void initQuestList()
    {
        mQuestViews.Clear();
        mNowDetailQuestId = null;
        mQuestTitleText.text = "";
        mQuestDescriptionText.text = "";
        mQuestRewardText.text = "";
        mQuestProgressText.text = "";

        var questList = PlayerHub.Instance.QuestLog.GetQuestList();

        //퀘스트를 담을 퀘스트 블록 생성
        foreach (var questId in questList)
        {
            CreateQuestView(GameDB.Instance.QuestDatabase.GetQuestById(questId));
        }
        if (questList.Count > 0)
        {
            showDetail(questList[0]);
        }
        mIsInit = true;
    }

    private void Start()
    {
        subscribeEvents();
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<QuestAccepted>()
        .Subscribe(e => mQuestViews[e.ID].SetQuestState(QuestState.Active))
        .AddTo(this);
        R3EventBus.Instance.Receive<QuestCompleted>()
        .Subscribe(e => mQuestViews[e.ID].SetQuestState(QuestState.Completed))
        .AddTo(this);
        R3EventBus.Instance.Receive<QuestRewarded>()
        .Subscribe(e => mQuestViews[e.ID].SetQuestState(QuestState.Rewarded))
        .AddTo(this);
    }

    private void CreateQuestView(Quest quest)
    {
        //퀘스트 블록 UI 생성 및 초기화
        QuestCardView questView = Instantiate(mQuestViewPrefab, mContent);
        questView.Bind(quest, showDetail);
        mQuestViews.Add(quest.Id, questView);
    }

    void showDetail(string id)
    {
        mNowDetailQuestId = id;
        Quest quest = GameDB.Instance.QuestDatabase.GetQuestById(id);
        mQuestTitleText.text = quest.Title;
        mQuestDescriptionText.text = quest.RequestInformation;
        mQuestRewardText.text = $"영혼석 {quest.RewardSoul}개";
        mQuestProgressText.text = $"진행도: {quest.CurrentAmount} / {quest.TargetAmount}";
    }
}
