using System;
using System.Collections.Generic;
using Achievement;
using UnityEngine;

public sealed class AchievementPresenter : MonoBehaviour
{
    [SerializeField] RectTransform mContent;
    [SerializeField] AchievementCardView mCardPrefab;

    private ObjectPool<AchievementCardView> mPool;

    void Awake()
    {
        mPool = new(mCardPrefab, mContent, 64);
    }

    void OnEnable()
    {
        Rebuild();
    }

    void Rebuild()
    {
        foreach (Transform c in mContent)
        {
            mPool.Return(c.GetComponent<AchievementCardView>());
        }

        //TODO: 업적 정보 가져오기
        // var achievements = GameDatabase.Instance.Achieve.All;

        // 임시 업적 정보
        var achievements = new List<TempAchievementInfo>
        {
            new(0, "업적 1", "업적 1 설명", true, DateTime.Now),
            new(1, "업적 2", "업적 2 설명", true, DateTime.Now),
            new(2, "업적 3", "업적 3 설명", false, DateTime.Now),
            new(3, "업적 4", "업적 4 설명", false, DateTime.Now),
            new(4, "업적 5", "업적 5 설명", false, DateTime.Now),
            new(5, "업적 6", "업적 6 설명", false, DateTime.Now),
            new(6, "업적 7", "업적 7 설명", false, DateTime.Now),
            new(7, "업적 8", "업적 8 설명", false, DateTime.Now),
            new(8, "업적 9", "업적 9 설명", false, DateTime.Now),
            new(9, "업적 10", "업적 10 설명", false, DateTime.Now),
        }; 

        foreach (var rec in achievements)
        {
            var card = mPool.Rent();
            card.Bind(rec);
            card.transform.SetAsLastSibling();
        }
    }
}
