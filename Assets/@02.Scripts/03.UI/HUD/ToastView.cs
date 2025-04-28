using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ToastView : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image mBGImage;
    [SerializeField] private TMP_Text mText;

    public void SetToastPopup(string msg, Color color = default)
    {
        mText.text = msg;
        if(color != default)
        {
            mBGImage.color = color;
        }
    }
}
