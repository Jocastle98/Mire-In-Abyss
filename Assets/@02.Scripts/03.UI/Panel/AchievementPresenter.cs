using System;
using System.Collections.Generic;
using AchievementStructs;
using UnityEngine;

public sealed class AchievementPresenter : TabPresenterBase
{
    [SerializeField] RectTransform mContent;
    [SerializeField] AchievementCardView mCardPrefab;

    private ObjectPool<AchievementCardView> mPool;

    void Awake()
    {
        mPool = new(mCardPrefab, mContent, 32);
    }

    public override void Initialize()
    {
        Rebuild();
    }

    void Rebuild()
    {
        foreach (Transform c in mContent)
        {
            mPool.Return(c.GetComponent<AchievementCardView>());
        }

        var achievements = GameDB.Instance.AchievementDatabase.AllAchievements;
        foreach (var rec in achievements)
        {
            var card = mPool.Rent();
            card.Bind(rec, UserData.Instance.GetAchievementData(rec.Id));
            card.transform.SetAsLastSibling();
        }
    }
}
