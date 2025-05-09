using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class BootPresenter : MonoBehaviour
{
    [SerializeField] ProgressBarUI mPregressBar;
    [SerializeField] TMP_Text mPercentTxt;

    void Update()
    {
        /* 진행률 업데이트 */
        // var p = SceneLoader.Progress;                  // 0~1
        // mPregressBar.SetProgress(p);
        // mPercentTxt.text = p * 100.ToString("F1") + "%";
    }
}
