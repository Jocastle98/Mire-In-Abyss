using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Events.HUD;
using R3;
using TMPro;
using UnityEngine;

public sealed class ToastPresenter : HudPresenterBase
{
    [Header("Refs")]
    [SerializeField] RectTransform mToastRoot;
    [SerializeField] ToastView mToastPrefab;
    [SerializeField] float mFadeTime = .25f;

    private ObjectPool<ToastView> mPool;
    private Queue<ToastPopup> mPending = new();        // 대기열
    private int mActiveCount;
    private int mMaxActive;

    /* ─────────────────────────────────────── */
    void Awake()
    {
        mMaxActive = Mathf.FloorToInt(mToastRoot.rect.height
                                         / mToastPrefab.GetComponent<RectTransform>().rect.height);
        mPool = new(mToastPrefab, mToastRoot, mMaxActive);
    }

    public override void Initialize()
    {
        subscribeEvents();
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<ToastPopup>()
            .Subscribe(EnqueueToast)
            .AddTo(mCD);
    }

    /* ─────────────────────────────────────── */
    private void EnqueueToast(ToastPopup popup)
    {
        if (mActiveCount < mMaxActive)
        {
            showToastAsync(popup).Forget();
        }
        else
        {
            mPending.Enqueue(popup);
        }
    }

    private async UniTaskVoid showToastAsync(ToastPopup popup)
    {
        mActiveCount++;

        var view = mPool.Rent();
        view.SetToastPopup(popup.Message, popup.Color);
        var cg = view.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        view.transform.SetAsLastSibling();

        // fade in, wait, fade out
        await cg.DOFade(1, mFadeTime).SetEase(Ease.OutQuad).ToUniTask();
        await UniTask.Delay((int)(popup.Duration * 1000));
        await cg.DOFade(0, mFadeTime).SetEase(Ease.InQuad).ToUniTask();

        mPool.Return(view);
        mActiveCount--;

        tryFlushPending();
    }

    /* ─── 빈칸이 있으면 대기열에서 끌어오기 ─── */
    private void tryFlushPending()
    {
        while (mActiveCount < mMaxActive && mPending.Count > 0)
        {
            showToastAsync(mPending.Dequeue()).Forget();
        }
    }

    /* ─────────────────────────────────────── */
    protected override void OnDisable()
    {
        base.OnDisable();
        mPending.Clear();
        mActiveCount = 0;
        foreach (Transform child in mToastRoot)
        {
            mPool.Return(child.GetComponent<ToastView>());
        }
    }
}
