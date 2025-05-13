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
        mMasterToggle.isOn = UserData.Instance.SoundData.IsMasterMuted;
        mBgmToggle   .isOn = UserData.Instance.SoundData.IsBgmMuted;
        mSeToggle    .isOn = UserData.Instance.SoundData.IsSeMuted;
        mUiToggle    .isOn = UserData.Instance.SoundData.IsUiMuted;

        mMasterVol.value = UserData.Instance.SoundData.MasterVol;
        mBgmVol.value = UserData.Instance.SoundData.BgmVol;
        mSeVol.value = UserData.Instance.SoundData.SeVol;
        mUiVol.value = UserData.Instance.SoundData.UiVol;

        mMasterToggle.onValueChanged.AddListener(AudioManager.Instance.SetMasterMute);
        mBgmToggle   .onValueChanged.AddListener(AudioManager.Instance.SetBgmMute);
        mSeToggle    .onValueChanged.AddListener(AudioManager.Instance.SetSeMute);
        mUiToggle    .onValueChanged.AddListener(AudioManager.Instance.SetUiMute);

        AudioManager.Instance.InitSliders(
            mMasterVol,
            mBgmVol,
            mSeVol,
            mUiVol
        );
    }
}