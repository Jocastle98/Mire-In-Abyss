using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Events.Player.Modules;
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
        R3EventBus.Instance.Receive<ItemSubtracked>()
            .Subscribe(e => subTrack(e))
            .AddTo(mCD);
    }

    private void addItem(ItemAdded itemInfo)
    {
        // 미니 아이템 뷰가 가득 찼을 경우 대기열에 추가
        if (mSlotsMap.Count >= mMaxMiniViewCount)
        {
            mPendingItems[itemInfo.ID] = itemInfo.Total;
            return;
        }

        if (!mSlotsMap.ContainsKey(itemInfo.ID))
        {
            var slot = mPool.Rent();
            slot.Bind(itemInfo.ID, itemInfo.Total);
            mSlotsMap[itemInfo.ID] = slot;
        }
        else
        {
            mSlotsMap[itemInfo.ID].SetCount(itemInfo.Total);
        }
    }

    private void subTrack(ItemSubtracked itemInfo)
    {
        if (mSlotsMap.ContainsKey(itemInfo.ID))
        {
            if (itemInfo.Total <= 0)
            {
                mPool.Return(mSlotsMap[itemInfo.ID]);
                mSlotsMap.Remove(itemInfo.ID);
                tryFlushPendingItems();
            }
            else
            {
                mSlotsMap[itemInfo.ID].SetCount(itemInfo.Total);
            }
        }
        else
        {
            mPendingItems[itemInfo.ID] = itemInfo.Total;
        }
    }

    private void tryFlushPendingItems()
    {
        while (mSlotsMap.Count < mMaxMiniViewCount && mPendingItems.Count > 0)
        {
            var itemInfo = mPendingItems.First();
            addItem(new ItemAdded(itemInfo.Key, itemInfo.Value, itemInfo.Value));
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
        mPendingItems.Clear();
    }
}
