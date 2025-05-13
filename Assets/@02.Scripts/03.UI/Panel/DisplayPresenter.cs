using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DisplayPresenter : TabPresenterBase
{
    [Header("UI")]
    [SerializeField] TMP_Dropdown mModeDropdown;       // “Fullscreen / Borderless / Windowed”
    [SerializeField] TMP_Dropdown mResolutionDropdown; // 해상도 목록

    private static readonly Vector2Int[] s_supportedResolutions = new[]
    {
        new Vector2Int(2560, 1440),
        new Vector2Int(1920, 1080),
        new Vector2Int(1600,  900),
        new Vector2Int(1366,  768),
        new Vector2Int(1280,  720),
        new Vector2Int(1024,  576),
        new Vector2Int( 800,  450),
    };

    Resolution[] mResolutions;
    readonly FullScreenMode[] mModeTable =
    {
        FullScreenMode.ExclusiveFullScreen,         // Fullscreen
        FullScreenMode.FullScreenWindow,            // Borderless
        FullScreenMode.Windowed                     // Windowed
    };

    public override void Initialize()
    {
        populateModeDropdown();
        populateResolutionDropdown();

        mModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
        mResolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        mModeDropdown.value = Array.FindIndex(mModeTable, mode => mode == UserData.Instance.FullScreen);
        mResolutionDropdown.value = Array.FindIndex(mResolutions, res => res.width == UserData.Instance.ScreenResolution.width && res.height == UserData.Instance.ScreenResolution.height);
    }

    void populateModeDropdown()
    {
        mModeDropdown.ClearOptions();
        mModeDropdown.AddOptions(new List<string>() { "Fullscreen", "Borderless", "Windowed" });
    }

    void populateResolutionDropdown()
    {
        var available = Screen.resolutions
            .Select(r => new Vector2Int(r.width, r.height))
            .Distinct()
            .ToHashSet();

        var list = s_supportedResolutions
            .Where(v => available.Contains(v))
            .ToList();

        var current = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
        if (!list.Contains(current))
            list.Add(current);

        mResolutions = list
            .Select(v => new Resolution
            {
                width = v.x,
                height = v.y,
                refreshRateRatio = Screen.currentResolution.refreshRateRatio
            })
            .OrderBy(r => r.width)
            .ToArray();

        mResolutionDropdown.ClearOptions();
        mResolutionDropdown.AddOptions(
            mResolutions.Select(r => $"{r.width}×{r.height}").ToList()
        );

        int curResIdx = Array.FindIndex(mResolutions,
            r => r.width == current.x && r.height == current.y
        );
        if (curResIdx < 0)
            curResIdx = mResolutions.Length - 1;
        mResolutionDropdown.value = curResIdx;
    }

    /* ────────── Callbacks ────────── */
    void OnDisplayModeChanged(int idx)
    {
        var mode = mModeTable[Mathf.Clamp(idx, 0, mModeTable.Length - 1)];
        Screen.fullScreenMode = mode;

        ApplyResolution(mResolutionDropdown.value, mode);
    }

    void OnResolutionChanged(int idx)
    {
        ApplyResolution(idx, Screen.fullScreenMode);
    }

    void ApplyResolution(int idx, FullScreenMode mode)
    {
        var res = mResolutions[Mathf.Clamp(idx, 0, mResolutions.Length - 1)];
        Screen.SetResolution(res.width, res.height, mode);
    }
}
