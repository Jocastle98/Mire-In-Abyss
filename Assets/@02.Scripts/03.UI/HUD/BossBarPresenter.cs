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
        DisableScene = SceneEnums.GameScene.Town;
    }

    public override void Initialize()
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

    private void UpdateHp(BossHpChanged b)
    {
        int currentHp = b.CurrentHp;

        if (currentHp <= 0)
        {
            currentHp = 0;
        }

        if (mBossID != b.ID)
        {
            mBossID = b.ID;
            //TODO: 보스 이름 정보 가져오기 및 보스 Type struct에 포함시켜 가져오기
            mNameText.text = "다크 드래곤";
            mSubNameText.text = "심연의 군주";
            mMaxHp = b.MaxHp;
        }
        else
        {
            if (b.CurrentHp <= 0)
            {
                OnDisengage(new BossDisengage(b.ID));
            }
        }

        // ─── Divide-by-zero 방지 ───
        float progress = (mMaxHp > 0) ? b.CurrentHp / (float)mMaxHp : 0f;
        progress = Mathf.Clamp01(progress);          // 혹시 모를 1 초과/NaN 보호

        mHpBarUI.SetProgress(progress);
        mHpText.text = $"{b.CurrentHp} / {mMaxHp}";

        if (!mIsEngaged)
        {
            mIsEngaged = true;
            showAsync().Forget();
        }
    }

    private void OnDisengage(BossDisengage b)
    {
        if (mBossID != b.ID)
        {
            return;
        }

        mIsEngaged = false;
        mBossID = -1;
        hide();
    }


    /* ---------- Animations ---------- */
    private async UniTaskVoid showAsync()
    {
        mRootGroup.interactable = true;
        mRootGroup.blocksRaycasts = true;
        await mRootGroup.DOFade(1f, 0.25f).SetEase(Ease.OutSine).ToUniTask();
    }
    private void hide()
    {
        mRootGroup.alpha = 0;
        mRootGroup.interactable = false;
        mRootGroup.blocksRaycasts = false;
        mMaxHp = -1;     // 0 대신 음수로 두어 Divide by zero 원천 차단
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
