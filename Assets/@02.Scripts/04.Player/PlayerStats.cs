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
    [SerializeField] private float mBaseDefence = 0f;
    [SerializeField] private float mBaseCritChance = 0.05f;

    private float mMaxHP;
    private float mCurrentHP;
    private float mMoveSpeed;
    private float mAttackPower;
    private float mDefence;
    private float mDamageReduction;
    private float mCritChance;
    private float mCritDamageMultiplier = 1.5f;

    private bool mbHasReviveAbility = false;
    private bool mbReviveUsed = false;
    private float mLifeStealPercentage = 0f;

    private bool mbDefenceBuff = false;
    private float mDefenceBuffValue = 0f;
    private float mDefenceBuffDuration = 0f;
    
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
    
    private List<(float value, string type)> mMaxHPModifiers = new List<(float, string)>();
    private List<(float value, string type)> mMoveSpeedModifiers = new List<(float, string)>();
    private List<(float value, string type)> mAttackPowerModifiers = new List<(float, string)>();
    private List<(float value, string type)> mDefenceModifiers = new List<(float, string)>();
    private List<(float value, string type)> mCritChanceModifiers = new List<(float, string)>();
    private List<(float value, string type)> mDamageReductionModifiers = new List<(float, string)>();

    private CancellationTokenSource mReviveCts;

    private void Start()
    {
        ResetStats();
    }

    private void Update()
    {
        UpdateBuffDuration();
        UpdateAoeDamageTimer();
    }

    private void OnDestroy()
    {
        mReviveCts?.Cancel();
        mReviveCts?.Dispose();
    }

    private void ResetStats()
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
        
        // 현재 체력이 최대 체력을 넘지 않도록
        if (mCurrentHP > mMaxHP)
            mCurrentHP = mMaxHP;
    }
    
    private void RecalculateStat(ref float stat, float baseStat, List<(float value, string type)> modifiers)
    {
        float addSum = 0f;
        float mulSum = 1f;
        
        foreach (var modifier in modifiers)
        {
            if (modifier.type == "add")
            {
                addSum += modifier.value;
            }
            else if (modifier.type == "mul" || modifier.type == "multiply")
            {
                mulSum *= (1f + modifier.value);
            }
            // fixed와 unique는 여기서 처리하지 않음 (특수 로직에서 처리)
        }
        
        stat = (baseStat + baseStat * addSum) * mulSum;
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
        // 주변 적에게 데미지 주는 로직
        // ex) 주변 콜라이더 감지 후 데미지 적용
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                /*// 데미지 처리
                Enemy enemy = hitCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(aoeDamageValue);
                }*/
            }
        }
        
        Debug.Log($"AoE 데미지 발동! 데미지: {mAoeDamageValue}");
    }

    public void TakeDamage(float damage)
    {
        if (mbDefenceBuff)
        {
            Debug.Log("방어 버프로 피해 무효화");
            return;
        }

        float finalDamage = damage * (1f - mDamageReduction);

        if (mbHasLastStand && !mbLastStandActive && mCurrentHP - finalDamage <= mMaxHP * 0.3f)
        {
            mbLastStandActive = true;
            mLastStandDuration = 5f;
            Debug.Log("5초간 HP 고정 및 스킬 강화");

            return;
        }

        mCurrentHP -= finalDamage;

        if (mCurrentHP <= 0)
        {
            if (mbHasReviveAbility && !mbReviveUsed)
            {
                Revive();
            }
            else
            {
                Die();
            }
        }
    }

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

    public void OnEnemyKilled()
    {
        if (mMoveSpeedBuffValue > 0)
        {
            mbMoveSpeedBuff = true;
            mMoveSpeedBuffDuration = 3f;
            Debug.Log($"적 처치 후 이속 버프 활성화 {mMoveSpeedBuffValue * 100}%");
            RecalculateAllStats();
        }
    }

    public void OnGuardSuccess()
    {
        if (mDefenceBuffValue > 0)
        {
            mbDefenceBuff = true;
            mDefenceBuffDuration = 5f;
            Debug.Log($"가드 성공, 방어 버프 활성화 {mDefenceBuffValue * 100}%");
            RecalculateAllStats();
        }
    }

    public bool OnSkillUse()
    {
        if (mbHasSkillReset && Random.value <= mSkillResetChance)
        {
            Debug.Log("스킬 쿨타임 초기화");
            return true;
        }

        return false;
    }

    public float OnDamageDealt(float damage)
    {
        if (mLifeStealPercentage > 0)
        {
            float healAmount = damage * mLifeStealPercentage;
            Heal(healAmount);
            Debug.Log($"흡혈 {healAmount} 회복");
            return healAmount;
        }

        return 0;
    }

    public void Heal(float amount)
    {
        mCurrentHP += amount;
        if (mCurrentHP > mMaxHP)
        {
            mCurrentHP = mMaxHP;
        }
    }

    public void Revive()
    {
        mbReviveUsed = true;
        mCurrentHP = mMaxHP;
        InvincibleAfterReviveAsync().Forget();
    }

    private async UniTaskVoid InvincibleAfterReviveAsync()
    {
        Debug.Log("부활하여 10초간 무적 상태");

        mReviveCts?.Cancel();
        mReviveCts?.Dispose();

        mReviveCts = new CancellationTokenSource();
        
        bool oldDefenceBuff = mbDefenceBuff;
        mbDefenceBuff = true;


        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: mReviveCts.Token);
            mbDefenceBuff = oldDefenceBuff;
            Debug.Log("무적 종료");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("무적 효과가 취소되었습니다.");
            mbDefenceBuff = oldDefenceBuff;
        }
        
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");
    }
    
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
        mMaxHPModifiers.Add((value, type));
        RecalculateAllStats();
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
        mbReviveUsed = false;
    }
    
    /// <summary>
    /// 부활 능력 비활성화
    /// </summary>
    public void DisableRevive()
    {
        mbHasReviveAbility = false;
    }

    /// <summary>
    /// 방어 버프 활성화
    /// </summary>
    public void EnableDefenceBuff(float value)
    {
        mDefenceBuffValue = value;
    }
    
    /// <summary>
    /// 방어 버프 비활성화
    /// </summary>
    public void DisableDefenceBuff()
    {
        mDefenceBuffValue = 0f;
        mbDefenceBuff = false;
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

    #region 스탯 가져오기 메서드
    public float GetCurrentHP() => mCurrentHP;
    public float GetMaxHP() => mMaxHP;
    public float GetMoveSpeed() => mMoveSpeed;
    public float GetAttackPower() => mAttackPower;
    public float GetDefence() => mDefence;
    public float GetCritChance() => mCritChance;
    public float GetDamageReduction() => mDamageReduction;
    #endregion
}