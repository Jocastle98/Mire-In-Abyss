using System.Collections.Generic;
using System.Linq;
using Events.Quest;
using R3;
using UIHUDEnums;
using UnityEngine;
using UnityEngine.Serialization;

public sealed class QuestBoxPresenter : HudPresenterBase
{
    [Header("Set in Inspector")] [SerializeField]
    RectTransform mContentRoot;

    [SerializeField] QuestCardView mCardPrefab;

    private ObjectPool<QuestCardView> mCardPool = null;
    private Dictionary<int, QuestCardView> mVisibleCards = new();
    private List<TempQuestInfo>  mPendingActive    = new();
    private List<TempQuestInfo>  mPendingComplete  = new();
    private int mMaxCardNumber;

    void Awake()
    {
        mMaxCardNumber = Mathf.FloorToInt(mContentRoot.GetComponent<RectTransform>().rect.height
                               / mCardPrefab.GetComponent<RectTransform>().rect.height);
        mCardPool = new(mCardPrefab, mContentRoot, mMaxCardNumber);
    }

    void Start()
    {
        subscribeEvents();
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<QuestAddedOrUpdated>()
            .Subscribe(e => onQuestAddOrUpdate(e.Info))
            .AddTo(mCD);

        R3EventBus.Instance.Receive<QuestCompleted>()
            .Subscribe(e => onQuestComplete(e.QuestId))
            .AddTo(mCD);
        
        R3EventBus.Instance.Receive<QuestRemoved>()
            .Subscribe(e => onQuestRemove(e.QuestId))
            .AddTo(mCD);
    }


    void onQuestAddOrUpdate(TempQuestInfo info)
    {
        // Completed Quest
        if (info.State == QuestState.Completed)
        {
            onQuestComplete(info.Id);
            return;
        }

        // Quest Update
        if (mVisibleCards.TryGetValue(info.Id, out var card))
        {
            card.Bind(info);
            return;
        }

        int index = mPendingActive.FindIndex(q => q.Id == info.Id);
        if (index >= 0)
        {
            mPendingActive[index] = info;
            return;
        }

        // Quest Add 1. 빈자리 있는 경우
        if (mVisibleCards.Count < mMaxCardNumber)
        {
            spawnCard(info);
            return;
        }

        // 2. 가득 찼는데 Completed 카드가 화면에 있다면 그 자리를 교체
        QuestCardView firstCompleted = findFirstCompletedVisible();
        if (firstCompleted != null)
        {
            moveVisibleCardToCompleted(firstCompleted);
            spawnCard(info);
            return;
        }

        // 3. 빈 자리 없는 경우 Pending 리스트에 추가
        addPending(info);
    }

    void onQuestComplete(int id)
    {
        if (mVisibleCards.TryGetValue(id, out var card))
        {
            card.ApplyQuestComplete();
            card.transform.SetAsLastSibling();
            if(mVisibleCards.Count == mMaxCardNumber && mPendingActive.Count > 0)
            {
                moveVisibleCardToCompleted(card);
                fillVacancyFromPending();
            }
        }
        else
        {
            promotePendingToCompleted(id);
        }
    }

    void onQuestRemove(int id)
    {
        if (mVisibleCards.TryGetValue(id, out var card))
        {
            mCardPool.Return(card);
            mVisibleCards.Remove(id);
            fillVacancyFromPending();
        }
        else
        {
            removeFromPendingList(id);
        }
    }

    /* ============  Helper Methods  =============== */
    QuestCardView spawnCard(TempQuestInfo info)
    {
        var card = mCardPool.Rent();
        card.Bind(info);
        card.transform.SetAsFirstSibling();
        mVisibleCards[info.Id] = card;
        return card;
    }

    QuestCardView findFirstCompletedVisible()
    {
        foreach (var kv in mVisibleCards)
        {
            if (kv.Value.IsCompleted) 
            {
                return kv.Value;
            }
        }
        return null;
    }

    void moveVisibleCardToCompleted(QuestCardView card)
    {
        mVisibleCards.Remove(card.QuestInfo.Id);
        mCardPool.Return(card);
        mPendingComplete.Add(card.QuestInfo);
    }

    void addPending(TempQuestInfo info)
    {
        if (info.State == QuestState.Completed)
        {
             mPendingComplete.Add(info);
        }
        else
        {
             mPendingActive.Add(info);
        }
    }

    void fillVacancyFromPending()
    {
        while (mVisibleCards.Count < mMaxCardNumber)
        {
            TempQuestInfo? next = null;
            if (mPendingActive.Count > 0)  
            {
                 next = mPendingActive.Last();
                 mPendingActive.RemoveAt(mPendingActive.Count - 1);
            }
            else if (mPendingComplete.Count > 0) 
            {
                next = mPendingComplete.Last();
                mPendingComplete.RemoveAt(mPendingComplete.Count - 1);
            }
            else 
            {
                break;
            }

            var card = spawnCard(next.Value);
            card.transform.SetAsFirstSibling();
        }
    }

    void promotePendingToCompleted(int id)
    {
        // Active 리스트 ⇒ Completed 리스트 이동
        int index = mPendingActive.FindIndex(q => q.Id == id);
        if (index >= 0)
        {
            mPendingComplete.Add(mPendingActive[index]);
            mPendingActive[index] = mPendingActive[^1];
            mPendingActive.RemoveAt(mPendingActive.Count - 1);
        }
    }

    void removeFromPendingList(int id)
    {
        int index = mPendingActive.FindIndex(q => q.Id == id);
        if (index >= 0)
        {
            mPendingActive[index] = mPendingActive[^1];
            mPendingActive.RemoveAt(mPendingActive.Count - 1);
            return;
        }

        index = mPendingComplete.FindIndex(q => q.Id == id);
        if (index >= 0)
        {
            mPendingComplete[index] = mPendingComplete[^1];
            mPendingComplete.RemoveAt(mPendingComplete.Count - 1);
            return;
        }
    }

    protected override void OnDisable()
    {
        mCD.Dispose();
        foreach (var c in mVisibleCards.Values) mCardPool.Return(c);
        mVisibleCards.Clear();
        mPendingActive.Clear();
        mPendingComplete.Clear();
    }
}