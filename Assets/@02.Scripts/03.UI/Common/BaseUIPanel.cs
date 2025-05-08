using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine.Serialization;

/// <summary>
/// 모든 UI 패널이 상속받을 베이스 클래스. 기본 Show/Hide 기능 및 초기화 인터페이스 제공
/// </summary>
public abstract class BaseUIPanel : MonoBehaviour
{
    protected readonly CompositeDisposable mDisposables = new();
    public CanvasGroup CG;
    protected virtual void Awake() => CG = GetComponent<CanvasGroup>();

    public virtual async UniTask Show(Action onComplete = null)
    {
        gameObject.SetActive(true);
        CG.alpha = 0;
        await CG.DOFade(1, .2f).ToUniTask();
    }
    public virtual async UniTask Hide(Action onComplete = null)
    {
        await CG.DOFade(0, .15f).ToUniTask();
        mDisposables.Dispose();
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}