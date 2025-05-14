using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ControlPresenter : TabPresenterBase
{
    [SerializeField] Slider mMouseSensSlider;
    [SerializeField] TMP_InputField mSensInput;

    public override void Initialize()
    {
        float sens = UserData.Instance.MouseSensitivity;
        mMouseSensSlider.value = sens;
        mSensInput.text        = mMouseSensSlider.value.ToString("0.00");

        /* ─── 이벤트 등록 ─── */
        mMouseSensSlider.onValueChanged.AddListener(v=>
        {
            mSensInput.text = v.ToString("0.00");
            UserData.Instance.MouseSensitivity = v;
        });

        mSensInput.onEndEdit.AddListener(t=>{
            if(float.TryParse(t,out var v))
            {
                mMouseSensSlider.value = v;
                v = mMouseSensSlider.value;
                mSensInput.text = v.ToString("0.00");
                UserData.Instance.MouseSensitivity = v;
            }
        });
    }
}
