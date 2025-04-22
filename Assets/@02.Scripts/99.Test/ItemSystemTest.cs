using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSystemTest : MonoBehaviour
{
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private ItemEffectSystem itemEffectSystem;
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private TMP_Dropdown itemDropdown;
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private Button printStatsButton;
    [SerializeField] private Button printItemsButton;
    [SerializeField] private TextMeshProUGUI statsText;

    private void Start()
    {
        if (itemDatabase == null || itemEffectSystem == null || playerStats == null) return;
        InitializeUI();
    }

    private void InitializeUI()
    {
        itemDropdown.ClearOptions();

        List<string> itemNames = itemDatabase.ItemNames;
        if (itemNames != null && itemNames.Count > 0)
        {
            itemDropdown.AddOptions(itemNames);
        }
        else
        {
            itemDropdown.AddOptions(new List<string>{"아이템 없음"});
            acquireButton.interactable = false;
            removeButton.interactable = false;
        }
        
        acquireButton.onClick.AddListener(AcquireSelectedItem);
        removeButton.onClick.AddListener(RemoveSelectedItem);
        printStatsButton.onClick.AddListener(PrintPlayerStats);
        printItemsButton.onClick.AddListener(PrintAcquiredItems);

    }

    private void AcquireSelectedItem()
    {
        string selectedItem = itemDropdown.options[itemDropdown.value].text;
        itemEffectSystem.AcquireItem(selectedItem);
        PrintPlayerStats();
    }

    private void RemoveSelectedItem()
    {
        string selectedItem = itemDropdown.options[itemDropdown.value].text;
        itemEffectSystem.RemoveItem(selectedItem);
        PrintPlayerStats();
    }

    private void PrintPlayerStats()
    {
        string stats = $"플레이어 스탯\n" +
                       $"최대 체력: {playerStats.GetMaxHP():F1}\n" +
                       $"현재 체력: {playerStats.GetCurrentHP():F1}\n" +
                       $"이동 속도: {playerStats.GetMoveSpeed():F1}\n" +
                       $"공격력: {playerStats.GetAttackPower():F1}\n" +
                       $"방어력: {playerStats.GetDefence():F1}\n" +
                       $"치명타 확률: {playerStats.GetCritChance() * 100:F1}%\n" +
                       $"데미지 감소: {playerStats.GetDamageReduction() * 100:F1}%";

        statsText.text = stats;
        Debug.Log(stats);
    }

    private void PrintAcquiredItems()
    {
        itemEffectSystem.DebugPrintAcquiredItems();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AcquireSelectedItem();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveSelectedItem();
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintPlayerStats();
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            PrintAcquiredItems();
        }
    }
}
