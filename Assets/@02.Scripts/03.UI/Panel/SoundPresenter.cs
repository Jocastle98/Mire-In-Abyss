using UnityEngine;
using UnityEngine.UI;

public sealed class SoundPresenter : MonoBehaviour
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

    void Start()
    {
        mMasterToggle.isOn = PlayerPrefs.GetInt(Constants.MasterMuteKey, 0) == 1;
        mBgmToggle   .isOn = PlayerPrefs.GetInt(Constants.BgmMuteKey,   0) == 1;
        mSeToggle    .isOn = PlayerPrefs.GetInt(Constants.SeMuteKey,    0) == 1;
        mUiToggle    .isOn = PlayerPrefs.GetInt(Constants.UiMuteKey,    0) == 1;

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