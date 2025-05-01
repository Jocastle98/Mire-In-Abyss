using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Events.Item;
using R3;
using UnityEngine;
using UnityEngine.UI;

public sealed class ItemSummaryPresenter : HudPresenterBase
{
    [SerializeField] MiniItemView mSlotPrefab;
    [SerializeField] RectTransform mMiniItemRoot;
    private readonly Dictionary<int, MiniItemView> mSlotsMap = new();
    private Dictionary<int, int> mPendingItems = new();
    private ObjectPool<MiniItemView> mPool;
    private int mMaxMiniViewCount;

    void Awake()
    {
        int rowCount = Mathf.FloorToInt((mMiniItemRoot.rect.width - 20f)   // padding
                                         / mMiniItemRoot.GetComponent<GridLayoutGroup>().cellSize.x);
        int colCount = Mathf.FloorToInt((mMiniItemRoot.rect.height - 80f)   // tap image height and padding
                                         / mMiniItemRoot.GetComponent<GridLayoutGroup>().cellSize.y);
        mMaxMiniViewCount = rowCount * colCount;
        mPool = new(mSlotPrefab, mMiniItemRoot, mMaxMiniViewCount);
    }

    void Start()
    {
        subscribeEvents();
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<ItemAdded>()
            .Subscribe(e => addItem(e))
            .AddTo(mCD);
        R3EventBus.Instance.Receive<ItemSubTracked>()
            .Subscribe(e => subTrack(e))
            .AddTo(mCD);
    }

    private void addItem(ItemAdded itemInfo)
    {
        // 미니 아이템 뷰가 가득 찼을 경우 대기열에 추가
        if (mSlotsMap.Count >= mMaxMiniViewCount)
        {
            if (mPendingItems.TryGetValue(itemInfo.ID, out int existing))
            {
                mPendingItems[itemInfo.ID] = existing + itemInfo.Count;
            }
            else
            {
                mPendingItems[itemInfo.ID] = itemInfo.Count;
            }

            return;
        }

        if (!mSlotsMap.ContainsKey(itemInfo.ID))
        {
            var slot = mPool.Rent();
            slot.Bind(itemInfo.ID, itemInfo.Count);
            mSlotsMap[itemInfo.ID] = slot;
        }
        else
        {
            mSlotsMap[itemInfo.ID].IncreaseCount(itemInfo.Count);
        }
    }

    private void subTrack(ItemSubTracked itemInfo)
    {
        if (mSlotsMap.ContainsKey(itemInfo.ID))
        {
            int newItemCount = mSlotsMap[itemInfo.ID].ItemCount - itemInfo.Count;
            if (newItemCount <= 0)
            {
                mPool.Return(mSlotsMap[itemInfo.ID]);
                mSlotsMap.Remove(itemInfo.ID);
                tryFlushPendingItems();
            }
            else
            {
                mSlotsMap[itemInfo.ID].SubTrack(itemInfo.Count);
            }
        }
    }

    private void tryFlushPendingItems()
    {
        while (mSlotsMap.Count < mMaxMiniViewCount && mPendingItems.Count > 0)
        {
            var itemInfo = mPendingItems.First();
            addItem(new ItemAdded(itemInfo.Key, itemInfo.Value));
            mPendingItems.Remove(itemInfo.Key);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var s in mSlotsMap.Values)
        {
            mPool.Return(s);
        }
        mSlotsMap.Clear();
    }
}
