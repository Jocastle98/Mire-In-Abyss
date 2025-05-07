using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Events.Player.Modules;
using UnityEngine;
using R3;

public class ItemEffectSystem : MonoBehaviour
{
    [SerializeField] private ItemDatabase mItemDatabase;
    [SerializeField] private PlayerStats mPlayerStats;

    private void Awake()
    {
        if (mPlayerStats == null)
        {
            mPlayerStats = GetComponent<PlayerStats>();
            if (mPlayerStats == null)
            {
                mPlayerStats = FindObjectOfType<PlayerStats>();
            }
        }

        if (mItemDatabase == null)
        {
            mItemDatabase = GetComponent<ItemDatabase>();
            if (mItemDatabase == null)
            {
                mItemDatabase = FindObjectOfType<ItemDatabase>();
            }
        }
    }

    void Start()
    {
        subscribeEvents();
    }

    void subscribeEvents()
    {
        R3EventBus.Instance.Receive<ItemAdded>()
            .Subscribe(e => AcquireItem(e))
            .AddTo(this);
        R3EventBus.Instance.Receive<ItemSubtracked>()
            .Subscribe(e => RemoveItem(e))
            .AddTo(this);
    }

    #region 아이템 획득 및 제거
    /// <summary>
    /// 아이템을 획득시 호출하는 메서드
    /// 아이템을 획득하고 효과를 적용. 동일 아이템 재획득 시 효과가 중첩.
    /// </summary>
    /// <param name="itemId">획득할 아이템 아이디</param>
    public void AcquireItem(ItemAdded itemInfo)
    {
        Item item = mItemDatabase.GetItemByID(itemInfo.ID);
        if (item == null) return;

        ApplyItemEffect(item);

        Debug.Log($"아이템 획득: {item.ItemName}, 보유 수량: {itemInfo.Total}");
    }

    /// <summary>
    /// 아이템을 상점에 팔때 호출하는 메서드
    /// 아이템, 효과 제거.
    /// </summary>
    /// <param name="itemName">제거할 아이템 이름</param>
    public void RemoveItem(ItemSubtracked itemInfo)
    {
        Item item = mItemDatabase.GetItemByID(itemInfo.ID);
        if (item == null) return;

        RemoveItemEffect(item);
        Debug.Log($"아이템 제거: {item.ItemName}, 남은 수량: {itemInfo.Total}");
    }
    #endregion

    #region 아이템 효과 적용
    /// <summary>
    /// 아이템의 효과를 플레이어에게 적용
    /// </summary>
    /// <param name="item">적용할 아이템</param>
    private void ApplyItemEffect(Item item)
    {
        if (mPlayerStats == null)
        {
            Debug.LogError("playerstat 컴포넌트 할당되지 않음");
            return;
        }

        switch (item.EffectType)
        {
            // Common 등급 아이템 (1-5)
            case "maxHP":
                ApplyStatEffect(item, mPlayerStats.ModifyMaxHP);
                break;

            case "moveSpeed":
                ApplyStatEffect(item, mPlayerStats.ModifyMoveSpeed);
                break;

            case "attackPower":
                ApplyStatEffect(item, mPlayerStats.ModifyAttackPower);
                break;

            case "damageReduction":
                ApplyStatEffect(item, mPlayerStats.ModifyDamageReduction);
                break;

            case "critChance":
                ApplyStatEffect(item, mPlayerStats.ModifyCritChance);
                break;

            // Special 등급 아이템 (6-10)
            // 붉은 월계관은 attackPower 효과를 사용하므로 이미 위에서 처리

            case "lifeSteal":
                mPlayerStats.SetLifeSteal(item.Value);
                break;

            case "defenceBuff":
                mPlayerStats.EnableDefenceBuff(item.Value);
                break;

            case "moveSpeedBuff":
                mPlayerStats.EnableMoveSpeedBuff(item.Value);
                break;

            case "revive":
                mPlayerStats.EnableRevive();
                break;

            // Epic 등급 아이템 (11-15)
            case "critBoost":
                ApplyStatEffect(item, mPlayerStats.ModifyCritChance);
                mPlayerStats.SetCritDamageMultiplier(2f);
                break;

            case "skillReset":
                mPlayerStats.EnableSkillReset(item.Value);
                break;

            case "aoeDamage":
                mPlayerStats.EnableAoeDamage(item.Value);
                break;

            case "lastStand":
                mPlayerStats.EnableLastStand(item.Value);
                break;

            default:
                // 복합 효과(쉼표로 구분된 여러 효과) 처리 (현재는 epic 전설의 대지만 사용)
                if (item.EffectType.Contains(","))
                {
                    string[] effects = item.EffectType.Split(',');
                    for (int i = 0; i < effects.Length; i++)
                    {
                        string effect = effects[i].Trim();
                        if (effect == "maxHP")
                            ApplyStatEffect(item, mPlayerStats.ModifyMaxHP);
                        else if (effect == "defence")
                            ApplyStatEffect(item, mPlayerStats.ModifyDefence);
                    }
                }
                else
                {
                    Debug.LogWarning($"알 수 없는 효과 타입: {item.EffectType}, 아이템: {item.ItemName}");
                }
                break;
        }
    }

