using System.Collections.Generic;
using UIHUDEnums;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ProgressBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform mFillBarMaskRT;
    [SerializeField] private GameObject mFillBar;

    [Header("Padding (0~1)")]
     [Range(0f, 1f)]
    [Tooltip("Background width에서 이 비율만큼 뺀 값으로 FillBar width를 설정합니다.")]
    [SerializeField] private float mWidthPadding;

    [Range(0f, 1f)]
    [Tooltip("Background height에서 이 비율만큼 뺀 값으로 FillBar height를 설정합니다.")]
    [SerializeField] private float mHeightPadding;

    private Image mFillImage;
    private RectTransform mBackgroundRT;
    private RectTransform mFillBarRT;


    private void Awake()
    {
        mBackgroundRT = GetComponent<RectTransform>();
        mFillImage = mFillBar.GetComponent<Image>();
        mFillBarRT = mFillBar.GetComponent<RectTransform>();
    }

    private void Start()
    {
        applyPadding();
    }

    /// <param name="progress"> 0 ~ 1</param>
    public void SetProgress(float progress)
    {
        mFillImage.fillAmount = Mathf.Clamp01(progress);
    }

    private void applyPadding()
    {
        if (mBackgroundRT == null || mFillBarMaskRT == null)
        {
            return;
        }

        Vector2 newSize = mBackgroundRT.sizeDelta;
        newSize.x -= mWidthPadding * mBackgroundRT.rect.width;
        newSize.y -= mHeightPadding * mBackgroundRT.rect.height;
        mFillBarMaskRT.sizeDelta = newSize;
        mFillBarRT.sizeDelta = newSize;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            if (this != null)
            {
                applyPadding();
            }
        };
    }
#endif
}