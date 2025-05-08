using UnityEngine;
using UnityEngine.UI;

public sealed class SoundPresenter : MonoBehaviour
{
    [SerializeField] Toggle mMasterToggle, mBgmToggle, mSeToggle, mUiToggle;
    [SerializeField] Slider mMasterVol, mBgmVol, mSeVol, mUiVol;
    void Start()
    {
        /* 초기값 로드 */
        // mMasterToggle.isOn = AudioBus.MasterMute;
        // mBgmToggle.isOn = AudioBus.BgmMute;
        // mSeToggle.isOn = AudioBus.SeMute;
        // mUiToggle.isOn = AudioBus.UiMute;

        // mMasterVol.value = AudioBus.MasterVol;
        // mBgmVol.value = AudioBus.BgmVol;
        // mSeVol.value = AudioBus.SeVol;
        // mUiVol.value = AudioBus.UiVol;

        /* 리스너 */
        // mMasterToggle.onValueChanged.AddListener(v => AudioBus.SetMasterMute(v));
        // mBgmToggle.onValueChanged.AddListener(v => AudioBus.SetBgmMute(v));
        // mSeToggle.onValueChanged.AddListener(v => AudioBus.SetSeMute(v));
        // mUiToggle.onValueChanged.AddListener(v => AudioBus.SetUiMute(v));

        // mMasterVol.onValueChanged.AddListener(AudioBus.SetMasterVol);
        // mBgmVol.onValueChanged.AddListener(AudioBus.SetBgmVol);
        // mSeVol.onValueChanged.AddListener(AudioBus.SetSeVol);
        // mUiVol.onValueChanged.AddListener(AudioBus.SetUiVol);
    }
}
