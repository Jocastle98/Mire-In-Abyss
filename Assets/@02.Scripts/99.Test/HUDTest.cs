using System;
using Events.Abyss;
using Events.Quest;
using UIHUDEnums;
using UnityEngine;
using UnityEngine.Serialization;

public class HUDTest: MonoBehaviour
{
    public GameObject Enemy;
    public bool IsSpawned = false;
    public int DifficultyLevel = 1;
    public float DifficultyProgress = 0;

    [Header("Quest")] [SerializeField] private string mQuestTitle;
    [SerializeField] private string mQuestDesc;
    
    [Header("QuestUpdate")]
    [SerializeField] private int mUpdatedQuestID;
    [SerializeField] private string mUpdatedQuestTitle;
    [SerializeField] private string mUpdatedQuestDesc;
    [SerializeField] private QuestState mUpdatedQuestState;
    
    DateTime mStartUtc;
    private int mLastQuestID = -1;

    
    private void Start()
    {
        mStartUtc     = DateTime.UtcNow;
    }

    private void Update()
    {
        DifficultyProgress += Time.deltaTime * 0.3f;
        if (DifficultyProgress >= 1)
        {
            DifficultyProgress = 0;
            DifficultyLevel++;
            R3EventBus.Instance.Publish(new DifficultyChanged(DifficultyLevel));
        }
        
        R3EventBus.Instance.Publish(new DifficultyProgressed(DifficultyProgress));
        
        TimeSpan elapsed = DateTime.UtcNow - mStartUtc;
        R3EventBus.Instance.Publish(new PlayTimeChanged(elapsed));
    }

    public void OnToggleEnemyExist()
    {
        if (IsSpawned)
        {
            IsSpawned = false;
            R3EventBus.Instance.Publish(new EnemyDied(Enemy.transform));
        }
        else
        {
            IsSpawned = true;
            R3EventBus.Instance.Publish(new EnemySpawned(Enemy.transform));
        }
    }

    public void OnDifficultyLevelUp()
    {
        DifficultyLevel++;
        DifficultyProgress = 0;
        R3EventBus.Instance.Publish(new DifficultyChanged(DifficultyLevel));
        R3EventBus.Instance.Publish(new DifficultyProgressed(DifficultyProgress));
    }

    public void OnAddQuest()
    {
        mLastQuestID++;
        TempQuestInfo questInfo = new TempQuestInfo(mLastQuestID, mQuestTitle, mQuestDesc, QuestState.Active);
        R3EventBus.Instance.Publish(new QuestAddedOrUpdated(questInfo));
    }

    public void OnQuestUpdated()
    {
        if (mUpdatedQuestState == QuestState.Completed)
        {
            OnQuestCompleted();
            return;
        }
            
        TempQuestInfo questInfo = new TempQuestInfo(mUpdatedQuestID, mUpdatedQuestTitle, mUpdatedQuestDesc, QuestState.Active);
        R3EventBus.Instance.Publish(new QuestAddedOrUpdated(questInfo));
    }

    public void OnQuestCompleted()
    {
        R3EventBus.Instance.Publish(new QuestCompleted(mUpdatedQuestID));
    }

    public void OnRemoveQuest()
    {
        R3EventBus.Instance.Publish(new QuestRemoved(mUpdatedQuestID));
    }
}