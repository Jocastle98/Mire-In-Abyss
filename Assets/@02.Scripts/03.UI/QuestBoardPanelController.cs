using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class QuestBoardPanelController : PopupPanelController
{
    [Header("퀘스트 패널")] 
    [SerializeField] private Transform mQuestListContainer;
    [SerializeField] private QuestBlock mQuestListPrefab;

    [Header("퀘스트 선택 설정")] 
    [SerializeField] private int mCommonQuestCount = 3;
    [SerializeField] private int mEpicQuestCount = 1;
    [SerializeField] private int mCommonQuestMinID = 1;
    [SerializeField] private int mCommonQuestMaxID = 10;
    [SerializeField] private int mEpicQuestMinID = 11;
    [SerializeField] private int mEpicquestMaxID = 15;
    
    private List<QuestBlock> mQuestLists = new List<QuestBlock>();
    private QuestDatabase mQuestDatabase;
    
    private void Start()
    {
        GenerateQuestList();
    }

    public void SetQuestDatabase(QuestDatabase questDatabase)
    {
        mQuestDatabase = questDatabase;
    }
    
    public override void Show()
    {
        base.Show();
        GenerateQuestList();
    }

    private void GenerateQuestList()
    {
        ClearQuestLists();
        if (mQuestDatabase == null) return;

        List<string> allQuestIds = mQuestDatabase.QuestIds;
        if (allQuestIds == null || allQuestIds.Count == 0) return;

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
                    else if (id >= mEpicQuestMinID && id <= mEpicquestMaxID)
                    {
                        epicQuestIds.Add(questId);
                    }
                }
                
            }
        }

        GenerateRandomQuests(commonQuestsIds, mCommonQuestCount);
        GenerateRandomQuests(epicQuestIds, mEpicQuestCount);
    }

    private void GenerateRandomQuests(List<string> questPool, int count)
    {
        if(questPool.Count == 0) return;

        for (int i = 0; i < questPool.Count; i++)
        {
            string temp = questPool[i];
            int randomIndex = Random.Range(i, questPool.Count);
            questPool[i] = questPool[randomIndex];
            questPool[randomIndex] = temp;
        }

        int numToGenerate = Mathf.Min(count, questPool.Count);

        for (int i = 0; i < numToGenerate; i++)
        {
            string questId = questPool[i];
            Quest quest = mQuestDatabase.GetQuestById(questId);

            if (quest != null)
            {
                CreateQuestList(quest);
            }
        }
    }

    private void CreateQuestList(Quest quest)
    {
        if (mQuestListPrefab == null || mQuestListContainer == null) return;

        QuestBlock questList = Instantiate(mQuestListPrefab, mQuestListContainer);
        questList.Initialize(quest, OnQuestListClicked);
        mQuestLists.Add(questList);
    }

    private void ClearQuestLists()
    {
        foreach (var list in mQuestLists)
        {
            if (list != null)
            {
                Destroy(list.gameObject);
            }
        }
        mQuestLists.Clear();
    }

    private void OnQuestListClicked(Quest quest)
    {
        bool activated = mQuestDatabase.ActivateQuest(quest.Id);

        if (activated)
        {
            foreach (var list in mQuestLists)
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
    
    public void OnClickCloseButton()
    {
        Hide();
    }
}
