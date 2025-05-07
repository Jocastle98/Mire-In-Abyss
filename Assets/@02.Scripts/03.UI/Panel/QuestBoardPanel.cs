using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Events.Player.Modules;
using R3;
using Cysharp.Threading.Tasks;

public class QuestBoardPanel : BaseUIPanel
{
    [Header("퀘스트 패널")] 
    [SerializeField] private Transform mQuestListContainer; //퀘스트 목록이 표시될 parent Transform
    [SerializeField] private QuestBlock mQuestListPrefab;   //퀘스트 항목 UI 프리팹

    [Header("퀘스트 선택 설정")] 
    [SerializeField] private int mCommonQuestCount = 3;     //일반퀘스트 개수
    [SerializeField] private int mEpicQuestCount = 1;       //에픽퀘스트 개수
    [SerializeField] private int mCommonQuestMinID = 1;     //일반 퀘스트 ID 최소 범위
    [SerializeField] private int mCommonQuestMaxID = 10;    //일반 퀘스트 ID 최대 범위
    [SerializeField] private int mEpicQuestMinID = 11;      //에픽 퀘스트 ID 최소 범위
    [SerializeField] private int mEpicQuestMaxID = 15;      //에픽 퀘스트 ID 최대 범위
    
    private List<QuestBlock> mQuestBlocks = new List<QuestBlock>();  //현재 생성된 퀘스트 UI 요소 목록
    
    private void Start()
    {
        GenerateQuestList();
    }
    
    /// <summary>
    /// 패널 표시 시 퀘스트 목록 생성
    /// </summary>
    public override async UniTask Show()
    {
        GenerateQuestList();
        await base.Show();
    }

    /// <summary>
    /// 퀘스트 목록을 생성
    /// 일반 퀘스트와 에픽 퀘스트를 랜덤하게 선택하여 표시
    /// </summary>
    private void GenerateQuestList()
    {
        //기존 목록 초기화
        ClearQuestBlocks();
        if (GameDB.Instance.QuestDatabase == null) return;

        //데이터베이스에서 모든 퀘스트 ID가져오기
        List<string> allQuestIds = GameDB.Instance.QuestDatabase.QuestIds;
        if (allQuestIds == null || allQuestIds.Count == 0) return;

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
        GenerateRandomQuests(commonQuestsIds, mCommonQuestCount);
        GenerateRandomQuests(epicQuestIds, mEpicQuestCount);
    }

    /// <summary>
    /// 퀘스트 풀에서 지정된 개수만큼 랜덤하게 퀘스트를 선택하여 UI 생성
    /// </summary>
    /// <param name="questPool">퀘스트 ID 풀</param>
    /// <param name="count">선택할 퀘스트 개수</param>
    private void GenerateRandomQuests(List<string> questPool, int count)
    {
        if(questPool.Count == 0) return;

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

        //선택된 개수만큼 퀘스트 UI생성
        for (int i = 0; i < numToGenerate; i++)
        {
            string questId = questPool[i];
            Quest quest = GameDB.Instance.QuestDatabase.GetQuestById(questId);

            if (quest != null)
            {
                CreateQuestBlock(quest);
            }
        }
    }

    /// <summary>
    /// 퀘스트 정보를 받아 UI요소를 생성하는 메서드
    /// </summary>
    /// <param name="quest">표시할 퀘스트 정보</param>
    private void CreateQuestBlock(Quest quest)
    {
        if (mQuestListPrefab == null || mQuestListContainer == null) return;

        //퀘스트 블록 UI 생성 및 초기화
        QuestBlock questList = Instantiate(mQuestListPrefab, mQuestListContainer);
        questList.Initialize(quest, OnQuestBlockClicked);
        mQuestBlocks.Add(questList);
    }

    /// <summary>
    /// 모든 퀘스트UI요소를 제거하는 메서드
    /// </summary>
    private void ClearQuestBlocks()
    {
        foreach (var list in mQuestBlocks)
        {
            if (list != null)
            {
                Destroy(list.gameObject);
            }
        }
        mQuestBlocks.Clear();
    }

    /// <summary>
    /// 퀘스트 항목 클릭 시 호출되는 콜백 메서드
    /// </summary>
    /// <param name="quest"></param>
    private void OnQuestBlockClicked(Quest quest)
    {
        bool activated = GameDB.Instance.QuestDatabase.ActivateQuest(quest.Id);

        if (activated)
        {
            foreach (var list in mQuestBlocks)
            {
                if (list.QuestId == quest.Id)
                {
                    list.SetAccepted(true);
                }
            }
        }
        else
        {
            Debug.Log("퀘스트를 수락할 수 없습니다.");
        }
    }
}
