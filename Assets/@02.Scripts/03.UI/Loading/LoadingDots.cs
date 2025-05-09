using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class LoadingDots : MonoBehaviour
{
    [SerializeField] Image[] mDots;
    [SerializeField] float   mJumpHeight = 22f;
    [SerializeField] float   mJumpTime   = .25f;
    [SerializeField] float   mFadeTime   = .10f;
    [SerializeField] float   mInterval   = .15f;   // 점 간 딜레이
    [SerializeField] float   mDimAlpha   = .5f;    // 기본 투명도
    [SerializeField] float   mLitAlpha   = 1f;     // 점화 시 투명도

    Tween mLoopSeq;

    void Start()
    {
        /* 0) 초기 투명도 ↓ */
        foreach (var d in mDots)
            d.color = new Color(1, 1, 1, mDimAlpha);

        /* 1) 메인 시퀀스 하나만 만들기 -------------------------------- */
        var seq = DOTween.Sequence()
                         .SetUpdate(true)          // TimeScale 0 도 재생
                         .SetLink(gameObject)      // Overlay 파괴 시 자동 Kill
                         .SetEase(Ease.Linear);

        float segment = mInterval + mJumpTime * 2;
        for (int i = 0; i < mDots.Length; ++i)
        {
            float start = i * segment;            // dot i 시작 시각
            var rt = mDots[i].rectTransform;
            float y0 = rt.anchoredPosition.y;

            /*   ── 위로 점프 & 점화 ────────────────────────────── */
            seq.Insert(start,
                mDots[i].DOFade(mLitAlpha, mFadeTime));

            seq.Insert(start,
                rt.DOAnchorPosY(y0 + mJumpHeight, mJumpTime)
                  .SetEase(Ease.OutQuad));

            /*   ── 아래로, 동시에 불 끔 ───────────────────────── */
            float downStart = start + mJumpTime;

            seq.Insert(downStart,
                rt.DOAnchorPosY(y0, mJumpTime)
                  .SetEase(Ease.InQuad));

            seq.Insert(downStart,
                mDots[i].DOFade(mDimAlpha, mFadeTime));
        }

        seq.AppendInterval(mInterval);

        seq.SetLoops(-1, LoopType.Restart);        // 무한 반복
        mLoopSeq = seq;
    }

    void OnDestroy()
    {
        mLoopSeq?.Kill();
    }
}
