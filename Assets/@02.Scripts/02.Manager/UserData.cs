// UserDataService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3; // or UniRx if preferred
using UnityEngine;
using UnityEngine.SceneManagement;


public sealed class UserData : Singleton<UserData>
{
    private const string DATA_FILE = "userdata.json";

    // ───────── Settings ─────────
    // SoundPresenter를 통해서만 변경
    readonly ReactiveProperty<float> mMouseSens = new(1.0f);
    readonly ReactiveProperty<float> mMasterVol = new(0.5f);
    readonly ReactiveProperty<float> mBgmVol = new(0.5f);
    readonly ReactiveProperty<float> mSeVol = new(0.5f);
    readonly ReactiveProperty<float> mUiVol = new(0.5f);
    readonly ReactiveProperty<bool> mMasterMute = new(false);
    readonly ReactiveProperty<bool> mBgmMute = new(false);
    readonly ReactiveProperty<bool> mSeMute = new(false);
    readonly ReactiveProperty<bool> mUiMute = new(false);
    readonly ReactiveProperty<FullScreenMode> mFullScreen = new(FullScreenMode.ExclusiveFullScreen);
    readonly ReactiveProperty<Resolution> mResolution = new(new Resolution { width = 1920, height = 1080 });

    // ───────── Public Accessors ─────────
    public float MouseSensitivity { get => mMouseSens.Value; set => mMouseSens.Value = Mathf.Clamp(value, 0.1f, 10f); }
    public float MasterVolume { get => mMasterVol.Value; set => mMasterVol.Value = Mathf.Clamp01(value); }
    public float BgmVolume { get => mBgmVol.Value; set => mBgmVol.Value = Mathf.Clamp01(value); }
    public float SeVolume { get => mSeVol.Value; set => mSeVol.Value = Mathf.Clamp01(value); }
    public float UiVolume { get => mUiVol.Value; set => mUiVol.Value = Mathf.Clamp01(value); }
    public bool IsMasterMuted { get => mMasterMute.Value; set => mMasterMute.Value = value; }
    public bool IsBgmMuted { get => mBgmMute.Value; set => mBgmMute.Value = value; }
    public bool IsSeMuted { get => mSeMute.Value; set => mSeMute.Value = value; }
    public bool IsUiMuted { get => mUiMute.Value; set => mUiMute.Value = value; }
    public FullScreenMode FullScreen { get => mFullScreen.Value; set => mFullScreen.Value = value; }
    public Resolution ScreenResolution { get => mResolution.Value; set => mResolution.Value = value; }

    // Reactive streams
    public ReadOnlyReactiveProperty<float> ObsMouseSensitivity => mMouseSens;
    public ReadOnlyReactiveProperty<float> ObsMasterVolume => mMasterVol;
    public ReadOnlyReactiveProperty<float> ObsBgmVolume => mBgmVol;
    public ReadOnlyReactiveProperty<float> ObsSeVolume => mSeVol;
    public ReadOnlyReactiveProperty<float> ObsUiVolume => mUiVol;
    public ReadOnlyReactiveProperty<bool> ObsMasterMuted => mMasterMute;
    public ReadOnlyReactiveProperty<bool> ObsBgmMuted => mBgmMute;
    public ReadOnlyReactiveProperty<bool> ObsSeMuted => mSeMute;
    public ReadOnlyReactiveProperty<bool> ObsUiMuted => mUiMute;
    public ReadOnlyReactiveProperty<FullScreenMode> ObsFullScreen => mFullScreen;
    public ReadOnlyReactiveProperty<Resolution> ObsResolution => mResolution;

    // ───────── Currency ─────────
    // PlayerHub의 Inventory를 통해서만 변경
    readonly ReactiveProperty<int> mSoul = new(0);
    public int Soul { get => mSoul.Value; set => mSoul.Value = Mathf.Max(0, value); }
    public ReadOnlyReactiveProperty<int> ObsSoul => mSoul;

    // ───────── Codex & Upgrades ─────────
    // AchievementManager를 통해서만 변경
    public Dictionary<string, UserAchievementData> AchievementDataMap { get; private set; }
    // PlayerHub Inventory를 통해서만 변경
    public Dictionary<int, UserItemData> ItemDataMap { get; private set; }
    // SoulStoneShopPanelController를 통해서만 변경
    public Dictionary<string, int> SoulUpgradeDataMap { get; private set; }


    protected override void Awake()
    {
        base.Awake();
        AchievementDataMap = new Dictionary<string, UserAchievementData>();
        ItemDataMap = new Dictionary<int, UserItemData>();
        SoulUpgradeDataMap = new Dictionary<string, int>();
    }

    public async UniTask InitializeAsync()
    {
        await LoadFromDiskAsync();
    }

    // ───────── Load / Save ─────────
    async UniTask LoadFromDiskAsync()
    {
        string path = Path.Combine(Application.persistentDataPath, DATA_FILE);
        if (!File.Exists(path)) return;

        string json = await UniTask.RunOnThreadPool(() => File.ReadAllText(path));
        var dto = JsonUtility.FromJson<UserDataDTO>(json);

        // Settings
        MouseSensitivity = dto.settings.mouseSensitivity;
        IsMasterMuted = dto.settings.isMasterMuted;
        IsBgmMuted = dto.settings.isBgmMuted;
        IsSeMuted = dto.settings.isSeMuted;
        IsUiMuted = dto.settings.isUiMuted;
        MasterVolume = dto.settings.masterVol;
        BgmVolume = dto.settings.bgmVol;
        SeVolume = dto.settings.seVol;
        UiVolume = dto.settings.uiVol;
        FullScreen = dto.settings.displayMode;
        ScreenResolution = dto.settings.resolution;

        // Currency
        Soul = dto.soul;

        // Codex
        AchievementDataMap = dto.achievements
            .ToDictionary(a => a.id,
                          a => new UserAchievementData(a.id, a.currentAmount, a.isCompleted, new DateTime(a.clearDateTicks)));
        ItemDataMap = dto.items
            .ToDictionary(i => i.id,
                          i => new UserItemData(i.id, i.isUnlocked));
        SoulUpgradeDataMap = dto.soulUpgrades
            .ToDictionary(su => su.id, su => su.level);
    }

