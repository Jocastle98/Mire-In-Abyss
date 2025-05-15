using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Events.HUD;
using Events.Player;
using Events.Player.Modules;
using R3;
using TMPro;
using UIEnums;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
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

    private ObjectPool<BuffSlotView> mPool;
    private Dictionary<int, BuffSlotView> mBuffSlots = new();

    void Awake()
    {
        mPool = new(mBuffSlotPrefab, mBuffRoot, 8);
    }

    void OnEnable()
    {
        // 새 씬이 로드된 직후 호출
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 버프 슬롯 풀 반환
        foreach (var v in mBuffSlots.Values)
            mPool.Return(v);
        mBuffSlots.Clear();
    }



    public override void Initialize()
    {
        subscribeEvents();
        mLevelText.text = "1";
        // TODO: Abyss, Town 진입 시 플레이어 HP 정보 받아오기
        mHpText.text = $"{TempRefManager.Instance.PlayerStats.GetCurrentHP()} / {TempRefManager.Instance.PlayerStats.GetMaxHP()}";

        mExpBarUI.SetProgress(0);
        mHpBarUI.SetProgress(TempRefManager.Instance.PlayerStats.GetCurrentHP() / TempRefManager.Instance.PlayerStats.GetMaxHP());
    }

    private void subscribeEvents()
    {
        /* ─── HP ─── */
        R3EventBus.Instance.Receive<PlayerHpChanged>()
            .Subscribe(e =>
            {
                mHpBarUI.SetProgress(e.Current / e.Max);
                mHpText.text = $"{(int)e.Current} / {(int)e.Max}";
            })
            .AddTo(mCD);

        /* ─── EXP ─── */
        R3EventBus.Instance.Receive<PlayerExpChanged>()
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
            .Subscribe(e => addBuff(e))
            .AddTo(mCD);

        R3EventBus.Instance.Receive<BuffRefreshed>()
            .Subscribe(e => refreshBuff(e))
            .AddTo(mCD);

        R3EventBus.Instance.Receive<BuffEnded>()
            .Subscribe(e => removeBuff(e.ID))
            .AddTo(mCD);
    }

    /* ────── Buff helpers ────── */
    private void addBuff(BuffAdded buffInfo)
    {
        if (mBuffSlots.TryGetValue(buffInfo.ID, out var buffSlot))
        {
            buffSlot.Refresh(buffInfo.Duration);
            return;
        }

        var newBuffSlot = mPool.Rent();
        newBuffSlot.Bind(buffInfo);
        mBuffSlots[buffInfo.ID] = newBuffSlot;
    }

    private void refreshBuff(BuffRefreshed buffInfo)
    {
        if (mBuffSlots.TryGetValue(buffInfo.ID, out var buffSlot))
        {
            buffSlot.Refresh(buffInfo.NewRemain);
        }
    }

    private void removeBuff(int id)
    {
        if (!mBuffSlots.TryGetValue(id, out var buffSlot))
        {
            return;
        }

        mPool.Return(buffSlot);
        mBuffSlots.Remove(id);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 던전 씬 진입 시에만 초기화
        if (scene.name == Constants.AbyssDungeonScene ||
            scene.name == Constants.AbyssFieldScene)
        {
            var stats = TempRefManager.Instance.PlayerStats;
            var curHp = stats.GetCurrentHP();
            var maxHp = stats.GetMaxHP();
            mHpText.text = $"{curHp} / {maxHp}";
            mHpBarUI.SetProgress(curHp / (float)maxHp);
        }
    }
}
