using System.Collections.Generic;
using Events.Player.Modules;
using R3;
using UIHUDEnums;
using UnityEngine;


public sealed class QuestLog : MonoBehaviour
{
    readonly Dictionary<string, Quest> mActiveQuests = new();
    public IReadOnlyDictionary<string, Quest> ActiveQuests => mActiveQuests;
    readonly Dictionary<string, Quest> mCompletedQuests = new();
    public IReadOnlyDictionary<string, Quest> CompletedQuests => mCompletedQuests;

    public readonly Subject<QuestAccepted> Accepted = new();
    public readonly Subject<QuestUpdated> Progress = new();
    public readonly Subject<QuestCompleted> Completed = new();
    public readonly Subject<QuestRewarded> Rewarded = new();


    public Quest GetQuest(string id)
    {
        if (mActiveQuests.TryGetValue(id, out var q))
        {
            return q;
        }
        else if (mCompletedQuests.TryGetValue(id, out q))
        {
            return q;
        }
        else
        {
            Debug.LogError($"Quest {id} not found");
            return null;
        }
    }

    public List<string> GetQuestList()
    {
        List<string> questList = new();
        foreach (var quest in mActiveQuests)
        {
            questList.Add(quest.Key);
        }
        foreach (var quest in mCompletedQuests)
        {
            questList.Add(quest.Key);
        }
        return questList;
    }

    public void Accept(string id)
    {
        if (mActiveQuests.ContainsKey(id)) 
        {
            Debug.LogError($"Quest {id} already accepted");
            return;
        }

        mActiveQuests.Add(id, GameDB.Instance.QuestDatabase.GetQuestById(id));
        Accepted.OnNext(new QuestAccepted(id));
    }

    public void AddProgress(string id, int addedAmt = 1)
    {
        if (!mActiveQuests.TryGetValue(id, out var q))
        {
            //Debug.LogError($"Quest {id} is not active");
            return;
        }

        q.CurrentAmount = Mathf.Min(q.CurrentAmount + addedAmt, q.TargetAmount);
        Progress.OnNext(new QuestUpdated(id, q.CurrentAmount));
        if (q.CurrentAmount == q.TargetAmount)
        {
            q.isCompleted = true;
            mCompletedQuests.Add(id, q);
            mActiveQuests.Remove(id);
            Completed.OnNext(new QuestCompleted(id));
        }
    }

    public bool Reward(string id)
    {
        if (!mCompletedQuests.TryGetValue(id, out var q))
        {
            Debug.LogError($"Quest {id} is not completed");
            return false;
        }

        // 퀘스트 보상 처리
        PlayerHub.Instance.Inventory.AddSoul(q.RewardSoul);

        mCompletedQuests.Remove(id);
        Rewarded.OnNext(new QuestRewarded(id));
        return true;
    }
}
