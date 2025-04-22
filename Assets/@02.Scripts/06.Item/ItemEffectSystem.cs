using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffectSystem : MonoBehaviour
{
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private PlayerStats playerStats;

    // 획득한 아이템과 그 수량을 저장하는 딕셔너리
    private Dictionary<string, int> acquiredItems = new Dictionary<string, int>();
    
    #region 아이템 획득 및 제거
    /// <summary>
    /// 아이템을 획득시 호출하는 메서드
    /// 아이템을 획득하고 효과를 적용. 동일 아이템 재획득 시 효과가 중첩.
    /// </summary>
    /// <param name="itemName">획득할 아이템 이름</param>
    public void AcquireItem(string itemName)
    {
        Item item = itemDatabase.GetItemByName(itemName);
        if(item == null) return;
        
        // 이미 가지고 있는 아이템이면 수량 증가
        if (acquiredItems.ContainsKey(itemName))
        {
            acquiredItems[itemName]++;
        }
        else
        {
            acquiredItems[itemName] = 1;
        }
        
        ApplyItemEffect(item);
        
        Debug.Log($"아이템 획득: {item.name}, 보유 수량: {acquiredItems[itemName]}");
    }

    /// <summary>
    /// 아이템을 상점에 팔때 호출하는 메서드
    /// 아이템, 효과 제거.
    /// </summary>
    /// <param name="itemName">제거할 아이템 이름</param>
    public void RemoveItem(string itemName)
    {
        Item item = itemDatabase.GetItemByName(itemName);
        if (item == null) return;

        if (acquiredItems.ContainsKey(itemName) && acquiredItems[itemName] > 0)
        {
            acquiredItems[itemName]--;
            RemoveItemEffect(item);
            
            Debug.Log($"아이템 제거: {item.name}, 남은 수량: {acquiredItems[itemName]}");
            
            // 수량이 0이 되면 딕셔너리에서 제거
            if (acquiredItems[itemName] <= 0)
            {
                acquiredItems.Remove(itemName);
            }
        }
        else
        {
            Debug.LogWarning($"제거할 아이템이 없음: {itemName}");
        }
    }
    #endregion

    #region 아이템 효과 적용
    /// <summary>
    /// 아이템의 효과를 플레이어에게 적용
    /// </summary>
    /// <param name="item">적용할 아이템</param>
    private void ApplyItemEffect(Item item)
    {
        if (playerStats == null)
        {
            Debug.LogError("playerstat 컴포넌트 할당되지 않음");
            return;
        }

        switch (item.effectType)
        {
            // Common 등급 아이템 (1-5)
            case "maxHP":
                ApplyStatEffect(item, playerStats.ModifyMaxHP);
                break;
                
            case "moveSpeed":
                ApplyStatEffect(item, playerStats.ModifyMoveSpeed);
                break;
                
            case "attackPower":
                ApplyStatEffect(item, playerStats.ModifyAttackPower);
                break;
                
            case "damageReduction":
                ApplyStatEffect(item, playerStats.ModifyDamageReduction);
                break;
                
            case "critChance":
                ApplyStatEffect(item, playerStats.ModifyCritChance);
                break;
                
            // Special 등급 아이템 (6-10)
            // 붉은 월계관은 attackPower 효과를 사용하므로 이미 위에서 처리
            
            case "lifeSteal":
                playerStats.SetLifeSteal(item.value);
                break;
                
            case "defenceBuff":
                playerStats.EnableDefenceBuff(item.value);
                break;
                
            case "moveSpeedBuff":
                playerStats.EnableMoveSpeedBuff(item.value);
                break;
                
            case "revive":
                playerStats.EnableRevive();
                break;
                
            // Epic 등급 아이템 (11-15)
            case "critBoost":
                ApplyStatEffect(item, playerStats.ModifyCritChance);
                playerStats.SetCritDamageMultiplier(2f);
                break;
                
            case "skillReset":
                playerStats.EnableSkillReset(item.value);
                break;
                
            case "aoeDamage":
                playerStats.EnableAoeDamage(item.value);
                break;
                
            case "lastStand":
                playerStats.EnableLastStand(item.value);
                break;
            
            default:
                // 복합 효과(쉼표로 구분된 여러 효과) 처리 (현재는 epic 전설의 대지만 사용)
                if (item.effectType.Contains(","))
                {
                    string[] effects = item.effectType.Split(',');
                    for (int i = 0; i < effects.Length; i++)
                    {
                        string effect = effects[i].Trim();
                        if (effect == "maxHP")
                            ApplyStatEffect(item, playerStats.ModifyMaxHP);
                        else if (effect == "defence")
                            ApplyStatEffect(item, playerStats.ModifyDefence);
                    }
                }
                else
                {
                    Debug.LogWarning($"알 수 없는 효과 타입: {item.effectType}, 아이템: {item.name}");
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
        if (playerStats == null) return;
        
        switch (item.effectType)
        {
            // Common 등급 아이템 (1-5)
            case "maxHP":
                RemoveStatEffect(item, playerStats.ModifyMaxHP);
                break;
                
            case "moveSpeed":
                RemoveStatEffect(item, playerStats.ModifyMoveSpeed);
                break;
                
            case "attackPower":
                RemoveStatEffect(item, playerStats.ModifyAttackPower);
                break;
                
            case "damageReduction":
                RemoveStatEffect(item, playerStats.ModifyDamageReduction);
                break;
                
            case "critChance":
                RemoveStatEffect(item, playerStats.ModifyCritChance);
                break;
                
            // Special 등급 아이템 (6-10)
            // 붉은 월계관은 위에서 처리
                
            case "lifeSteal":
                // 다른 lifeSteal 아이템이 없는 경우에만 제거
                if (!HasOtherItemsOfType(item.name, "lifeSteal"))
                {
                    playerStats.ResetLifeSteal();
                }
                break;
                
            case "defenceBuff":
                if (!HasOtherItemsOfType(item.name, "defenceBuff"))
                {
                    playerStats.DisableDefenceBuff();
                }
                break;
                
            case "moveSpeedBuff":
                if (!HasOtherItemsOfType(item.name, "moveSpeedBuff"))
                {
                    playerStats.DisableMoveSpeedBuff();
                }
                break;
                
            case "revive":
                // 다른 revive 아이템이 없는 경우에만 제거
                if (!HasOtherItemsOfType(item.name, "revive"))
                {
                    playerStats.DisableRevive();
                }
                break;
                
            // Epic 등급 아이템 (11-15)
            case "critBoost":
                RemoveStatEffect(item, playerStats.ModifyCritChance);
                if (!HasOtherItemsOfType(item.name, "critBoost"))
                {
                    playerStats.ResetCritDamageMultiplier();
                }
                break;
                
            case "skillReset":
                if (!HasOtherItemsOfType(item.name, "skillReset"))
                {
                    playerStats.DisableSkillReset();
                }
                break;
                
            case "aoeDamage":
                if (!HasOtherItemsOfType(item.name, "aoeDamage"))
                {
                    playerStats.DisableAoeDamage();
                }
                break;
                
            case "lastStand":
                if (!HasOtherItemsOfType(item.name, "lastStand"))
                {
                    playerStats.DisableLastStand();
                }
                break;
            
            default:
                // 복합 효과 처리
                if (item.effectType.Contains(","))
                {
                    string[] effects = item.effectType.Split(',');
                    for (int i = 0; i < effects.Length; i++)
                    {
                        string effect = effects[i].Trim();
                        if (effect == "maxHP")
                            RemoveStatEffect(item, playerStats.ModifyMaxHP);
                        else if (effect == "defence")
                            RemoveStatEffect(item, playerStats.ModifyDefence);
                    }
                }
                break;
        }
    }
    
    /// <summary>
    /// 특정 효과 타입의 다른 아이템이 있는지 확인합니다.
    /// </summary>
    /// <param name="currentItemName">현재 아이템 이름</param>
    /// <param name="effectType">확인할 효과 타입</param>
    /// <returns>다른 아이템이 있으면 true, 없으면 false</returns>
    private bool HasOtherItemsOfType(string currentItemName, string effectType)
    {
        foreach (var pair in acquiredItems)
        {
            if (pair.Key != currentItemName && pair.Value > 0)
            {
                Item otherItem = itemDatabase.GetItemByName(pair.Key);
                if (otherItem != null && otherItem.effectType == effectType)
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
        modifyFunction(item.value, item.valueType);
    }
    
    /// <summary>
    /// 기본 스탯 효과를 제거합니다.
    /// </summary>
    /// <param name="item">제거할 아이템</param>
    /// <param name="modifyFunction">제거할 스탯 수정 함수</param>
    private void RemoveStatEffect(Item item, Action<float, string> modifyFunction)
    {
        // 값 타입에 따라 적절히 제거
        switch (item.valueType.ToLower())
        {
            case "add":
                modifyFunction(-item.value, "add");
                break;
            case "multiply":
            case "mul":
                // 1로 나누는 것은 곱하기의 역연산
                modifyFunction(1f / item.value, "multiply");
                break;
            case "percent":
                // 퍼센트 증가를 제거
                modifyFunction(-item.value, "percent");
                break;
            default:
                Debug.LogWarning($"알 수 없는 값 타입: {item.valueType}");
                break;
        }
    }
    #endregion

    #region 공용 메서드
    /// <summary>
    /// 특정 아이템의 보유 수량을 반환
    /// </summary>
    /// <param name="itemName">확인할 아이템 이름</param>
    /// <returns>해당 아이템의 보유 수량, 없으면 0</returns>
    public int GetItemCount(string itemName)
    {
        if (acquiredItems.TryGetValue(itemName, out int count))
        {
            return count;
        }
        return 0;
    }
    
    /// <summary>
    /// 모든 획득한 아이템 목록을 반환
    /// </summary>
    /// <returns>아이템 이름과 수량을 포함한 딕셔너리</returns>
    public Dictionary<string, int> GetAllAcquiredItems()
    {
        return new Dictionary<string, int>(acquiredItems);
    }
    
    /// <summary>
    /// 특정 효과 타입의 아이템을 보유하고 있는지 확인
    /// </summary>
    /// <param name="effectType">확인할 효과 타입</param>
    /// <returns>해당 효과 타입의 아이템이 있으면 true, 없으면 false</returns>
    public bool HasItemWithEffectType(string effectType)
    {
        foreach (var pair in acquiredItems)
        {
            if (pair.Value > 0)
            {
                Item item = itemDatabase.GetItemByName(pair.Key);
                if (item != null && item.effectType.Contains(effectType))
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
        Debug.Log($"===== 획득한 아이템 목록 ({acquiredItems.Count}개) =====");
        foreach (var pair in acquiredItems)
        {
            Item item = itemDatabase.GetItemByName(pair.Key);
            if (item != null)
            {
                Debug.Log($"이름: {item.name}, 수량: {pair.Value}, 효과: {item.effectType}, 값: {item.value}, 유형: {item.valueType}");
            }
        }
    }
    #endregion
}