    public void SaveToDisk()
    {
        var dto = new UserDataDTO
        {
            settings = new SettingsDTO
            {
                mouseSensitivity = MouseSensitivity,
                isMasterMuted = IsMasterMuted,
                isBgmMuted = IsBgmMuted,
                isSeMuted = IsSeMuted,
                isUiMuted = IsUiMuted,
                masterVol = MasterVolume,
                bgmVol = BgmVolume,
                seVol = SeVolume,
                uiVol = UiVolume,
                displayMode = FullScreen,
                resolution = ScreenResolution
            },
            soul = Soul,
            achievements = AchievementDataMap.Values
                .Select(a => new AchievementDTO
                {
                    id = a.Id,
                    currentAmount = a.CurrentAmount,
                    isCompleted = a.IsCompleted,
                    clearDateTicks = a.ClearDate.Date.Ticks
                }).ToList(),
            items = ItemDataMap.Values
                .Select(i => new ItemDTO(i.Id, i.IsUnlocked)).ToList(),
            soulUpgrades = SoulUpgradeDataMap
                .Select(kv => new SoulUpgradeDTO(kv.Key, kv.Value)).ToList()
        };
        string json = JsonUtility.ToJson(dto, true);
        string path = Path.Combine(Application.persistentDataPath, DATA_FILE);
        File.WriteAllText(path, json);
    }

    // ───────── Data Access ─────────
    public void UpdateAchievementData(string id, int currentAmount, bool isCompleted = false)
    {
        if (!AchievementDataMap.TryGetValue(id, out var data))
        {
            Debug.LogError($"Achievement {id} not found");
            return;
        }

        if (isCompleted)
        {
            data.IsCompleted = true;
            data.ClearDate = DateTime.Now;
        }
        else
        {
            data.CurrentAmount = currentAmount;
        }
    }
    public UserAchievementData GetAchievementData(string id)
    {
        return AchievementDataMap.TryGetValue(id, out var data)
            ? data
            : null;
    }

    public void SetItemData(UserItemData data)
    {
        ItemDataMap[data.Id] = data;
    }
    public UserItemData GetItemData(int id)
    {
        return ItemDataMap.TryGetValue(id, out var data)
            ? data
            : null;
    }

    public void SetSoulUpgradeLevel(string id, int level)
    {
        SoulUpgradeDataMap[id] = level;
    }
    public int GetSoulUpgradeLevel(string id)
    {
        return SoulUpgradeDataMap.TryGetValue(id, out var lvl) ? lvl : 0;
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        throw new NotImplementedException();
    }

    // ───────── DTOs ─────────
    [Serializable]
    class UserDataDTO
    {
        public SettingsDTO settings;
        public int soul;
        public List<AchievementDTO> achievements;
        public List<ItemDTO> items;
        public List<SoulUpgradeDTO> soulUpgrades;
    }

    [Serializable]
    struct SettingsDTO
    {
        public float mouseSensitivity;
        public bool isMasterMuted;
        public bool isBgmMuted;
        public bool isSeMuted;
        public bool isUiMuted;
        public float masterVol;
        public float bgmVol;
        public float seVol;
        public float uiVol;
        public FullScreenMode displayMode;
        public Resolution resolution;
    }

    [Serializable]
    struct AchievementDTO
    {
        public string id;
        public int currentAmount;
        public bool isCompleted;
        public long clearDateTicks;
        public AchievementDTO(string id, int currentAmount, bool isCompleted, long clearDateTicks)
        {
            this.id = id;
            this.currentAmount = currentAmount;
            this.isCompleted = isCompleted;
            this.clearDateTicks = clearDateTicks;
        }
    }
    [Serializable]
    struct ItemDTO
    {
        public int id;
        public bool isUnlocked;
        public ItemDTO(int id, bool isUnlocked)
        {
            this.id = id;
            this.isUnlocked = isUnlocked;
        }
    }

    [Serializable]
    struct SoulUpgradeDTO
    {
        public string id;
        public int level;
        public SoulUpgradeDTO(string id, int level)
        {
            this.id = id;
            this.level = level;
        }
    }
}

/// <summary>
/// Simple user data containers.
/// </summary>
public class UserAchievementData
{
    public string Id;
    public int CurrentAmount;
    public bool IsCompleted;
    public DateTime ClearDate;

    public UserAchievementData(string id, int currentAmount, bool isCompleted, DateTime clearDate = default)
    {
        Id = id;
        CurrentAmount = currentAmount;
        IsCompleted = isCompleted;
        ClearDate = clearDate;
    }
}

public class UserItemData
{
    public int Id;
    public bool IsUnlocked;

    public UserItemData(int id, bool isUnlocked)
    {
        Id = id;
        IsUnlocked = isUnlocked;
    }
}