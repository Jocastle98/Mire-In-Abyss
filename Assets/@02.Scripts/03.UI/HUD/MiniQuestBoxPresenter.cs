using System.Collections.Generic;
using System.Linq;
using Events.Player.Modules;
using R3;
using UnityEngine;

public sealed class MiniQuestBoxPresenter : HudPresenterBase
{

    [Header("Set in Inspector")]
    [SerializeField]
    RectTransform mContentRoot;

    [SerializeField] MiniQuestCardView mCardPrefab;

    private ObjectPool<MiniQuestCardView> mCardPool = null;
    private Dictionary<string, MiniQuestCardView> mVisibleCards = new();
    private List<string> mPendingActive = new();
    private List<string> mPendingComplete = new();
    private int mMaxCardNumber;

    void Awake()
    {
        mMaxCardNumber = Mathf.FloorToInt(mContentRoot.GetComponent<RectTransform>().rect.height
                               / mCardPrefab.GetComponent<RectTransform>().rect.height);
        mCardPool = new(mCardPrefab, mContentRoot, mMaxCardNumber);
        DisableScene = SceneEnums.GameScene.Town;
    }

    public override void Initialize()
    {
        subscribeEvents();
        var quests = PlayerHub.Instance.QuestLog.GetQuestList();
        foreach (var quest in quests)
        {
            onQuestAccepted(quest);
        }
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<QuestAccepted>()
            .Subscribe(e => onQuestAccepted(e.ID))
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

    private void onQuestAccepted(string id)
    {
        // Quest Add 1. 빈자리 있는 경우
        if (mVisibleCards.Count < mMaxCardNumber)
        {
            spawnCard(id);
            return;
        }

        // 2. 가득 찼는데 Completed 카드가 화면에 있다면 그 자리를 교체
        MiniQuestCardView firstCompleted = findFirstCompletedVisible();
        if (firstCompleted != null)
        {
            moveVisibleCardToCompleted(firstCompleted);
            spawnCard(id);
            return;
        }

        // 3. 빈 자리 없는 경우 Pending 리스트에 추가
        addPending(id);
    }

    private void onQuestUpdated(QuestUpdated e)
    {
        if (mVisibleCards.TryGetValue(e.ID, out var card))
        {
            card.QuestUpdated(e.CurrentAmount);
        }
    }

    private void onQuestComplete(string id)
    {
        if (mVisibleCards.TryGetValue(id, out var card))
        {
            card.QuestCompleted();
            card.transform.SetAsLastSibling();
            if (mVisibleCards.Count == mMaxCardNumber && mPendingActive.Count > 0)
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

    private void onQuestRemove(string id)
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
    private MiniQuestCardView spawnCard(string id)
    {
        var card = mCardPool.Rent();
        Quest quest = PlayerHub.Instance.QuestLog.GetQuest(id);
        card.Bind(id, quest.CurrentAmount, quest.TargetAmount, quest.isCompleted);
        if (quest.CurrentAmount >= quest.TargetAmount)
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
            if (PlayerHub.Instance.QuestLog.GetQuest(kv.Key).isCompleted)
            {
                return kv.Value;
            }
        }
        return null;
    }

    private void moveVisibleCardToCompleted(MiniQuestCardView card)
    {
        mVisibleCards.Remove(card.ID);
        addPending(card.ID);
        mCardPool.Return(card);
    }

    private void addPending(string id)
    {
        Quest quest = PlayerHub.Instance.QuestLog.GetQuest(id);
        if (quest.isCompleted)
        {
            mPendingComplete.Add(id);
        }
        else
        {
            mPendingActive.Add(id);
        }
    }

    private void fillVacancyFromPending()
    {
        while (mVisibleCards.Count < mMaxCardNumber)
        {
            if (mPendingActive.Count > 0)
            {
                string id = mPendingActive.Last();
                mPendingActive.RemoveAt(mPendingActive.Count - 1);
                spawnCard(id);
            }
            else if (mPendingComplete.Count > 0)
            {
                string id = mPendingComplete.Last();
                mPendingComplete.RemoveAt(mPendingComplete.Count - 1);
                spawnCard(id);
            }
            else
            {
                break;
            }
        }
    }

    private void promotePendingToCompleted(string id)
    {
        if (mPendingActive.Contains(id))
        {
            mPendingComplete.Add(id);
            mPendingActive.Remove(id);
        }
    }

    private void removeFromPendingList(string id)
    {
        if (mPendingActive.Contains(id))
        {
            mPendingActive.Remove(id);
        }
        else if (mPendingComplete.Contains(id))
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