using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestSystemTest : MonoBehaviour
{
    [SerializeField] private QuestDatabase questDatabase;
    
    [SerializeField] private TMP_Dropdown questDropdown;
    [SerializeField] private Button activateButton;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button updateProgressBar;
    [SerializeField] private Button printActiveQuestsButton;
    [SerializeField] private Button printAllQuestsButton;
    [SerializeField] private TMP_InputField progressAmountInput;
    [SerializeField] private TextMeshProUGUI questDetialsTexts;

    private void Start()
    {
        if (questDatabase == null) return;
        InitializeUI();

        questDatabase.OnQuestActivated += OnQuestActivated;
        questDatabase.OnQuestUpdated += OnQuestUpdated;
        questDatabase.OnQuestCompleted += OnQuestCompleted;
    }

    private void OnDestroy()
    {
        if (questDatabase != null)
        {
            questDatabase.OnQuestActivated -= OnQuestActivated;
            questDatabase.OnQuestUpdated -= OnQuestUpdated;
            questDatabase.OnQuestCompleted -= OnQuestCompleted; 
        }
    }

    private void InitializeUI()
    {
        questDropdown.ClearOptions();

        List<string> questIds = questDatabase.QuestIds;
        if (questIds != null && questIds.Count > 0)
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            foreach (var questId in questIds)
            {
                Quest quest = questDatabase.GetQuestById(questId);
                options.Add(new TMP_Dropdown.OptionData($"{quest.Id}: {quest.Title}"));
            }
            questDropdown.AddOptions(options);
        }
        else
        {
            questDropdown.AddOptions(new List<string> { "퀘스트 없음" });
            activateButton.interactable = false;
        }
        
        activateButton.onClick.AddListener(ActivateSelectedQuest);
        completeButton.onClick.AddListener(CompleteSelectedQuest);
        updateProgressBar.onClick.AddListener(UpdateSelectedQuestProgress);
        printActiveQuestsButton.onClick.AddListener(PrintActiveQuests);
        printAllQuestsButton.onClick.AddListener(PrintAllQuests);
        
        questDropdown.onValueChanged.AddListener(OnQuestSelected);

        if (questIds.Count > 0)
        {
            OnQuestSelected(0);
        }
    }

    private void OnQuestSelected(int index)
    {
        if (questDatabase.QuestIds.Count <= index) return;

        string questId = GetSelectedQuestId();
        Quest quest = questDatabase.GetQuestById(questId);

        if (quest != null)
        {
            questDetialsTexts.text = $"ID: {quest.Id}\n" +
                                     $"제목: {quest.Title}\n" +
                                     $"목표: {quest.Objective}\n" +
                                     $"대상: {quest.RequestInformation}\n" +
                                     $"목표량: {quest.TargetAmount}\n" +
                                     $"보상: {quest.RewardSoul} 소울\n" +
                                     $"설명: {quest.Description}";
        }
    }

    private string GetSelectedQuestId()
    {
        if (questDropdown.options.Count == 0 || questDatabase.QuestIds.Count == 0) return "";
        string optionText = questDropdown.options[questDropdown.value].text;
        return optionText.Split(':')[0].Trim();
    }

    private void ActivateSelectedQuest()
    {
        string questId = GetSelectedQuestId();
        if (string.IsNullOrEmpty(questId)) return;

        bool success = questDatabase.ActivateQuest(questId);
        Debug.Log(success? $"퀘스트 활성화 성공 {questId}" : $"퀘스트 활성화 실패 {questId}");
    }

    private void CompleteSelectedQuest()
    {
        string questId = GetSelectedQuestId();
        if (string.IsNullOrEmpty(questId)) return;

        bool success = questDatabase.CompleteQuest(questId);
        Debug.Log(success ? $"퀘스트 완료 처리 성공 {questId}" : $"퀘스트 완료 처리 실패 {questId}");
    }

    private void UpdateSelectedQuestProgress()
    {
        string questId = GetSelectedQuestId();
        if (string.IsNullOrEmpty(questId)) return;

        Quest quest = questDatabase.GetQuestById(questId);
        if (quest == null) return;

        int progressAmount = 1;
        if (!string.IsNullOrEmpty(progressAmountInput.text))
        {
            if (int.TryParse(progressAmountInput.text, out int amount))
            {
                progressAmount = amount;
            }
        }
        
        questDatabase.UpdateQuestProgress(quest.Objective, progressAmount, quest.RequestInformation);
        Debug.Log($"퀘스트 진행 상황 업데이트 {quest.Objective}, 대상 {quest.RequestInformation}, 양 {progressAmount}");
    }

    private void PrintActiveQuests()
    {
        List<Quest> activeQuests = questDatabase.GetActiveQuests();
        Debug.Log($"활성화 된 퀘스트 {activeQuests.Count}개");

        foreach (var quest in activeQuests)
        {
            Debug.Log($"- {quest.Title}: {quest.CurrentAmount}/{quest.TargetAmount} " +
                      $"({quest.Progress * 100:F0}%) {(quest.isCompleted ? "[완료]" : "")}");
        }
    }

    private void PrintAllQuests()
    {
        questDatabase.DebugPrintAllQuest();
    }

    private void OnQuestActivated(Quest quest)
    {
        Debug.Log($"이벤트: 퀘스트 활성화 - {quest.Title}");
    }
    
    private void OnQuestUpdated(Quest quest)
    {
        Debug.Log($"이벤트: 퀘스트 업데이트 - {quest.Title}, 진행도: {quest.CurrentAmount}/{quest.TargetAmount}");
    }
    
    private void OnQuestCompleted(Quest quest)
    {
        Debug.Log($"이벤트: 퀘스트 완료 - {quest.Title}, 보상: {quest.RewardSoul} 소울");
    }

    
}
