using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerStats : MonoBehaviour
{
    [Header("기본스탯")] 
    [SerializeField] private float mBaseMaxHP = 100f;
    [SerializeField] private float mBaseMoveSpeed = 5f;
    [SerializeField] private float mBaseAttackPower = 10f;
    [SerializeField] private float mBaseDefence = 2f;
    [SerializeField] private float mBaseCritChance = 0.05f;

    private float mMaxHP;
    private float mCurrentHP;
    private float mMoveSpeed;
    private float mAttackPower;
    private float mDefence;
    private float mDamageReduction;
    private float mCritChance;
    private float mCritDamageMultiplier = 1.5f;
    private float mAttackSpeed = 1f;

    //특수효과 관련 변수
    private bool mbHasReviveAbility = false;
    private bool mbReviveUsed = false;
    private int mReviveCount = 0;
    
    private float mLifeStealPercentage = 0f;

    private bool mbDefenceBuff = false;
    private float mDefenceBuffValue = 0f;
    private float mDefenceBuffDuration = 0f;
    private int mDefenceBuffStacks = 0;
    private const int MAX_DEFENCE_BUFF_STACKS = 6;
    
    private bool mbMoveSpeedBuff = false;
    private float mMoveSpeedBuffValue = 0f;
    private float mMoveSpeedBuffDuration = 0f;
    
    private bool mbHasSkillReset = false;
    private float mSkillResetChance = 0f;
    
    private bool mbHasAoeDamage = false;
    private float mAoeDamageValue = 0f;
    private float mAoeDamageTimer = 0f;
    
    private bool mbHasLastStand = false;
    private float mLastStandValue = 0f;
    private bool mbLastStandActive = false;
    private float mLastStandDuration = 0f;

    private float mSoulStoneMultiplier = 1.0f;
    private float mCoolDownReduction = 0.0f;
    private float mItemDropRateBonus = 0.0f;
    private float mExpMultiplier = 1.0f;
    private float mGoldMultiplier = 1.0f;
    
    //퀘스트 용 변수
    private bool mIsLowHealthTracking = false;
    private float mLowHealthStartTime = 0f;
    private bool mLowHealthQuestCompleted = false;
    
    //스탯 변경 추적을 위한 리스트
    private List<(float value, string type)> mMaxHPModifiers = new List<(float, string)>();
    private List<(float value, string type)> mMoveSpeedModifiers = new List<(float, string)>();
    private List<(float value, string type)> mAttackPowerModifiers = new List<(float, string)>();
    private List<(float value, string type)> mDefenceModifiers = new List<(float, string)>();
    private List<(float value, string type)> mCritChanceModifiers = new List<(float, string)>();
    private List<(float value, string type)> mDamageReductionModifiers = new List<(float, string)>();
    private List<(float value, string type)> mAttackSpeedModifiers = new List<(float value, string type)>();

    private CancellationTokenSource mReviveCts;
    
    //이벤트 
    public event Action OnDeath;
    public event Action<float> OnHealthChanged;
    public event Action OnReturnToTown;
    public event Action OnCooldownReduced;

    private void Awake()
    {
        ResetStats();
    }

    private void Start()
    {
        mCurrentHP = mMaxHP;
    }

    private void Update()
    {
        UpdateBuffDuration();
        UpdateAoeDamageTimer();
        CheckLowHealthQuest();
    }

    private void OnDestroy()
    {
        mReviveCts?.Cancel();
        mReviveCts?.Dispose();
    }

    public void ResetStats()
    {
        mMaxHP = mBaseMaxHP;
        mCurrentHP = mMaxHP;
        mMoveSpeed = mBaseMoveSpeed;
        mAttackPower = mBaseAttackPower;
        mDefence = mBaseDefence;
        mCritChance = mBaseCritChance;
        mDamageReduction = 0f;

        RecalculateAllStats();
    }
    
    private void RecalculateAllStats()
    {
        RecalculateStat(ref mMaxHP, mBaseMaxHP, mMaxHPModifiers);
        RecalculateStat(ref mMoveSpeed, mBaseMoveSpeed, mMoveSpeedModifiers);
        RecalculateStat(ref mAttackPower, mBaseAttackPower, mAttackPowerModifiers);
        RecalculateStat(ref mDefence, mBaseDefence, mDefenceModifiers);
        RecalculateStat(ref mCritChance, mBaseCritChance, mCritChanceModifiers);
        RecalculateStat(ref mDamageReduction, 0f, mDamageReductionModifiers);
        RecalculateStat(ref mAttackSpeed, mAttackSpeed, mAttackSpeedModifiers);
        
        // 현재 체력이 최대 체력을 넘지 않도록
        if (mCurrentHP > mMaxHP)
            mCurrentHP = mMaxHP;

        if (mMoveSpeed >= 10f)
        {
            PlayerHub.Instance.QuestLog.AddProgress("Q010", 1);
        }

        if (mDefence >= 10f)
        {
            PlayerHub.Instance.QuestLog.AddProgress("Q002", 1);
        }

        //크리티컬 0~100% 사잇값만 가짐
        mCritChance = Mathf.Clamp01(mCritChance);
        UpdateAttackSpeedToController();
    }
    
    private void RecalculateStat(ref float stat, float baseStat, List<(float value, string type)> modifiers)
    {
        float percentSum = 0f;
        float mulSum = 1f;
        float flatAdd = 0f;
        
        foreach (var modifier in modifiers)
        {
            if (modifier.type == "add" || modifier.type == "percent")
            {
                percentSum += modifier.value;
            }
            else if (modifier.type == "mul" || modifier.type == "multiply")
            {
                mulSum *= (1f + modifier.value);
            }
            else if (modifier.type.ToLower() == "flat")
            {
                flatAdd += modifier.value;
            }
            // fixed와 unique는 여기서 처리하지 않음 (특수 로직에서 처리)
        }
        
        stat = (baseStat + (baseStat * percentSum) + flatAdd) * mulSum;
    }

    private void CheckLowHealthQuest()
    {
        if (mLowHealthQuestCompleted) return;
        float healthPercentage = mCurrentHP / mMaxHP;

        if (healthPercentage <= 0.2)
        {
            if (!mIsLowHealthTracking)
            {
                mIsLowHealthTracking = true;
                mLowHealthStartTime = Time.time;
            }
            else
            {
                float timeInLowHealth = Time.time - mLowHealthStartTime;
                if (timeInLowHealth >= 60f)
                {
                    PlayerHub.Instance.QuestLog.AddProgress("Q008", 1);
                    mLowHealthQuestCompleted = true;
                }
            }
        }
        else
        {
            if (mIsLowHealthTracking)
            {
                mIsLowHealthTracking = false;
            }
        }
    }

    private void UpdateAttackSpeedToController()
    {
        PlayerController player = GetComponent<PlayerController>();
        player.SetAttackSpeed(mAttackSpeed);
    }
    
    private void UpdateBuffDuration()
    {
        // 방어 버프 업데이트
        if (mbDefenceBuff && mDefenceBuffDuration > 0)
        {
            mDefenceBuffDuration -= Time.deltaTime;
            if (mDefenceBuffDuration <= 0)
            {
                mbDefenceBuff = false;
                mDefenceBuffStacks = 0;
                Debug.Log("방어 버프 종료");
                RecalculateAllStats();
            }
        }
        
        // 이동 속도 버프 업데이트
        if (mbMoveSpeedBuff && mMoveSpeedBuffDuration > 0)
        {
            mMoveSpeedBuffDuration -= Time.deltaTime;
            if (mMoveSpeedBuffDuration <= 0)
            {
                mbMoveSpeedBuff = false;
                Debug.Log("이동 속도 버프 종료");

                ModifyMoveSpeed(-mMoveSpeedBuffValue, "percent");
                
                for (int i = 0; i < mMoveSpeedModifiers.Count; i++)
                {
                    if (mMoveSpeedModifiers[i].type == "moveSpeedBuff")
                    {
                        mMoveSpeedModifiers.RemoveAt(i);
                        break;
                    }
                }
                RecalculateAllStats();
            }
        }
        
        // Last Stand 버프 업데이트
        if (mbLastStandActive && mLastStandDuration > 0)
        {
            mLastStandDuration -= Time.deltaTime;
            if (mLastStandDuration <= 0)
            {
                mbLastStandActive = false;
                ModifyAttackPower(-mLastStandValue, "percent");
                Debug.Log("Last Stand 효과 종료");
                RecalculateAllStats();
            }
        }
    }
    
    private void UpdateAoeDamageTimer()
    {
        if (mbHasAoeDamage)
        {
            mAoeDamageTimer -= Time.deltaTime;
            if (mAoeDamageTimer <= 0)
            {
                DealAoeDamage();
                mAoeDamageTimer = 2f; // 2초마다 발동
            }
        }
    }
    
    private void DealAoeDamage()
    {
        int dynamiteCount = GetDynamiteCount();

        if (dynamiteCount <= 0)
        {
            DisableAoeDamage();
            return;
        }

        float totalDamage = mAoeDamageValue * dynamiteCount;
        
        // 주변 적에게 데미지 주는 로직
        // ex) 주변 콜라이더 감지 후 데미지 적용
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5f, LayerMask.GetMask("Enemy"));
        if (hitColliders.Length == 0) return;
        foreach (var hitCollider in hitColliders)
        {
            EnemyBTController enemy = hitCollider.GetComponent<EnemyBTController>();
            if (enemy != null)
            {
                enemy.SetHit((int)totalDamage,4);
            }
        }

        Debug.Log($"AoE 데미지 발동! {hitColliders.Length}명의 적에게 {dynamiteCount}개 다이너마이트로 {totalDamage} 데미지 적용");
    }

    private int GetDynamiteCount()
    {
        int dynamiteItemID = 13;
        if (PlayerHub.Instance.Inventory.Items.TryGetValue(dynamiteItemID, out int count))
        {
            return count;
        }

        return 0;
    }

    /// <summary>
    /// 플레이어가 데미지를 받을 때 호출
    /// </summary>
    /// <param name="damage">받는 데미지 양</param>
    public void TakeDamage(float damage, float overrideReduction = 0.0f)
    {
        if (mbLastStandActive)
        {
            Debug.Log("lastStand 효과로 피해 무효화");
            return;
        }
        
        float finalDamage = damage;
        
        //방어력 적용
        if (mDefence > 0.0f)
        {
            finalDamage = Mathf.Max(damage - mDefence, 1);
        }
        
        //방어 버프 적용(데미지 무효에서 비율 감소로 변경)
        if (mbDefenceBuff)
        {
            finalDamage *= (1.0f - mDefenceBuffValue);
            Debug.Log($"방어 버프로 피해 {mDefenceBuffValue * 100}% 감소, 최종 데미지 {finalDamage}");
        }
        // [방어, 구르기, 패링 성공]으로 발생하는 피해 감소 상태 적용 시 스탯 상의 피해 감소와 별개로 적용
        else if (overrideReduction > 0.0f)
        {
            finalDamage *= (1.0f - overrideReduction);
        }
        else
        {
            finalDamage *= (1.0f - mDamageReduction);
        }

        //15번 아이템 last stand 효과 체크
        if (mbHasLastStand && !mbLastStandActive && mCurrentHP - finalDamage <= mMaxHP * 0.1f)
        {
            mbLastStandActive = true;
            mLastStandDuration = 5f;
            
            ModifyAttackPower(mLastStandValue, "percent");
            Debug.Log("5초간 HP 고정 및 스킬 강화");

            return;
        }

        //데미지 적용
        mCurrentHP -= finalDamage;
        OnHealthChanged?.Invoke(mCurrentHP);
        Debug.Log($"데미지 받음{finalDamage}, 남은체력 {mCurrentHP}/{mMaxHP}");

        //사망 처리
        if (mCurrentHP <= 0)
        {
            if (mbHasReviveAbility && mReviveCount > 0)
            {
                Revive();
            }
            else
            {
                Die();
            }
        }
    }

    /// <summary>
    /// 공격 데미지를 계산하여 반환(크리티컬 적용)
    /// </summary>
    /// <returns>최종 공격 데미지</returns>
    public float GetAttackDamage()
    {
        bool isCritical = Random.value <= mCritChance;
        float damage = mAttackPower;

        if (isCritical)
        {
            damage *= mCritDamageMultiplier;
            Debug.Log("치명타 발동");
        }

        return damage;
    }

    /// <summary>
    /// 적을 처치했을 때 호출
    /// </summary>
    public void OnEnemyKilled()
    {
        if (mMoveSpeedBuffValue > 0)
        {
            mbMoveSpeedBuff = true;
            mMoveSpeedBuffDuration = 3f;
            Debug.Log($"적 처치 후 이속 버프 활성화 {mMoveSpeedBuffValue * 100}%");

            for (int i = 0; i < mMoveSpeedModifiers.Count; i++)
            {
                if (mMoveSpeedModifiers[i].type == "moveSpeedBuff")
                {
                    mMoveSpeedModifiers.RemoveAt(i);
                    break;
                }
            }
            ModifyMoveSpeed(mMoveSpeedBuffValue, "percent");
            RecalculateAllStats();
        }
    }

    /// <summary>
    /// 가드 성공 시 호출
    /// </summary>
    public void OnGuardSuccess()
    {
        if (mDefenceBuffValue > 0)
        {
            mbDefenceBuff = true;
            mDefenceBuffDuration = 5f;
            Debug.Log($"가드 성공, 방어 버프 활성화 {mDefenceBuffValue * 100}%");
            RecalculateAllStats();
        }

        PlayerHub.Instance.QuestLog.AddProgress("Q003", 1);
    }

    /// <summary>
    /// 스킬 사용시 호출
    /// </summary>
    /// <returns></returns>
    public bool OnSkillUse()
    {
        if (mbHasSkillReset && Random.value <= mSkillResetChance)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 데미지 적용한 후 호출, 흡혈효과
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public float OnDamageDealt(float damage)
    {
        if (mLifeStealPercentage > 0)
        {
            float healAmount = damage * mLifeStealPercentage;
            Heal(healAmount);
            PlayerHub.Instance.QuestLog.AddProgress("Q005", (int)healAmount);
            Debug.Log($"흡혈 {healAmount} 회복");
            return healAmount;
        }

        return 0;
    }

    /// <summary>
    /// 체력을 회복
    /// </summary>
    /// <param name="amount">회복량</param>
    public void Heal(float amount)
    {
        float oldHP = mCurrentHP;
        mCurrentHP += amount;
        
        if (mCurrentHP > mMaxHP)
        {
            mCurrentHP = mMaxHP;
        }

        float actualHeal = mCurrentHP - oldHP;

        if (actualHeal > 0)
        {
            OnHealthChanged?.Invoke(mCurrentHP);
        }
    }

    /// <summary>
    /// 부활 효과 적용
    /// </summary>
    public void Revive()
    {
        mReviveCount--;

        if (mReviveCount <= 0)
        {
            mbHasReviveAbility = false;
            mReviveCount = 0;
        }
        
        mCurrentHP = mMaxHP;
        OnHealthChanged?.Invoke(mCurrentHP);
        PlayerHub.Instance.QuestLog.AddProgress("Q013", 1);
        AchievementManager.Instance.AddProgress("A019", 1);

        Debug.Log($"부활 완료, 남은 부활 횟수 : {mReviveCount}");
    }

    public void DecreaseReviveCount()
    {
        mReviveCount = Mathf.Max(0, mReviveCount - 1);
        if (mReviveCount <= 0)
        {
            mbHasReviveAbility = false;
        }

        Debug.Log($"부활 능력 감소 : 현재 {mReviveCount}회 가능");
    }

    private void Die()
    {
        R3EventBus.Instance.Publish(new Events.HUD.ToastPopup("사망. 5초뒤 마을로 이동합니다.", 2f, Color.red));
        Debug.Log("플레이어 사망!");
        mIsLowHealthTracking = false;
        OnDeath?.Invoke();
        
        Invoke("GoHome",5f);
    }

    private void GoHome()
    {
        AbyssManager.LetsGoHome();
    }
    
    /// <summary>
    /// 지연 시간 후 버프 적용
    /// </summary>
    /// <param name="buffAction"></param>
    /// <param name="delaySeconds"></param>
    /// <param name="cancellationToken"></param>
    public async UniTaskVoid ApplyBuffAfterDelayAsync(Action buffAction, float delaySeconds, CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: cancellationToken);
            buffAction?.Invoke();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("버프 적용이 취소되었습니다.");
        }
    }

    #region 스탯 수정 메서드
    /// <summary>
    /// 최대 체력 수정
    /// </summary>
    public void ModifyMaxHP(float value, string type)
    {
        float oldMaxHP = mMaxHP;
        mMaxHPModifiers.Add((value, type));
        RecalculateAllStats();

        if (mMaxHP > oldMaxHP)
        {
            float increase = mMaxHP - oldMaxHP;
            Heal(increase);
        }
    }
    
    /// <summary>
    /// 이동 속도 수정
    /// </summary>
    public void ModifyMoveSpeed(float value, string type)
    {
        mMoveSpeedModifiers.Add((value, type));
        RecalculateAllStats();
    }
    
    /// <summary>
    /// 공격력 수정
    /// </summary>
    public void ModifyAttackPower(float value, string type)
    {
        mAttackPowerModifiers.Add((value, type));
        RecalculateAllStats();
    }
    
    /// <summary>
    /// 방어력 수정
    /// </summary>
    public void ModifyDefence(float value, string type)
    {
        mDefenceModifiers.Add((value, type));
        RecalculateAllStats();
    }
    
    /// <summary>
    /// 치명타 확률 수정
    /// </summary>
    public void ModifyCritChance(float value, string type)
    {
        mCritChanceModifiers.Add((value, type));
        RecalculateAllStats();
    }
    
    /// <summary>
    /// 데미지 감소율 수정
    /// </summary>
    public void ModifyDamageReduction(float value, string type)
    {
        mDamageReductionModifiers.Add((value, type));
        RecalculateAllStats();
    }

    public void ModifyAttackSpeed(float value, string type)
    {
        mAttackSpeedModifiers.Add((value, type));
        RecalculateAllStats();
    }

    public void ResetStatsFromAbyssToTown()
    {
        ResetAllEffects();              //모든 버프, 특수효과 초기화
        
        PlayerHub.Instance.Inventory.ResetItems();
        
        ResetStats();                   //기본 스탯 초기화
        
        Heal(GetMaxHP());        //최대 체력으로 회복
        
        OnReturnToTown?.Invoke();
    }

    public void ResetAllEffects()
    {
        ResetLifeSteal();
        DisableRevive();
        DisableDefenceBuff();
        DisableMoveSpeedBuff();
        ResetCritDamageMultiplier();
        DisableSkillReset();
        DisableAoeDamage();
        DisableLastStand();
        
        mAttackSpeedModifiers.Clear();
        mDefenceModifiers.Clear();
        mAttackPowerModifiers.Clear();
        mCritChanceModifiers.Clear();
        mDamageReductionModifiers.Clear();
        mMoveSpeedModifiers.Clear();
        mMaxHPModifiers.Clear();
    }
    
    
    public void RemoveLevelBonuses()
    {
        mMaxHPModifiers.RemoveAll(m => m.type == "level");
        mAttackPowerModifiers.RemoveAll(m => m.type == "level");
        mDefenceModifiers.RemoveAll(m => m.type == "level");
        RecalculateAllStats();
    }
    
    
    #endregion

    #region 특수 효과 활성화/비활성화 메서드
    /// <summary>
    /// 흡혈 설정
    /// </summary>
    public void SetLifeSteal(float percentage)
    {
        mLifeStealPercentage = percentage;
    }

    /// <summary>
    /// 흡혈 효과 초기화
    /// </summary>
    public void ResetLifeSteal()
    {
        mLifeStealPercentage = 0f;
    }

    /// <summary>
    /// 부활 능력 활성화
    /// </summary>
    public void EnableRevive()
    {
        mbHasReviveAbility = true;
        mReviveCount++;
        Debug.Log($"부활 능력 활성화 : 현재 {mReviveCount}회 가능");
    }
    
    /// <summary>
    /// 부활 능력 비활성화
    /// </summary>
    public void DisableRevive()
    {
        mReviveCount = 0;
        mbHasReviveAbility = false;
    }

    /// <summary>
    /// 방어 버프 활성화
    /// </summary>
    public void EnableDefenceBuff(float value)
    {
        mDefenceBuffStacks = Mathf.Min(mDefenceBuffStacks + 1, MAX_DEFENCE_BUFF_STACKS);
        mDefenceBuffValue = value * mDefenceBuffStacks;
    }
    
    /// <summary>
    /// 방어 버프 비활성화
    /// </summary>
    public void DisableDefenceBuff()
    {
        mDefenceBuffValue = 0f;
        mbDefenceBuff = false;
        mDefenceBuffStacks = 0;
    }
    
    /// <summary>
    /// 이동 속도 버프 활성화
    /// </summary>
    public void EnableMoveSpeedBuff(float value)
    {
        mMoveSpeedBuffValue = value;
        Debug.Log($"이동 속도 버프 능력 설정: {value * 100}%");
    }
    
    /// <summary>
    /// 이동 속도 버프 비활성화
    /// </summary>
    public void DisableMoveSpeedBuff()
    {
        mMoveSpeedBuffValue = 0f;
        mbMoveSpeedBuff = false;
    }
    
    /// <summary>
    /// 치명타 데미지 배율 설정
    /// </summary>
    public void SetCritDamageMultiplier(float multiplier)
    {
        mCritDamageMultiplier = multiplier;
        Debug.Log($"치명타 피해 배율 설정: {multiplier}배");
    }
    
    /// <summary>
    /// 치명타 데미지 배율 초기화
    /// </summary>
    public void ResetCritDamageMultiplier()
    {
        mCritDamageMultiplier = 1.5f; // 기본값으로 초기화
    }
    
    /// <summary>
    /// 스킬 쿨타임 초기화 능력 활성화
    /// </summary>
    public void EnableSkillReset(float chance)
    {
        mbHasSkillReset = true;
        mSkillResetChance = chance;
        Debug.Log($"스킬 쿨타임 초기화 능력 설정: {chance * 100}% 확률");
    }
    
    /// <summary>
    /// 스킬 쿨타임 초기화 능력 비활성화
    /// </summary>
    public void DisableSkillReset()
    {
        mbHasSkillReset = false;
        mSkillResetChance = 0f;
    }
    
    /// <summary>
    /// AoE 데미지 능력 활성화
    /// </summary>
    public void EnableAoeDamage(float damage)
    {
        mbHasAoeDamage = true;
        mAoeDamageValue = damage;
        mAoeDamageTimer = 2f;
        Debug.Log($"주변 데미지 능력 설정: {damage} 데미지");
    }
    
    /// <summary>
    /// AoE 데미지 능력 비활성화
    /// </summary>
    public void DisableAoeDamage()
    {
        mbHasAoeDamage = false;
        mAoeDamageValue = 0f;
    }
    
    /// <summary>
    /// 라스트 스탠드 능력 활성화
    /// </summary>
    public void EnableLastStand(float value)
    {
        mbHasLastStand = true;
        mLastStandValue = value;
        Debug.Log("Last Stand 능력 활성화");
    }
    
    /// <summary>
    /// 라스트 스탠드 능력 비활성화
    /// </summary>
    public void DisableLastStand()
    {
        mbHasLastStand = false;
        mLastStandValue = 0f;
        mbLastStandActive = false;
    }
    #endregion

    #region 특수 영혼석 강화

    public void SetSoulStoneMultiplier(float multiplier)
    {
        mSoulStoneMultiplier = 1.0f + multiplier;
    }

    public float GetSoulStoneMultiplier()
    {
        return mSoulStoneMultiplier;
    }

    public void SetCoolDownReduction(float reduction)
    {
        mCoolDownReduction = reduction;
        OnCooldownReduced?.Invoke();
    }

    public float GetCoolDownReduction()
    {
        return mCoolDownReduction;
    }

    public void SetItemDropRateBonus(float bonus)
    {
        mItemDropRateBonus = bonus;
    }

    public float GetItemDropRateBonus()
    {
        return mItemDropRateBonus;
    }

    public void SetExpMultiplier(float multiplier)
    {
        mExpMultiplier = 1.0f + multiplier;

        var levelController = GetComponent<PlayerLevelController>();
        if (levelController != null)
        {
            levelController.SetExpMultiplier(mExpMultiplier);
        }
    }

    public float GetExpMultiplier()
    {
        return mExpMultiplier;
    }

    public void SetGoldMultiplier(float multiplier)
    {
        mGoldMultiplier = 1.0f + multiplier;
    }

    public float GetGoldMultiplier()
    {
        return mGoldMultiplier;
    }
    
    #endregion

    #region 스탯 가져오기 메서드
    public float GetCurrentHP() => mCurrentHP;
    public float GetMaxHP() => mMaxHP;
    public float GetMoveSpeed() => mMoveSpeed;
    public float GetAttackPower() => mAttackPower;
    public float GetDefence() => mDefence;
    public float GetCritChance() => mCritChance;
    public float GetDamageReduction() => mDamageReduction;
    public float GetAttackSpeed() => mAttackSpeed;
    public float GetLifeStealPercentage() => mLifeStealPercentage;
    public int ReviveCount => mReviveCount;

    #endregion
}