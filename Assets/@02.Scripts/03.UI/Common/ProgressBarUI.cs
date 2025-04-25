using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ProgressBarUI : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private GameObject mFillBar;

    [Header("Padding (px)")] [Tooltip("Background width 에서 이만큼 빼서 FillBar width 로 설정")] [SerializeField]
    private float mWidthPadding;

    [Tooltip("Background height 에서 이만큼 빼서 FillBar height 로 설정")] [SerializeField]
    private float mHeightPadding;

    private Image mFillImage;
    private RectTransform mBackgroundRT;
    private RectTransform mFillRT;


    private void Awake()
    {
        mBackgroundRT = GetComponent<RectTransform>();
        mFillRT = mFillBar.GetComponent<RectTransform>();
        mFillImage = mFillBar.GetComponent<Image>();
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
        if (mBackgroundRT == null || mFillRT == null)
        {
            return;
        }

        Vector2 newSize = mBackgroundRT.sizeDelta;
        newSize.x -= mWidthPadding;
        newSize.y -= mHeightPadding;
        mFillRT.sizeDelta = newSize;
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