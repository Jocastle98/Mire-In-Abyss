using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;


//Temporary 각종 UI와 연결용으로 임시로 생성한 UserData 클래스입니다.
// 많이 바뀔 예정이니 사용하지 말아주세요.

public class UserData : Singleton<UserData>, IInitializable
{
    // 재화
    public int Soul;
    
    // Codex 데이터 (Item, Achievement)
    public Dictionary<string, UserAchievementData> AchievementDataMap;
    public Dictionary<string, UserItemData> ItemDataMap;

    // Settings 데이터 (Sound, Display, Control)
    public SoundData SoundData;

    public FullScreenMode DisplayMode;
    public Resolution Resolution;

    public float MouseSensitivity;

    // 영혼석 업그레이드 데이터
    public Dictionary<string, int> SoulUpgradeDataMap; // UpgradeId, CurrentLevel

    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

    protected override void Awake()
    {
        base.Awake();
        AchievementDataMap = new Dictionary<string, UserAchievementData>();
    }

    public UniTask InitializeAsync()
    {
        LoadAllData();
        return UniTask.CompletedTask;
    }

    public void LoadAllData()
    {
        //TODO: 모든 데이터 로드
        SoundData = new SoundData()
        {
            IsMasterOn = true,
            IsBgmOn = true,
            IsSeOn = true,
            IsUiOn = true,
            MasterVol = 0.5f,
            BgmVol = 0.5f,
            SeVol = 0.5f,
            UiVol = 0.5f
        };
        DisplayMode = FullScreenMode.ExclusiveFullScreen;
        Resolution = new Resolution() { width = 1920, height = 1080 };
        MouseSensitivity = 1f;

        SoulUpgradeDataMap = new Dictionary<string, int>();
    }

    
    public void SaveSettings()
    {
        //TODO: 설정 저장
    }

    public void SetSoulUpgradeData(string id, int level)
    {
        SoulUpgradeDataMap[id] = level;
    }

    public int GetSoulUpgradeData(string id)
    {
        return SoulUpgradeDataMap.TryGetValue(id, out var level) ? level : 0;
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
    
    private void applySaveData()
    {
        //TODO: 모든 데이터 적용
        // 재화 - PlayerHub
        // Codex - CodexManager
        // Settings - Sound, Display, Control
        // 영혼석 업그레이드 - SoulStoneShopPanelController
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

public class SoundData
{
    public bool IsMasterOn;
    public bool IsBgmOn;
    public bool IsSeOn;
    public bool IsUiOn;
    public float MasterVol;
    public float BgmVol;
    public float SeVol;
    public float UiVol;
}