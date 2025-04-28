using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Events.HUD;
using Events.Player;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public sealed class PlayerStatusPresenter : HudPresenterBase
{
    [Header("Refs")]
    [SerializeField] ProgressBarUI mHpBarUI;
    [SerializeField] TMP_Text mHpText;
    [SerializeField] ProgressBarUI mExpBarUI;
    [SerializeField] TMP_Text mLevelText;
    [Header("Buff")]
    [SerializeField] RectTransform mBuffRoot;
    [SerializeField] BuffSlotView mBuffSlotPrefab;

    ObjectPool<BuffSlotView> mPool;
    private Dictionary<int, BuffSlotView> mBuffSlots = new();
    private Dictionary<int, Sprite> mBuffIconMap = null;

    void Awake()
    {
        mPool = new(mBuffSlotPrefab, mBuffRoot, 8);
    }

    void Start()
    {
        subscribeEvents();
        setBuffSprites().Forget();
    }

    void subscribeEvents()
    {
        /* ─── HP ─── */
        R3EventBus.Instance.Receive<PlayerHpChanged>()
            .Subscribe(e =>
            {
                mHpBarUI.SetProgress(e.Current / (float)e.Max);
                mHpText.text = $"{e.Current} / {e.Max}";
            })
            .AddTo(mCD);

        /* ─── EXP ─── */
        _ = R3EventBus.Instance.Receive<PlayerExpChanged>()
            .Subscribe(e =>
            {
                mExpBarUI.SetProgress(e.Current / (float)e.Max);
            })
            .AddTo(mCD);

        /* ─── 레벨 텍스트 ─── */
        R3EventBus.Instance.Receive<PlayerLevelChanged>()
            .Subscribe(e => mLevelText.text = e.Level.ToString())
            .AddTo(mCD);

        /* ─── 버프 아이콘 ─── */
        R3EventBus.Instance.Receive<BuffAdded>()
            .Subscribe(e => addBuff(e).Forget())
            .AddTo(mCD);

        R3EventBus.Instance.Receive<BuffEnded>()
            .Subscribe(e => removeBuff(e.ID))
            .AddTo(mCD);
    }

    /* ────── Buff helpers ────── */
    async UniTaskVoid addBuff(BuffAdded buffInfo)
    {
        await setBuffSprites();

        if (mBuffSlots.ContainsKey(buffInfo.ID))
        {
            mBuffSlots[buffInfo.ID].Bind(buffInfo, mBuffIconMap[buffInfo.ID]);
            return;
        }

        var buffSlot = mPool.Rent();
        buffSlot.Bind(buffInfo, mBuffIconMap[buffInfo.ID]);
        mBuffSlots[buffInfo.ID] = buffSlot;
    }

    async UniTask setBuffSprites()
    {
        if (mBuffIconMap != null)
        {
            return;
        }

        var list = new List<Sprite>();
        var handle = Addressables.LoadAssetsAsync<Sprite>
        (
            "Buff_Icons",
            sp => list.Add(sp)
        );
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            mBuffIconMap = new();
            foreach (var sp in list)
            {
                int id = int.Parse(sp.name.Split('_')[1]); // "icon_101"
                mBuffIconMap[id] = sp;
            }
        }
        else
        {
            Debug.LogError("Failed to load buff icons");
        }
        Addressables.Release(handle);
    }

    void removeBuff(int id)
    {
        if (!mBuffSlots.TryGetValue(id, out var buffSlot))
        {
            return;
        }

        mPool.Return(buffSlot);
        mBuffSlots.Remove(id);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var v in mBuffSlots.Values)
        {
            mPool.Return(v);
        }
        mBuffSlots.Clear();
    }
}
