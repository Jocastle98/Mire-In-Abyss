using System;
using Events.Abyss;
using Events.Combat;
using Events.HUD;
using Events.Player;
using Events.Player.Modules;
using QuestEnums;
using UnityEngine;
using UnityEngine.Serialization;

public class HUDTest : MonoBehaviour
{
    [SerializeField] private CanvasGroup mTestButtonGroup;
    [Header("Minimap")]
    [SerializeField] private TestEnemy mEnemyDummy;
    [SerializeField] private TestPortal mPortalDummy;
    [SerializeField] private TestShop mShopDummy;
    [SerializeField] private TestBoss mBossDummy;
    [Header("Difficulty")]
    [SerializeField] private int mDifficultyLevel = 1;
    [SerializeField] private float mDifficultyProgress = 0;

    [Header("Quest")]
    [SerializeField] private string mQuestTitle;
    [SerializeField] private string mQuestDesc;

    [Header("QuestUpdate")]
    [SerializeField] private string mUpdatedQuestID;
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

    [Header("Combat")]
    [SerializeField] private int mEnemyHp;
    [SerializeField] private int mEnemyMaxHp;
    [SerializeField] private int mWeaponDamageMin;
    [SerializeField] private int mWeaponDamageMax;

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
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            R3EventBus.Instance.Publish(new SkillUsed(6));
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            R3EventBus.Instance.Publish(new SkillUsed(7));
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            R3EventBus.Instance.Publish(new SkillUsed(8));
        }
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            R3EventBus.Instance.Publish(new SkillUsed(9));
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
            TrackableEventHelper.PublishDestroyed(mEnemyDummy);
            TrackableEventHelper.PublishDestroyed(mBossDummy);
            TrackableEventHelper.PublishDestroyed(mPortalDummy);
            TrackableEventHelper.PublishDestroyed(mShopDummy);
        }
        else
        {
            mIsSpawned = true;
            TrackableEventHelper.PublishSpawned(mEnemyDummy);
            TrackableEventHelper.PublishSpawned(mBossDummy);
            TrackableEventHelper.PublishSpawned(mPortalDummy);
            TrackableEventHelper.PublishSpawned(mShopDummy);
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
        PlayerHub.Instance.QuestLog.Accept(mLastQuestID.ToString(), 1);
    }

    public void OnQuestUpdated()
    {
        if (mUpdatedQuestState == QuestState.Completed)
        {
            OnQuestCompleted();
            return;
        }

        PlayerHub.Instance.QuestLog.AddProgress(mUpdatedQuestID, 1);
    }

    public void OnQuestCompleted()
    {
        PlayerHub.Instance.QuestLog.Reward(mUpdatedQuestID);
    }

    public void OnCurrencyChanged()
    {
        PlayerHub.Instance.Inventory.AddGold(mGold);
        PlayerHub.Instance.Inventory.AddSoul(mSoul);
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
        PlayerHub.Instance.BuffController.AddBuff(mBuffID, mBuffDuration, mBuffIsDebuff);
    }

    public void OnToastPopup()
    {
        R3EventBus.Instance.Publish(new ToastPopup(mToastMessage));
    }

    public void OnItemAdded()
    {
        PlayerHub.Instance.Inventory.AddItem(mItemID, mItemCount);
    }

    public void OnItemSubTracked()
    {
        PlayerHub.Instance.Inventory.RemoveItem(mItemID, mItemCount);
    }

    public void OnAttackDummy()
    {
        int damage = UnityEngine.Random.Range(mWeaponDamageMin, mWeaponDamageMax);
        mEnemyHp -= damage;
        R3EventBus.Instance.Publish(new DamagePopup(mEnemyDummy.transform.position, damage));
        if (mEnemyHp < 0)
        {
            mEnemyHp = mEnemyMaxHp;
        }
        R3EventBus.Instance.Publish(new EnemyHpChanged(mEnemyDummy.GetInstanceID(), mEnemyHp, mEnemyMaxHp));
    }
}