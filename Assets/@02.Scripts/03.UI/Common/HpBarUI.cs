using System.Collections.Generic;
using DG.Tweening;
using UIHUDEnums;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class HpBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform mFillBarMaskRT;
    [SerializeField] private RectTransform mBackgroundRT;
    [SerializeField] private GameObject mFillBar;
    [SerializeField] private Image mAfterFillBarImage;
    private Image mFillImage;
    private RectTransform mFillBarRT;

    private float mCurrentFillAmount = 1f;
    private Tween bgTween;


    private void Awake()
    {
        mFillImage = mFillBar.GetComponent<Image>();
        mFillBarRT = mFillBar.GetComponent<RectTransform>();
    }

    private void Start()
    {
    }

    private void OnEnable()
    {
        mCurrentFillAmount = 1f;
    }

    /// <param name="progress"> 0 ~ 1</param>
    public void SetProgress(float progress)
    {
        mFillImage.fillAmount = Mathf.Clamp01(progress);
        if (progress < mCurrentFillAmount)
        {
            bgTween?.Kill();
            bgTween = DOTween.To(() => mAfterFillBarImage.fillAmount,
                                 x => mAfterFillBarImage.fillAmount = x,
                                 progress,
                                 0.5f)
                                 .SetDelay(0.5f)
                              .SetEase(Ease.OutSine);
        }
        // 증가 – BG는 즉시 따라잡음
        else
        {
            bgTween?.Kill();
            mAfterFillBarImage.fillAmount = progress;
        }
        mCurrentFillAmount = progress;
    }
}