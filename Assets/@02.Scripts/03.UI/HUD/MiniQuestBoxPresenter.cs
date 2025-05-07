using System.Collections.Generic;
using System.Linq;
using Events.Player.Modules;
using R3;
using UnityEngine;

public sealed class MiniQuestBoxPresenter : HudPresenterBase
{
    class PendingQuestInfo
    {
        public int ID;
        public int Progress;
        public int Target;
        public bool IsCompleted => Progress >= Target;
    }

    [Header("Set in Inspector")] [SerializeField]
    RectTransform mContentRoot;

    [SerializeField] MiniQuestCardView mCardPrefab;

    private ObjectPool<MiniQuestCardView> mCardPool = null;
    private Dictionary<int, MiniQuestCardView> mVisibleCards = new();
    private Dictionary<int, PendingQuestInfo>  mPendingActive    = new();
    private Dictionary<int, PendingQuestInfo>  mPendingComplete  = new();
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
        R3EventBus.Instance.Receive<QuestAccepted>()
            .Subscribe(e => onQuestAccepted(e))
            .AddTo(mCD);

        R3EventBus.Instance.Receive<QuestUpdated>()
            .Subscribe(e => onQuestUpdated(e))
            .AddTo(mCD);

        R3EventBus.Instance.Receive<QuestCompleted>()
            .Subscribe(e => onQuestComplete(e.ID))
            .AddTo(mCD);
        
        R3EventBus.Instance.Receive<QuestRewarded>()
            .Subscribe(e => onQuestRemove(e.ID))
            .AddTo(mCD);
    }

    private void onQuestAccepted(QuestAccepted e)
    {
        // Quest Add 1. 빈자리 있는 경우
        if (mVisibleCards.Count < mMaxCardNumber)
        {
            spawnCard(e.ID, e.Cur, e.Target);
            return;
        }

        // 2. 가득 찼는데 Completed 카드가 화면에 있다면 그 자리를 교체
        MiniQuestCardView firstCompleted = findFirstCompletedVisible();
        if (firstCompleted != null)
        {
            moveVisibleCardToCompleted(firstCompleted);
            spawnCard(e.ID, e.Cur, e.Target);
            return;
        }

        // 3. 빈 자리 없는 경우 Pending 리스트에 추가
        addPending(new PendingQuestInfo { ID = e.ID, Progress = e.Cur, Target = e.Target });
    }

    private void onQuestUpdated(QuestUpdated e)
    {
        if (mVisibleCards.TryGetValue(e.ID, out var card))
        {
            card.QuestUpdated(e.Cur);
        }
        else if(mPendingActive.TryGetValue(e.ID, out var pending))
        {
            pending.Progress = e.Cur;
        }
    }

    private void onQuestComplete(int id)
    {
        if (mVisibleCards.TryGetValue(id, out var card))
        {
            card.QuestCompleted();
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

    private void onQuestRemove(int id)
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
    private MiniQuestCardView spawnCard(int id, int progress, int target)
    {
        var card = mCardPool.Rent();
        card.Bind(id, progress, target);
        if(progress >= target)
        {
            card.transform.SetAsLastSibling();
        }
        else
        {
            card.transform.SetAsFirstSibling();
        }
        mVisibleCards[id] = card;
        return card;
    }

    private MiniQuestCardView findFirstCompletedVisible()
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

    private void moveVisibleCardToCompleted(MiniQuestCardView card)
    {
        mVisibleCards.Remove(card.ID);
        addPending(new PendingQuestInfo { ID = card.ID, Progress = card.Progress, Target = card.Target });
        mCardPool.Return(card);
    }

    private void addPending(PendingQuestInfo info)
    {
        if (info.IsCompleted)
        {
            mPendingComplete[info.ID] = info;
        }
        else
        {
            mPendingActive[info.ID] = info;
        }
    }

    private void fillVacancyFromPending()
    {
        while (mVisibleCards.Count < mMaxCardNumber)
        {
            if (mPendingActive.Count > 0)  
            {
                int id = mPendingActive.First().Key;
                mPendingActive.Remove(id);
                spawnCard(id, mPendingActive[id].Progress, mPendingActive[id].Target);
            }
            else if (mPendingComplete.Count > 0) 
            {
                int id = mPendingComplete.First().Key;
                mPendingComplete.Remove(id);
                spawnCard(id, mPendingComplete[id].Progress, mPendingComplete[id].Target);
            }
            else 
            {
                break;
            }
        }
    }

    private void promotePendingToCompleted(int id)
    {
        if (mPendingActive.TryGetValue(id, out var pending))
        {
            mPendingComplete[id] = pending;
            mPendingActive.Remove(id);
        }
    }

    private void removeFromPendingList(int id)
    {
        if (mPendingActive.TryGetValue(id, out _))
        {
            mPendingActive.Remove(id);
        }
        else if (mPendingComplete.TryGetValue(id, out _))
        {
            mPendingComplete.Remove(id);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var c in mVisibleCards.Values) mCardPool.Return(c);
        mVisibleCards.Clear();
        mPendingActive.Clear();
        mPendingComplete.Clear();
    }
}