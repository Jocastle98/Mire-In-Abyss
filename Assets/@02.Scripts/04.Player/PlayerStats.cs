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
    [SerializeField] private float baseMaxHP = 100f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseAttackPower = 10f;
    [SerializeField] private float baseDefence = 0f;
    [SerializeField] private float baseCritChance = 0.05f;

    private float maxHP;
    private float currentHP;
    private float moveSpeed;
    private float attackPower;
    private float defence;
    private float damageReduction;
    private float critChance;
    private float critDamageMultiplier = 1.5f;

    private bool hasReviveAbility = false;
    private bool reviveUsed = false;
    private float lifeStealPercentage = 0f;

    private bool defenceBuff = false;
    private float defenceBuffValue = 0f;
    private float defenceBuffDuration = 0f;
    
    private bool moveSpeedBuff = false;
    private float moveSpeedBuffValue = 0f;
    private float moveSpeedBuffDuration = 0f;
    
    private bool hasSkillReset = false;
    private float skillResetChance = 0f;
    
    private bool hasAoeDamage = false;
    private float aoeDamageValue = 0f;
    private float aoeDamageTimer = 0f;
    
    private bool hasLastStand = false;
    private float lastStandValue = 0f;
    private bool lastStandActive = false;
    private float lastStandDuration = 0f;
    
    private List<(float value, string type)> maxHPModifiers = new List<(float, string)>();
    private List<(float value, string type)> moveSpeedModifiers = new List<(float, string)>();
    private List<(float value, string type)> attackPowerModifiers = new List<(float, string)>();
    private List<(float value, string type)> defenceModifiers = new List<(float, string)>();
    private List<(float value, string type)> critChanceModifiers = new List<(float, string)>();
    private List<(float value, string type)> damageReductionModifiers = new List<(float, string)>();

    private CancellationTokenSource reviveCts;

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
        reviveCts?.Cancel();
        reviveCts?.Dispose();
    }

    private void ResetStats()
    {
        maxHP = baseMaxHP;
        currentHP = maxHP;
        moveSpeed = baseMoveSpeed;
        attackPower = baseAttackPower;
        defence = baseDefence;
        critChance = baseCritChance;
        damageReduction = 0f;

        RecalculateAllStats();
    }
    
    private void RecalculateAllStats()
    {
        RecalculateStat(ref maxHP, baseMaxHP, maxHPModifiers);
        RecalculateStat(ref moveSpeed, baseMoveSpeed, moveSpeedModifiers);
        RecalculateStat(ref attackPower, baseAttackPower, attackPowerModifiers);
        RecalculateStat(ref defence, baseDefence, defenceModifiers);
        RecalculateStat(ref critChance, baseCritChance, critChanceModifiers);
        RecalculateStat(ref damageReduction, 0f, damageReductionModifiers);
        
        // 현재 체력이 최대 체력을 넘지 않도록
        if (currentHP > maxHP)
            currentHP = maxHP;
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
        if (defenceBuff && defenceBuffDuration > 0)
        {
            defenceBuffDuration -= Time.deltaTime;
            if (defenceBuffDuration <= 0)
            {
                defenceBuff = false;
                Debug.Log("방어 버프 종료");
                RecalculateAllStats();
            }
        }
        
        // 이동 속도 버프 업데이트
        if (moveSpeedBuff && moveSpeedBuffDuration > 0)
        {
            moveSpeedBuffDuration -= Time.deltaTime;
            if (moveSpeedBuffDuration <= 0)
            {
                moveSpeedBuff = false;
                Debug.Log("이동 속도 버프 종료");
                RecalculateAllStats();
            }
        }
        
        // Last Stand 버프 업데이트
        if (lastStandActive && lastStandDuration > 0)
        {
            lastStandDuration -= Time.deltaTime;
            if (lastStandDuration <= 0)
            {
                lastStandActive = false;
                Debug.Log("Last Stand 효과 종료");
                RecalculateAllStats();
            }
        }
    }
    
    private void UpdateAoeDamageTimer()
    {
        if (hasAoeDamage)
        {
            aoeDamageTimer -= Time.deltaTime;
            if (aoeDamageTimer <= 0)
            {
                DealAoeDamage();
                aoeDamageTimer = 2f; // 2초마다 발동
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
        
        Debug.Log($"AoE 데미지 발동! 데미지: {aoeDamageValue}");
    }

    public void TakeDamage(float damage)
    {
        if (defenceBuff)
        {
            Debug.Log("방어 버프로 피해 무효화");
            return;
        }

        float finalDamage = damage * (1f - damageReduction);

        if (hasLastStand && !lastStandActive && currentHP - finalDamage <= maxHP * 0.3f)
        {
            lastStandActive = true;
            lastStandDuration = 5f;
            Debug.Log("5초간 HP 고정 및 스킬 강화");

            return;
        }

        currentHP -= finalDamage;

        if (currentHP <= 0)
        {
            if (hasReviveAbility && !reviveUsed)
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
        bool isCritical = Random.value <= critChance;
        float damage = attackPower;

        if (isCritical)
        {
            damage *= critDamageMultiplier;
            Debug.Log("치명타 발동");
        }

        return damage;
    }

    public void OnEnemyKilled()
    {
        if (moveSpeedBuffValue > 0)
        {
            moveSpeedBuff = true;
            moveSpeedBuffDuration = 3f;
            Debug.Log($"적 처치 후 이속 버프 활성화 {moveSpeedBuffValue * 100}%");
            RecalculateAllStats();
        }
    }

    public void OnGuardSuccess()
    {
        if (defenceBuffValue > 0)
        {
            defenceBuff = true;
            defenceBuffDuration = 5f;
            Debug.Log($"가드 성공, 방어 버프 활성화 {defenceBuffValue * 100}%");
            RecalculateAllStats();
        }
    }

    public bool OnSkillUse()
    {
        if (hasSkillReset && Random.value <= skillResetChance)
        {
            Debug.Log("스킬 쿨타임 초기화");
            return true;
        }

        return false;
    }

    public float OnDamageDealt(float damage)
    {
        if (lifeStealPercentage > 0)
        {
            float healAmount = damage * lifeStealPercentage;
            Heal(healAmount);
            Debug.Log($"흡혈 {healAmount} 회복");
            return healAmount;
        }

        return 0;
    }

    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }

    public void Revive()
    {
        reviveUsed = true;
        currentHP = maxHP;
        InvincibleAfterReviveAsync().Forget();
    }

    private async UniTaskVoid InvincibleAfterReviveAsync()
    {
        Debug.Log("부활하여 10초간 무적 상태");

        reviveCts?.Cancel();
        reviveCts?.Dispose();

        reviveCts = new CancellationTokenSource();
        
        bool oldDefenceBuff = defenceBuff;
        defenceBuff = true;


        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: reviveCts.Token);
            defenceBuff = oldDefenceBuff;
            Debug.Log("무적 종료");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("무적 효과가 취소되었습니다.");
            defenceBuff = oldDefenceBuff;
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
        maxHPModifiers.Add((value, type));
        RecalculateAllStats();
    }
    
    /// <summary>
    /// 이동 속도 수정
    /// </summary>
    public void ModifyMoveSpeed(float value, string type)
    {
        moveSpeedModifiers.Add((value, type));
        RecalculateAllStats();
    }
    
    /// <summary>
    /// 공격력 수정
    /// </summary>
    public void ModifyAttackPower(float value, string type)
    {
        attackPowerModifiers.Add((value, type));
        RecalculateAllStats();
    }
    
    /// <summary>
    /// 방어력 수정
    /// </summary>
    public void ModifyDefence(float value, string type)
    {
        defenceModifiers.Add((value, type));
        RecalculateAllStats();
    }
    
    /// <summary>
    /// 치명타 확률 수정
    /// </summary>
    public void ModifyCritChance(float value, string type)
    {
        critChanceModifiers.Add((value, type));
        RecalculateAllStats();
    }
    
    /// <summary>
    /// 데미지 감소율 수정
    /// </summary>
    public void ModifyDamageReduction(float value, string type)
    {
        damageReductionModifiers.Add((value, type));
        RecalculateAllStats();
    }
    #endregion

    #region 특수 효과 활성화/비활성화 메서드
    /// <summary>
    /// 흡혈 설정
    /// </summary>
    public void SetLifeSteal(float percentage)
    {
        lifeStealPercentage = percentage;
    }

    /// <summary>
    /// 흡혈 효과 초기화
    /// </summary>
    public void ResetLifeSteal()
    {
        lifeStealPercentage = 0f;
    }

    /// <summary>
    /// 부활 능력 활성화
    /// </summary>
    public void EnableRevive()
    {
        hasReviveAbility = true;
        reviveUsed = false;
    }
    
    /// <summary>
    /// 부활 능력 비활성화
    /// </summary>
    public void DisableRevive()
    {
        hasReviveAbility = false;
    }

    /// <summary>
    /// 방어 버프 활성화
    /// </summary>
    public void EnableDefenceBuff(float value)
    {
        defenceBuffValue = value;
    }
    
    /// <summary>
    /// 방어 버프 비활성화
    /// </summary>
    public void DisableDefenceBuff()
    {
        defenceBuffValue = 0f;
        defenceBuff = false;
    }
    
    /// <summary>
    /// 이동 속도 버프 활성화
    /// </summary>
    public void EnableMoveSpeedBuff(float value)
    {
        moveSpeedBuffValue = value;
        Debug.Log($"이동 속도 버프 능력 설정: {value * 100}%");
    }
    
    /// <summary>
    /// 이동 속도 버프 비활성화
    /// </summary>
    public void DisableMoveSpeedBuff()
    {
        moveSpeedBuffValue = 0f;
        moveSpeedBuff = false;
    }
    
    /// <summary>
    /// 치명타 데미지 배율 설정
    /// </summary>
    public void SetCritDamageMultiplier(float multiplier)
    {
        critDamageMultiplier = multiplier;
        Debug.Log($"치명타 피해 배율 설정: {multiplier}배");
    }
    
    /// <summary>
    /// 치명타 데미지 배율 초기화
    /// </summary>
    public void ResetCritDamageMultiplier()
    {
        critDamageMultiplier = 1.5f; // 기본값으로 초기화
    }
    
    /// <summary>
    /// 스킬 쿨타임 초기화 능력 활성화
    /// </summary>
    public void EnableSkillReset(float chance)
    {
        hasSkillReset = true;
        skillResetChance = chance;
        Debug.Log($"스킬 쿨타임 초기화 능력 설정: {chance * 100}% 확률");
    }
    
    /// <summary>
    /// 스킬 쿨타임 초기화 능력 비활성화
    /// </summary>
    public void DisableSkillReset()
    {
        hasSkillReset = false;
        skillResetChance = 0f;
    }
    
    /// <summary>
    /// AoE 데미지 능력 활성화
    /// </summary>
    public void EnableAoeDamage(float damage)
    {
        hasAoeDamage = true;
        aoeDamageValue = damage;
        aoeDamageTimer = 2f;
        Debug.Log($"주변 데미지 능력 설정: {damage} 데미지");
    }
    
    /// <summary>
    /// AoE 데미지 능력 비활성화
    /// </summary>
    public void DisableAoeDamage()
    {
        hasAoeDamage = false;
        aoeDamageValue = 0f;
    }
    
    /// <summary>
    /// 라스트 스탠드 능력 활성화
    /// </summary>
    public void EnableLastStand(float value)
    {
        hasLastStand = true;
        lastStandValue = value;
        Debug.Log("Last Stand 능력 활성화");
    }
    
    /// <summary>
    /// 라스트 스탠드 능력 비활성화
    /// </summary>
    public void DisableLastStand()
    {
        hasLastStand = false;
        lastStandValue = 0f;
        lastStandActive = false;
    }
    #endregion

    #region 스탯 가져오기 메서드
    public float GetCurrentHP() => currentHP;
    public float GetMaxHP() => maxHP;
    public float GetMoveSpeed() => moveSpeed;
    public float GetAttackPower() => attackPower;
    public float GetDefence() => defence;
    public float GetCritChance() => critChance;
    public float GetDamageReduction() => damageReduction;
    #endregion
}