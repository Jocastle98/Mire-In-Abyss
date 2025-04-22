using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffectSystem : MonoBehaviour
{
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private PlayerStats playerStats;

    private List<Item> equippedItems = new List<Item>();

    public void EquipItem(string itemName)
    {
        Item item = itemDatabase.GetItemByName(itemName);
        if(item == null) return;
        
        equippedItems.Add(item);
        ApplyItemEffect(item);

        Debug.Log($"아이템 획득 {item.name}");
    }

    public void UnEquipItem(string itemName)
    {
        Item item = itemDatabase.GetItemByName(itemName);
        if (item == null) return;

        if (equippedItems.Contains(item))
        {
            equippedItems.Remove(item);
            RemoveItemEffect(item);
            Debug.Log($"아이템 제거 {item.name}");
        }
        else
        {
            Debug.LogWarning($"제거할 아이템이 없음 {itemName}");
        }
    }

    private void ApplyItemEffect(Item item)
    {
        if (playerStats == null)
        {
            Debug.LogError("playerstat컴포넌트 할당 안됨");
            return;
        }

        switch (item.effectType)
        {
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
                
            case "lifeSteal":
                playerStats.SetLifeSteal(item.value);
                break;
                
            case "revive":
                playerStats.EnableRevive();
                break;
                
            case "defenceBuff":
                // Unique 효과는 PlayerStats에서 특수하게 처리
                playerStats.EnableDefenceBuff(item.value);
                break;
                
            case "moveSpeedBuff":
                playerStats.EnableMoveSpeedBuff(item.value);
                break;
                
            case "critBoost":
                ApplyStatEffect(item, playerStats.ModifyCritChance);
                playerStats.SetCritDamageMultiplier(2f); // 피해량 2배 설정
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
                // 복합 효과(쉼표로 구분된 여러 효과) 처리
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
    
    private void RemoveItemEffect(Item item)
    {
        // 아이템 효과 제거 로직
        // 각 효과 타입별로 처리
    }
    
    private void ApplyStatEffect(Item item, Action<float, string> modifyFunction)
    {
        modifyFunction(item.value, item.valueType);
    }
}
