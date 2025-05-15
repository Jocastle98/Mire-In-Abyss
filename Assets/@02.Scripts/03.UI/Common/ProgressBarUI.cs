using System.Collections.Generic;
using UIHUDEnums;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ProgressBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform mFillBarMaskRT;
    [SerializeField] private GameObject mFillBar;


    private Image mFillImage;
    private RectTransform mBackgroundRT;
    private RectTransform mFillBarRT;


    private void Awake()
    {
        mBackgroundRT = GetComponent<RectTransform>();
        mFillImage = mFillBar.GetComponent<Image>();
        mFillBarRT = mFillBar.GetComponent<RectTransform>();
    }

    /// <param name="progress"> 0 ~ 1</param>
    public void SetProgress(float t)
    {
        if (float.IsNaN(t) || float.IsInfinity(t)) t = 0f;
        t = Mathf.Clamp01(t);
        mFillBarRT.localScale = new Vector3(t, 1, 1);
    }
}