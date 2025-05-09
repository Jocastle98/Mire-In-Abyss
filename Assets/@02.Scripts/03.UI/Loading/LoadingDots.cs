using DG.Tweening;
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

    void Start()
    {
        foreach (var dot in mDots) 
        {
            dot.color = new Color(1,1,1, mDimAlpha);
        }

        var seq = DOTween.Sequence().SetLoops(-1, LoopType.Restart);

        for (int i = 0; i < mDots.Length; ++i)
        {
            var r = mDots[i].rectTransform;
            float y0 = r.anchoredPosition.y;
            int idx = i;

            seq.AppendCallback(() =>
            {
                Sequence s = DOTween.Sequence();
                s.Append(mDots[idx].DOFade(mLitAlpha, mFadeTime));
                s.Join (r.DOAnchorPosY(y0 + mJumpHeight, mJumpTime)
                          .SetEase(Ease.OutQuad));
                s.Append(r.DOAnchorPosY(y0, mJumpTime)
                          .SetEase(Ease.InQuad));
                s.Join(mDots[idx].DOFade(mDimAlpha, mFadeTime));
            });

            seq.AppendInterval(mInterval);
        }
    }
}
