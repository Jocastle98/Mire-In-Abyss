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

        if (b.CurrentHp <= 0)
        {
            mHpBarUI.SetProgress(currentHp / (float)mMaxHp);
            mHpText.text = $"{currentHp} / {mMaxHp}";
            OnDisengage(new BossDisengage(b.ID));
            return;
        }
        else
        {
            mHpBarUI.SetProgress(currentHp / (float)mMaxHp);
            mHpText.text = $"{currentHp} / {mMaxHp}";

            if (!mIsEngaged)
            {
                mIsEngaged = true;
                showAsync().Forget();
            }
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
        hideAsync().Forget();
    }


    /* ---------- Animations ---------- */
    private async UniTaskVoid showAsync()
    {
        mRootGroup.interactable = true;
        mRootGroup.blocksRaycasts = true;
        await mRootGroup.DOFade(1f, 0.25f).SetEase(Ease.OutSine).ToUniTask();
    }
    private async UniTaskVoid hideAsync()
    {
        Debug.Log("Boss Hide Start");
        await mRootGroup.DOFade(0f, 0.15f)                // 부드럽게 사라짐
                     .SetEase(Ease.InSine)
                     .ToUniTask();
        Debug.Log("Boss Hide End");
        mRootGroup.interactable = false;
        mRootGroup.blocksRaycasts = false;
        mMaxHp = -1;     // 0 대신 음수로 두어 Divide by zero 원천 차단
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
