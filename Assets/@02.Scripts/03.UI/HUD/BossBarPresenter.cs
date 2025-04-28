using Cysharp.Threading.Tasks;
using DG.Tweening;
using Events.Combat;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BossBarPresenter : HudPresenterBase
{
    [Header("Refs")]
    [SerializeField] CanvasGroup mRootGroup;
    [SerializeField] TMP_Text mNameText;
    [SerializeField] TMP_Text mSubNameText;
    [SerializeField] TMP_Text mHpText;
    [SerializeField] ProgressBarUI mHpBarUI;

    private int mMaxHp;
    private int mBossID = -1;
    private bool mIsEngaged = false;

    void Awake()
    {
        mRootGroup.alpha = 0;
        mRootGroup.interactable = false;
        mRootGroup.blocksRaycasts = false;
    }

    void Start()
    {
        subscribeEvents();
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<BossHpChanged>()
            .Subscribe(e => UpdateHp(e))
            .AddTo(mCD);

        R3EventBus.Instance.Receive<BossDisengage>()
            .Subscribe(OnDisengage)
            .AddTo(mCD);
    }


    /* ---------- EVENTS ---------- */

    void UpdateHp(BossHpChanged b)
    {
        if (mBossID != b.ID)
        {
            mBossID = b.ID;
            mNameText.text = b.Name;
            mSubNameText.text = b.SubName;
            mMaxHp = b.MaxHp;
        }

        mHpBarUI.SetProgress(b.CurrentHp / (float)mMaxHp);
        mHpText.text = $"{b.CurrentHp} / {mMaxHp}";

        if (!mIsEngaged)
        {
            mIsEngaged = true;
            ShowAsync().Forget();
        }
    }

    void OnDisengage(BossDisengage b)
    {
        if (mBossID != b.ID)
        {
            return;
        }

        mIsEngaged = false;
        mBossID = -1;
        HideAsync().Forget();
    }


    /* ---------- Animations ---------- */
    async UniTaskVoid ShowAsync()
    {
        mRootGroup.interactable = true;
        mRootGroup.blocksRaycasts = true;
        await mRootGroup.DOFade(1f, 0.25f).SetEase(Ease.OutSine).ToUniTask();
    }
    async UniTaskVoid HideAsync()
    {
        await mRootGroup.DOFade(0f, 0.25f).SetEase(Ease.InSine).ToUniTask();
        mRootGroup.interactable = false;
        mRootGroup.blocksRaycasts = false;
        mMaxHp = 0;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