    /// <summary>
    /// 아이템 효과를 제거. 중첩된 경우 한 번만 제거
    /// </summary>
    /// <param name="item">제거할 아이템</param>
    private void RemoveItemEffect(Item item)
    {
        if (mPlayerStats == null) return;

        switch (item.EffectType)
        {
            // Common 등급 아이템 (1-5)
            case "maxHP":
                RemoveStatEffect(item, mPlayerStats.ModifyMaxHP);
                break;

            case "moveSpeed":
                RemoveStatEffect(item, mPlayerStats.ModifyMoveSpeed);
                break;

            case "attackPower":
                RemoveStatEffect(item, mPlayerStats.ModifyAttackPower);
                break;

            case "damageReduction":
                RemoveStatEffect(item, mPlayerStats.ModifyDamageReduction);
                break;

            case "critChance":
                RemoveStatEffect(item, mPlayerStats.ModifyCritChance);
                break;

            // Special 등급 아이템 (6-10)
            // 붉은 월계관은 위에서 처리

            case "lifeSteal":
                // 다른 lifeSteal 아이템이 없는 경우에만 제거
                if (!HasOtherItemsOfType(item.ID, "lifeSteal"))
                {
                    mPlayerStats.ResetLifeSteal();
                }
                break;

            case "defenceBuff":
                if (!HasOtherItemsOfType(item.ID, "defenceBuff"))
                {
                    mPlayerStats.DisableDefenceBuff();
                }
                break;

            case "moveSpeedBuff":
                if (!HasOtherItemsOfType(item.ID, "moveSpeedBuff"))
                {
                    mPlayerStats.DisableMoveSpeedBuff();
                }
                break;

            case "revive":
                // 다른 revive 아이템이 없는 경우에만 제거
                if (!HasOtherItemsOfType(item.ID, "revive"))
                {
                    mPlayerStats.DisableRevive();
                }
                break;

            // Epic 등급 아이템 (11-15)
            case "critBoost":
                RemoveStatEffect(item, mPlayerStats.ModifyCritChance);
                if (!HasOtherItemsOfType(item.ID, "critBoost"))
                {
                    mPlayerStats.ResetCritDamageMultiplier();
                }
                break;

            case "skillReset":
                if (!HasOtherItemsOfType(item.ID, "skillReset"))
                {
                    mPlayerStats.DisableSkillReset();
                }
                break;

            case "aoeDamage":
                if (!HasOtherItemsOfType(item.ID, "aoeDamage"))
                {
                    mPlayerStats.DisableAoeDamage();
                }
                break;

            case "lastStand":
                if (!HasOtherItemsOfType(item.ID, "lastStand"))
                {
                    mPlayerStats.DisableLastStand();
                }
                break;

            default:
                // 복합 효과 처리
                if (item.EffectType.Contains(","))
                {
                    string[] effects = item.EffectType.Split(',');
                    for (int i = 0; i < effects.Length; i++)
                    {
                        string effect = effects[i].Trim();
                        if (effect == "maxHP")
                            RemoveStatEffect(item, mPlayerStats.ModifyMaxHP);
                        else if (effect == "defence")
                            RemoveStatEffect(item, mPlayerStats.ModifyDefence);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 특정 효과 타입의 다른 아이템이 있는지 확인합니다.
    /// </summary>
    /// <param name="currentItemID">현재 아이템 이름</param>
    /// <param name="effectType">확인할 효과 타입</param>
    /// <returns>다른 아이템이 있으면 true, 없으면 false</returns>
    private bool HasOtherItemsOfType(int currentItemID, string effectType)
    {
        foreach (var pair in PlayerHub.Instance.Inventory.Items)
        {
            if (pair.Key != currentItemID && pair.Value > 0)
            {
                Item otherItem = mItemDatabase.GetItemByID(pair.Key);
                if (otherItem != null && otherItem.EffectType == effectType)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 기본 스탯 효과를 적용합니다.
    /// </summary>
    /// <param name="item">적용할 아이템</param>
    /// <param name="modifyFunction">적용할 스탯 수정 함수</param>
    private void ApplyStatEffect(Item item, Action<float, string> modifyFunction)
    {
        modifyFunction(item.Value, item.ValueType);
    }

    /// <summary>
    /// 기본 스탯 효과를 제거합니다.
    /// </summary>
    /// <param name="item">제거할 아이템</param>
    /// <param name="modifyFunction">제거할 스탯 수정 함수</param>
    private void RemoveStatEffect(Item item, Action<float, string> modifyFunction)
    {
        // 값 타입에 따라 적절히 제거
        switch (item.ValueType.ToLower())
        {
            case "add":
                modifyFunction(-item.Value, "add");
                break;
            case "multiply":
            case "mul":
                // 1로 나누는 것은 곱하기의 역연산
                modifyFunction(1f / item.Value, "multiply");
                break;
            case "percent":
                // 퍼센트 증가를 제거
                modifyFunction(-item.Value, "percent");
                break;
            default:
                Debug.LogWarning($"알 수 없는 값 타입: {item.ValueType}");
                break;
        }
    }
    #endregion

    #region 공용 메서드
    /// <summary>
    /// 특정 아이템의 보유 수량을 반환
    /// </summary>
    /// <param name="itemID">확인할 아이템 아이디</param>
    /// <returns>해당 아이템의 보유 수량, 없으면 0</returns>
    public int GetItemCount(int itemID)
    {
        if (PlayerHub.Instance.Inventory.Items.TryGetValue(itemID, out int count))
        {
            return count;
        }
        return 0;
    }

    /// <summary>
    /// 모든 획득한 아이템 목록을 반환
    /// </summary>
    /// <returns>아이템 이름과 수량을 포함한 딕셔너리</returns>
    public Dictionary<int, int> GetAllAcquiredItems()
    {
        return new Dictionary<int, int>(PlayerHub.Instance.Inventory.Items);
    }

    /// <summary>
    /// 특정 효과 타입의 아이템을 보유하고 있는지 확인
    /// </summary>
    /// <param name="effectType">확인할 효과 타입</param>
    /// <returns>해당 효과 타입의 아이템이 있으면 true, 없으면 false</returns>
    public bool HasItemWithEffectType(string effectType)
    {
        foreach (var pair in PlayerHub.Instance.Inventory.Items)
        {
            if (pair.Value > 0)
            {
                Item item = mItemDatabase.GetItemByID(pair.Key);
                if (item != null && item.EffectType.Contains(effectType))
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region 디버깅 용
    /// <summary>
    /// 디버깅용: 획득한 모든 아이템을 로그로 출력합니다.
    /// </summary>
    public void DebugPrintAcquiredItems()
    {
        Debug.Log($"===== 획득한 아이템 목록 ({PlayerHub.Instance.Inventory.Items.Count}개) =====");
        foreach (var pair in PlayerHub.Instance.Inventory.Items)
        {
            Item item = mItemDatabase.GetItemByID(pair.Key);
            if (item != null)
            {
                Debug.Log($"이름: {item.ItemName}, 수량: {pair.Value}, 효과: {item.EffectType}, 값: {item.Value}, 유형: {item.ValueType}");
            }
        }
    }
    #endregion
}