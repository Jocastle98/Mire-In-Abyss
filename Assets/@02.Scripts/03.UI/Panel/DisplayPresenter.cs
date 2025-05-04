using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DisplayPresenter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Dropdown mModeDropdown;       // “Fullscreen / Borderless / Windowed”
    [SerializeField] TMP_Dropdown mResolutionDropdown; // 해상도 목록

    Resolution[] mResolutions;
    readonly FullScreenMode[] mModeTable =
    {
        FullScreenMode.ExclusiveFullScreen,         // Fullscreen
        FullScreenMode.FullScreenWindow,            // Borderless
        FullScreenMode.Windowed                     // Windowed
    };

    void Start()
    {
        /* ─── 1) 화면 모드 드롭다운 채우기 ─── */
        mModeDropdown.ClearOptions();
        mModeDropdown.AddOptions(new List<string>() { "Fullscreen", "Borderless", "Windowed" });

        /* ─── 2) 해상도 드롭다운 채우기 ─── */
        mResolutions = Screen.resolutions
                            .Select(r => new Resolution { width = r.width, height = r.height, refreshRate = Screen.currentResolution.refreshRate })
                            .Distinct()
                            .OrderBy(r => r.width)
                            .ToArray();

        mResolutionDropdown.ClearOptions();
        mResolutionDropdown.AddOptions(
            mResolutions.Select(r => $"{r.width}×{r.height}").ToList());

        /* ─── 3) 현재 상태 → 드롭다운 값 세팅 ─── */
        int curModeIdx = Array.FindIndex(mModeTable, m => m == Screen.fullScreenMode);
        if (curModeIdx < 0) 
        {
            curModeIdx = 0;   
        }
        mModeDropdown.value = curModeIdx;

        int curResIdx = Array.FindIndex(mResolutions,
            r => r.width == Screen.currentResolution.width &&
                 r.height == Screen.currentResolution.height);
        if (curResIdx < 0)
        {
            curResIdx = mResolutions.Length - 1;
        }
        mResolutionDropdown.value = curResIdx;

        /* ─── 4) 리스너 등록 ─── */
        mModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
        mResolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
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
