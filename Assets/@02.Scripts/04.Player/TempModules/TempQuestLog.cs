using System.Collections.Generic;
using Events.Player.Modules;
using R3;
using UIHUDEnums;
using UnityEngine;


public sealed class TempQuestLog : MonoBehaviour
{
    public class QuestProgress { public int cur, target;}

    readonly Dictionary<int, QuestProgress> mActiveQuests = new();
    public IReadOnlyDictionary<int, QuestProgress> ActiveQuests => mActiveQuests;
    readonly Dictionary<int, QuestProgress> mCompletedQuests = new();
    public IReadOnlyDictionary<int, QuestProgress> CompletedQuests => mCompletedQuests;

    public readonly Subject<QuestAccepted> Accepted = new();
    public readonly Subject<QuestUpdated> Progress = new();
    public readonly Subject<QuestCompleted> Completed = new();
    public readonly Subject<QuestRewarded> Rewarded = new();


    /// <param name="id">"Quest ID"</param>
    /// <param name="target">"Quest 목표치"</param>
    public void Accept(int id, int target)
    {
        if (mActiveQuests.ContainsKey(id)) 
        {
            Debug.LogError($"Quest {id} already accepted");
            return;
        }

        mActiveQuests[id] = new() { cur = 0, target = target };
        Accepted.OnNext(new QuestAccepted(id, 0, target));
    }

    public void AddProgress(int id, int addedAmt = 1)
    {
        if (!mActiveQuests.TryGetValue(id, out var q))
        {
            Debug.LogError($"Quest {id} is not active");
            return;
        }

        q.cur = Mathf.Min(q.cur + addedAmt, q.target);
        Progress.OnNext(new QuestUpdated(id, q.cur, q.target));
        if (q.cur == q.target)
        {
            mActiveQuests.Remove(id);
            mCompletedQuests[id] = q;
            Completed.OnNext(new QuestCompleted(id));
        }
    }

    public bool Reward(int id)
    {
        if (!mCompletedQuests.TryGetValue(id, out var q))
        {
            Debug.LogError($"Quest {id} is not completed");
            return false;
        }

        mCompletedQuests.Remove(id);
        Rewarded.OnNext(new QuestRewarded(id));
        return true;
    }
}
