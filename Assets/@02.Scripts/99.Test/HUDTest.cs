using System;
using Events.Abyss;
using Events.Combat;
using Events.HUD;
using Events.Item;
using Events.Player;
using Events.Quest;
using UIHUDEnums;
using UnityEngine;
using UnityEngine.Serialization;

public class HUDTest : MonoBehaviour
{
    [SerializeField] private CanvasGroup mTestButtonGroup;
    [Header("Minimap")]
    [SerializeField] private GameObject mEnemyDummy;
    [SerializeField] private GameObject mPortalDummy;
    [SerializeField] private GameObject mShopDummy;
    [SerializeField] private GameObject mBossDummy;
    [Header("Difficulty")]
    [SerializeField] private int mDifficultyLevel = 1;
    [SerializeField] private float mDifficultyProgress = 0;

    [Header("Quest")]
    [SerializeField] private string mQuestTitle;
    [SerializeField] private string mQuestDesc;

    [Header("QuestUpdate")]
    [SerializeField] private int mUpdatedQuestID;
    [SerializeField] private string mUpdatedQuestTitle;
    [SerializeField] private string mUpdatedQuestDesc;
    [SerializeField] private QuestState mUpdatedQuestState;

    [Header("Currency")]
    [SerializeField] private int mGold;
    [SerializeField] private int mSoul;

    [Header("Boss")]
    [SerializeField] private int mBossID;
    [SerializeField] private string mBossName;
    [SerializeField] private string mBossSubName;
    [SerializeField] private int mBossMaxHp;
    [SerializeField] private int mBossCurrentHp;

    [Header("Player")]
    [SerializeField] private int mPlayerCurrentHp;
    [SerializeField] private int mPlayerMaxHp;
    [SerializeField] private int mPlayerCurrentExp;
    [SerializeField] private int mPlayerMaxExp;
    [SerializeField] private int mPlayerLevel;
    [SerializeField] private int mBuffID;
    [SerializeField] private int mBuffDuration;
    [SerializeField] private bool mBuffIsDebuff;

    [Header("Toast")]
    [SerializeField] private string mToastMessage;

    [Header("Item")]
    [SerializeField] private int mItemID;
    [SerializeField] private int mItemCount;

    private bool mIsTestButtonGroupActive = false;
    DateTime mStartUtc;
    private int mLastQuestID = -1;
    private bool mIsSpawned = false;



    private void Start()
    {
        mStartUtc = DateTime.UtcNow;

        mTestButtonGroup.alpha = 0;
        mTestButtonGroup.interactable = false;
        mTestButtonGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        difficultyTestProgress();
        skillTestProgress();
    }

    private void difficultyTestProgress()
    {
        mDifficultyProgress += Time.deltaTime * 0.3f;
        if (mDifficultyProgress >= 1)
        {
            mDifficultyProgress = 0;
            mDifficultyLevel++;
            R3EventBus.Instance.Publish(new DifficultyChanged(mDifficultyLevel));
        }

        R3EventBus.Instance.Publish(new DifficultyProgressed(mDifficultyProgress));

        TimeSpan elapsed = DateTime.UtcNow - mStartUtc;
        R3EventBus.Instance.Publish(new PlayTimeChanged(elapsed));
    }

    private void skillTestProgress()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            R3EventBus.Instance.Publish(new SkillUsed(4));
        }
        if(Input.GetKeyDown(KeyCode.F))
        {
            R3EventBus.Instance.Publish(new SkillUsed(5));
        }
    }
    public void OnTestButtonToggle()
    {
        mIsTestButtonGroupActive = !mIsTestButtonGroupActive;
        mTestButtonGroup.alpha = mIsTestButtonGroupActive ? 1 : 0;
        mTestButtonGroup.interactable = mIsTestButtonGroupActive;
        mTestButtonGroup.blocksRaycasts = mIsTestButtonGroupActive;
    }

    public void OnToggleMinimapIcon()
    {
        if (mIsSpawned)
        {
            mIsSpawned = false;
            R3EventBus.Instance.Publish(new EnemyDied(mEnemyDummy.transform));
            R3EventBus.Instance.Publish(new BossDied(mBossDummy.transform));
            R3EventBus.Instance.Publish(new PortalClosed(mPortalDummy.transform));
            R3EventBus.Instance.Publish(new ShopClosed(mShopDummy.transform));
        }
        else
        {
            mIsSpawned = true;
            R3EventBus.Instance.Publish(new EnemySpawned(mEnemyDummy.transform));
            R3EventBus.Instance.Publish(new BossSpawned(mBossDummy.transform));
            R3EventBus.Instance.Publish(new PortalSpawned(mPortalDummy.transform));
            R3EventBus.Instance.Publish(new ShopSpawned(mShopDummy.transform));
        }
    }

    public void OnDifficultyLevelUp()
    {
        mDifficultyLevel++;
        mDifficultyProgress = 0;
        R3EventBus.Instance.Publish(new DifficultyChanged(mDifficultyLevel));
        R3EventBus.Instance.Publish(new DifficultyProgressed(mDifficultyProgress));
    }

    public void OnAddQuest()
    {
        mLastQuestID++;
        TempQuestInfo questInfo = new TempQuestInfo(mLastQuestID, mQuestTitle, mQuestDesc, QuestState.Active);
        R3EventBus.Instance.Publish(new QuestAddedOrUpdated(questInfo));
    }

    public void OnQuestUpdated()
    {
        if (mUpdatedQuestState == QuestState.Completed)
        {
            OnQuestCompleted();
            return;
        }

        TempQuestInfo questInfo = new TempQuestInfo(mUpdatedQuestID, mUpdatedQuestTitle, mUpdatedQuestDesc, QuestState.Active);
        R3EventBus.Instance.Publish(new QuestAddedOrUpdated(questInfo));
    }

    public void OnQuestCompleted()
    {
        R3EventBus.Instance.Publish(new QuestCompleted(mUpdatedQuestID));
    }

    public void OnRemoveQuest()
    {
        R3EventBus.Instance.Publish(new QuestRemoved(mUpdatedQuestID));
    }

    public void OnCurrencyChanged()
    {
        R3EventBus.Instance.Publish(new CurrencyChanged(mGold, mSoul));
    }

    public void OnBossHpChanged()
    {
        R3EventBus.Instance.Publish(new BossHpChanged(mBossID, mBossName, mBossSubName, mBossMaxHp, mBossCurrentHp));
    }

    public void OnBossDisengage()
    {
        R3EventBus.Instance.Publish(new BossDisengage(mBossID));
    }

    public void OnPlayerHpChanged()
    {
        R3EventBus.Instance.Publish(new PlayerHpChanged(mPlayerCurrentHp, mPlayerMaxHp));
    }

    public void OnPlayerExpChanged()
    {
        R3EventBus.Instance.Publish(new PlayerExpChanged(mPlayerCurrentExp, mPlayerMaxExp));
    }

    public void OnPlayerLevelChanged()
    {
        R3EventBus.Instance.Publish(new PlayerLevelChanged(mPlayerLevel));
    }

    public void OnBuffAdded()
    {
        R3EventBus.Instance.Publish(new BuffAdded(mBuffID, mBuffDuration, mBuffIsDebuff));
    }

    public void OnToastPopup()
    {
        R3EventBus.Instance.Publish(new ToastPopup(mToastMessage));
    }

    public void OnItemAdded()
    {
        R3EventBus.Instance.Publish(new ItemAdded(mItemID, mItemCount));
    }

    public void OnItemSubTracked()
    {
        R3EventBus.Instance.Publish(new ItemSubTracked(mItemID, mItemCount));
    }
}