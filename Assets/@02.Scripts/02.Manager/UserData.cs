using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


//Temporary 각종 UI와 연결용으로 임시로 생성한 UserData 클래스입니다.
// 많이 바뀔 예정이니 사용하지 말아주세요.
public class UserData : Singleton<UserData>, IInitializable
{
    public int Gold;
    public int Soul;
    public Dictionary<string, UserAchievementData> AchievementDataMap;
    public Dictionary<string, UserItemData> ItemDataMap;

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

    protected override void Awake()
    {
        base.Awake();
        AchievementDataMap = new Dictionary<string, UserAchievementData>();
    }

    public UniTask InitializeAsync()
    {
        //TODO: 업적 데이터 로드
        LoadAchievementData();
        return UniTask.CompletedTask;
    }

    public void LoadAchievementData()
    {
        //TODO: 업적 데이터 로드
    }

    public UserAchievementData GetAchievementData(string id)
    {
        return new UserAchievementData()
        {
            Id = id,
            Progress = 0,
            IsUnlocked = true,
            ClearDate = DateTime.Now
        };
        // return AchievementDataMap[id];
    }

    public UserItemData GetItemData(int id)
    {
        return new UserItemData()
        {
            Id = id,
            IsUnlocked = true
        };
    }
}

public class UserAchievementData
{
    public string Id;
    public float Progress;
    public bool IsUnlocked;
    public DateTime ClearDate;
}

public class UserItemData
{
    public int Id;
    public bool IsUnlocked;
}