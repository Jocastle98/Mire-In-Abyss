using UnityEngine;
using UnityEngine.UI;

public sealed class SoundPresenter : TabPresenterBase
{
    [SerializeField] Toggle mMasterToggle, mBgmToggle, mSeToggle, mUiToggle;
    [SerializeField] Slider mMasterVol, mBgmVol, mSeVol, mUiVol;
    public override void Initialize()
    {
        mMasterToggle.isOn = !UserData.Instance.IsMasterOn;
        mBgmToggle.isOn = !UserData.Instance.IsBgmOn;
        mSeToggle.isOn = !UserData.Instance.IsSeOn;
        mUiToggle.isOn = !UserData.Instance.IsUiOn;

        mMasterVol.value = UserData.Instance.MasterVol;
        mBgmVol.value = UserData.Instance.BgmVol;
        mSeVol.value = UserData.Instance.SeVol;
        mUiVol.value = UserData.Instance.UiVol;

        mMasterToggle.onValueChanged.AddListener(v => UserData.Instance.IsMasterOn = !v);
        mBgmToggle.onValueChanged.AddListener(v => UserData.Instance.IsBgmOn = !v);
        mSeToggle.onValueChanged.AddListener(v => UserData.Instance.IsSeOn = !v);
        mUiToggle.onValueChanged.AddListener(v => UserData.Instance.IsUiOn = !v);

        mMasterVol.onValueChanged.AddListener(v => UserData.Instance.MasterVol = v);
        mBgmVol.onValueChanged.AddListener(v => UserData.Instance.BgmVol = v);
        mSeVol.onValueChanged.AddListener(v => UserData.Instance.SeVol = v);
        mUiVol.onValueChanged.AddListener(v => UserData.Instance.UiVol = v);
    }
}
