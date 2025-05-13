using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class BackButton : MonoBehaviour, IPointerEnterHandler,
                                    IPointerExitHandler
{
    [SerializeField] Button mButton;
    [SerializeField] Image mOutlineImage;

    private Tween mFadeTween;
    private Action mOnAfterClose;
    public void OnPointerEnter(PointerEventData eventData)
    {
        playFadeTween(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        playFadeTween(0f);
    }

    // Start is called before the first frame update
    void Start()
    {
        mOutlineImage.color = new Color(1, 1, 1, 0);
        mButton.onClick.AddListener(OnClick);
    }

    void playFadeTween(float alpha)
    {
        mFadeTween?.Kill(); // 있던 Tween 제거
        mFadeTween = mOutlineImage.DOFade(alpha, .3f)
                         .SetEase(Ease.OutQuad)
                         .SetLink(gameObject);
    }

    void OnClick()
    {
        UIManager.Instance.Pop().Forget();
        mOnAfterClose?.Invoke();
    }

    public void SetAfterCloseAction(Action onClose)
    {
        mOnAfterClose = onClose;
    }

    void OnDestroy()
    {
        mFadeTween?.Kill();
    }
}
