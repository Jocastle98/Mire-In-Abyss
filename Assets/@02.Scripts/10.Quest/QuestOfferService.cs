using System;
using System.Collections.Generic;
using Events.Player.Modules;
using QuestEnums;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using R3;
using System.Linq;

public class QuestOfferService: Singleton<QuestOfferService>
{
    [Header("퀘스트 선택 설정")] 
    [SerializeField] private int mCommonQuestCount = 3;     //일반퀘스트 개수
    [SerializeField] private int mEpicQuestCount = 1;       //에픽퀘스트 개수
    [SerializeField] private int mCommonQuestMinID = 1;     //일반 퀘스트 ID 최소 범위
    [SerializeField] private int mCommonQuestMaxID = 10;    //일반 퀘스트 ID 최대 범위
    [SerializeField] private int mEpicQuestMinID = 11;      //에픽 퀘스트 ID 최소 범위
    [SerializeField] private int mEpicQuestMaxID = 15;      //에픽 퀘스트 ID 최대 범위
    private Dictionary<string, QuestState> mGeneratedQuestStates;


    void Start()
    {
        SubscribeEvents();
        GenerateQuestList();
    }

    private void SubscribeEvents()
    {
        R3EventBus.Instance.Receive<QuestAccepted>()
        .Subscribe(e => OnQuestStateChanged(e.ID, QuestState.Active))
        .AddTo(this);
        R3EventBus.Instance.Receive<QuestCompleted>()
        .Subscribe(e => OnQuestStateChanged(e.ID, QuestState.Completed))
        .AddTo(this);
        R3EventBus.Instance.Receive<QuestRewarded>()
        .Subscribe(e => OnQuestStateChanged(e.ID, QuestState.Rewarded))
        .AddTo(this);
    }


    /// <summary>
    /// 퀘스트 목록을 생성
    /// 일반 퀘스트와 에픽 퀘스트를 랜덤하게 선택하여 표시
    /// </summary>
    public List<string> GenerateQuestList()
    {
        //데이터베이스에서 모든 퀘스트 ID가져오기
        List<string> allQuestIds = GameDB.Instance.QuestDatabase.QuestIds;
        if (allQuestIds == null || allQuestIds.Count == 0) 
        {
            return new List<string>();
        }

        //일반 퀘스트와 에픽 퀘스트 ID를 분류
        List<string> commonQuestsIds = new List<string>();
        List<string> epicQuestIds = new List<string>();

        foreach (var questId in allQuestIds)
        {
            if (questId.StartsWith("Q"))
            {
                string numericPart = questId.Substring(1);
                int id;

                if (int.TryParse(numericPart, out id))
                {
                    if (id >= mCommonQuestMinID && id <= mCommonQuestMaxID)
                    {
                        commonQuestsIds.Add(questId);
                    }
                    else if (id >= mEpicQuestMinID && id <= mEpicQuestMaxID)
                    {
                        epicQuestIds.Add(questId);
                    }
                }
                
            }
        }

        //랜덤 퀘스트 생성
        var result = new List<string>();
        result.AddRange(GenerateRandomQuests(commonQuestsIds, mCommonQuestCount));
        result.AddRange(GenerateRandomQuests(epicQuestIds, mEpicQuestCount));

        mGeneratedQuestStates = new Dictionary<string, QuestState>();
        foreach (var questId in result)
        {
            mGeneratedQuestStates[questId] = QuestState.Inactive;
        }

        return result;
    }

    public QuestState GetQuestState(string questId)
    {
        if (mGeneratedQuestStates.TryGetValue(questId, out var state))
        {
            return state;
        }

        Debug.LogError($"Quest {questId} not found");
        return QuestState.Inactive;
    }

    public List<string> GetQuestList()
    {
        return mGeneratedQuestStates.Keys.ToList();
    }

    /// <summary>
    /// 퀘스트 풀에서 지정된 개수만큼 랜덤하게 퀘스트를 선택하여 UI 생성
    /// </summary>
    /// <param name="questPool">퀘스트 ID 풀</param>
    /// <param name="count">선택할 퀘스트 개수</param>
    private List<String> GenerateRandomQuests(List<string> questPool, int count)
    {
        if(questPool == null || questPool.Count == 0) 
        {
            return new List<string>();
        }

        //퀘스트 ID 섞기
        for (int i = 0; i < questPool.Count; i++)
        {
            string temp = questPool[i];
            int randomIndex = Random.Range(i, questPool.Count);
            questPool[i] = questPool[randomIndex];
            questPool[randomIndex] = temp;
        }

        //요청된 개수와 실제 가능한 개수 중 작은 값을 선택
        int numToGenerate = Mathf.Min(count, questPool.Count);
        
        //요청된 개수만큼 생성된 퀘스트를 반환
        return questPool.GetRange(0, numToGenerate);
    }

    private void OnQuestStateChanged(string questId, QuestState newState)
    {
        mGeneratedQuestStates[questId] = newState;
    }


    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}
