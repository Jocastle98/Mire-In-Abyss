using UnityEngine;
using UnityEngine.UI;

public sealed class SoundPresenter : TabPresenterBase
{
    [SerializeField] Toggle mMasterToggle, mBgmToggle, mSeToggle, mUiToggle;
    [SerializeField] Slider mMasterVol, mBgmVol, mSeVol, mUiVol;
    public override void Initialize()
    {
        mMasterToggle.isOn = !UserData.Instance.SoundData.IsMasterOn;
        mBgmToggle.isOn = !UserData.Instance.SoundData.IsBgmOn;
        mSeToggle.isOn = !UserData.Instance.SoundData.IsSeOn;
        mUiToggle.isOn = !UserData.Instance.SoundData.IsUiOn;

        mMasterVol.value = UserData.Instance.SoundData.MasterVol;
        mBgmVol.value = UserData.Instance.SoundData.BgmVol;
        mSeVol.value = UserData.Instance.SoundData.SeVol;
        mUiVol.value = UserData.Instance.SoundData.UiVol;

        mMasterToggle.onValueChanged.AddListener(v => UserData.Instance.SoundData.IsMasterOn = !v);
        mBgmToggle.onValueChanged.AddListener(v => UserData.Instance.SoundData.IsBgmOn = !v);
        mSeToggle.onValueChanged.AddListener(v => UserData.Instance.SoundData.IsSeOn = !v);
        mUiToggle.onValueChanged.AddListener(v => UserData.Instance.SoundData.IsUiOn = !v);

        mMasterVol.onValueChanged.AddListener(v => UserData.Instance.SoundData.MasterVol = v);
        mBgmVol.onValueChanged.AddListener(v => UserData.Instance.SoundData.BgmVol = v);
        mSeVol.onValueChanged.AddListener(v => UserData.Instance.SoundData.SeVol = v);
        mUiVol.onValueChanged.AddListener(v => UserData.Instance.SoundData.UiVol = v);
    }
}
