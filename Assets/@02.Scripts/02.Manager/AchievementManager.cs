using System.Collections.Generic;
using Events.Player.Modules;
using Events.UserData;
using R3;
using UIHUDEnums;
using UnityEngine;
using UnityEngine.SceneManagement;


public sealed class AchievementManager : Singleton<AchievementManager>
{
    readonly Dictionary<string, UserAchievementData> mActiveAchievements = new();
    public IReadOnlyDictionary<string, UserAchievementData> ActiveAchievements => mActiveAchievements;
    readonly Dictionary<string, UserAchievementData> mCompletedAchievements = new();
    public IReadOnlyDictionary<string, UserAchievementData> CompletedAchievements => mCompletedAchievements;

    public readonly Subject<AchievementUpdated> Progress = new();
    public readonly Subject<AchievementCompleted> Completed = new();


    public void InitAchievementDataFromUserData()
    {
        var achievements = UserData.Instance.AchievementDataMap;
        foreach (var achievement in achievements)
        {
            if (achievement.Value.IsCompleted)
            {
                mCompletedAchievements.Add(achievement.Key, achievement.Value);
            }
            else
            {
                mActiveAchievements.Add(achievement.Key, achievement.Value);
            }
        }
    }

    public UserAchievementData GetUserAchievementData(string id)
    {
        if (mActiveAchievements.TryGetValue(id, out var a))
        {
            return a;
        }
        else if (mCompletedAchievements.TryGetValue(id, out a))
        {
            return a;
        }
        else
        {
            Debug.LogError($"Achievement {id} not found");
            return null;
        }
    }

    public List<string> GetAchievementList()
    {
        List<string> achievementList = new();
        foreach (var achievement in mActiveAchievements)
        {
            achievementList.Add(achievement.Key);
        }
        foreach (var achievement in mCompletedAchievements)
        {
            achievementList.Add(achievement.Key);
        }
        return achievementList;
    }

    public void AddProgress(string id, int addedAmt = 1)
    {
        if (!mActiveAchievements.TryGetValue(id, out var a))
        {
            //Debug.LogError($"Achievement {id} is not active");
            return;
        }

        int targetAmount = GameDB.Instance.AchievementDatabase.GetAchievementById(id).TargetAmount;

        a.CurrentAmount = Mathf.Min(a.CurrentAmount + addedAmt, targetAmount);
        Progress.OnNext(new AchievementUpdated(id, a.CurrentAmount));

        // 업적 달성 시 처리
        if (a.CurrentAmount == targetAmount)
        {
            a.IsCompleted = true;
            mCompletedAchievements.Add(id, a);
            mActiveAchievements.Remove(id);
            Completed.OnNext(new AchievementCompleted(id));

            UserData.Instance.UpdateAchievementData(id, a.CurrentAmount, true);
        }
        else // Userdata 업적 진행도 증가 처리
        {
            UserData.Instance.UpdateAchievementData(id, a.CurrentAmount);
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}
