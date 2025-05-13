using UnityEngine;
using UnityEngine.UI;

public sealed class SoundPresenter : TabPresenterBase
{
    [Header("Mute Toggles")]
    [SerializeField] private Toggle mMasterToggle;
    [SerializeField] private Toggle mBgmToggle;
    [SerializeField] private Toggle mSeToggle;
    [SerializeField] private Toggle mUiToggle;

    [Header("Volume Sliders")]
    [SerializeField] private Slider mMasterVol;
    [SerializeField] private Slider mBgmVol;
    [SerializeField] private Slider mSeVol;
    [SerializeField] private Slider mUiVol;

    public override void Initialize()
    {
        mMasterToggle.isOn = UserData.Instance.IsMasterMuted;
        mBgmToggle   .isOn = UserData.Instance.IsBgmMuted;
        mSeToggle    .isOn = UserData.Instance.IsSeMuted;
        mUiToggle    .isOn = UserData.Instance.IsUiMuted;

        mMasterVol.value = UserData.Instance.MasterVolume;
        mBgmVol.value = UserData.Instance.BgmVolume;
        mSeVol.value = UserData.Instance.SeVolume;
        mUiVol.value = UserData.Instance.UiVolume;

        mMasterToggle.onValueChanged.AddListener(e => UserData.Instance.IsMasterMuted = e);
        mBgmToggle   .onValueChanged.AddListener(e => UserData.Instance.IsBgmMuted = e);
        mSeToggle    .onValueChanged.AddListener(e => UserData.Instance.IsSeMuted = e);
        mUiToggle    .onValueChanged.AddListener(e => UserData.Instance.IsUiMuted = e);

        mMasterVol.onValueChanged.AddListener(e => UserData.Instance.MasterVolume = e);
        mBgmVol.onValueChanged.AddListener(e => UserData.Instance.BgmVolume = e);
        mSeVol.onValueChanged.AddListener(e => UserData.Instance.SeVolume = e);
        mUiVol.onValueChanged.AddListener(e => UserData.Instance.UiVolume = e);
    }
}