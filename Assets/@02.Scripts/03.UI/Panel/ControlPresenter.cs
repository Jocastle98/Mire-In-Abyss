using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ControlPresenter : MonoBehaviour
{
    [SerializeField] Slider mMouseSensSlider;
    [SerializeField] TMP_InputField mSensInput;

    void Start()
    {
        /* ─── 초기값 설정 ─── */
        // float sens = GameOption.MouseSensitivity;
        // mMouseSensSlider.value = sens;
        mSensInput.text        = mMouseSensSlider.value.ToString("0.00");

        /* ─── 이벤트 등록 ─── */
        mMouseSensSlider.onValueChanged.AddListener(v=>
        {
            mSensInput.text = v.ToString("0.00");
            //GameOption.MouseSensitivity = v;
        });

        mSensInput.onEndEdit.AddListener(t=>{
            if(float.TryParse(t,out var v))
            {
                mMouseSensSlider.value = v;
                v = mMouseSensSlider.value;
                mSensInput.text = v.ToString("0.00");
                //GameOption.MouseSensitivity = v;
            }
        });
    }
}
