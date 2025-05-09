using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class BootPresenter : MonoBehaviour
{
    [SerializeField] ProgressBarUI mPregressBar;
    [SerializeField] TMP_Text mPercentTxt;

    void Start()
    {
        mPregressBar.SetProgress(0);
        mPercentTxt.text = "0%";
    }

    void Update()
    {
        float p = BootLoader.Progress;                  // 0~1
        mPregressBar.SetProgress(p);
        mPercentTxt.text = (p * 100).ToString("F1") + "%";
    }
}
