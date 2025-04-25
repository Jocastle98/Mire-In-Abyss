using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestDatabase : MonoBehaviour
{
    public TextAsset questCsvFile;
    private string mQuestCsvFilePath = "Quests/quest_list";
    
    private Dictionary<string, Quest> mQuestDatabase = new Dictionary<string, Quest>();
    private Dictionary<string, Quest> mActiveQuests = new Dictionary<string, Quest>();
    private Dictionary<string, Quest> mCompletedQuests = new Dictionary<string, Quest>();

    public int QuestCount => mQuestDatabase.Count;
    public List<string> QuestIds => mQuestDatabase.Keys.ToList();

    public event Action<Quest> OnQuestActivated;
    public event Action<Quest> OnQuestUpdated;
    public event Action<Quest> OnQuestCompleted;
    
    private void Awake()
    {
        if (questCsvFile != null)
        {
            LoadQuestsFromCSV();
        }
        else
        {
            LoadQuestsFromResources();
        }
        
    }

    #region CSV 파싱 관련

    private void LoadQuestsFromResources()
    {
        TextAsset csvAsset = Resources.Load<TextAsset>(mQuestCsvFilePath);

        if (csvAsset == null)
        {
            Debug.LogError("Resources 폴더에서 csv 파일을 찾을 수 없음");
            return;
        }

        ParseCsvContent(csvAsset);
    }
    private void LoadQuestsFromCSV()
    {
        ParseCsvContent(questCsvFile);
    }

    private void ParseCsvContent(TextAsset csvAsset)
    {
        string[] lines = csvAsset.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if(string.IsNullOrEmpty(line))
                continue;

            Quest newQuest = ParseQuestFromCSV(line);
            if (newQuest != null)
            {
                mQuestDatabase[newQuest.Id] = newQuest;
            }
        }

        Debug.Log($"{mQuestDatabase.Count}개 퀘스트 로드");
    }

    private Quest ParseQuestFromCSV(string line)
    {
        try
        {
            List<string> values = CSVParser(line);

            if (values.Count < 7)
            {
                Debug.LogWarning($"CSV 라인 형식이 올바르지 않음 {line}");
                return null;
            }

            Quest quest = new Quest
            {
                Id = values[0],
                Title = values[1],
                RequestInformation = values[2],
                Objective = values[3],
                TargetAmount = int.Parse(values[4]),
                RewardSoul = int.Parse(values[5]),
                Description = values[6],
                CurrentAmount = 0,
                isCompleted = false
            };
            return quest;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"{e.Message}, {line}");
            return null;
        }
    }

    private List<string> CSVParser(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentValue = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentValue);
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }

        result.Add(currentValue);
        return result;
    }

    #endregion

    #region 퀘스트 관리 관련

    public Quest GetQuestById(string questId)
    {
        if (mQuestDatabase.TryGetValue(questId, out Quest quest))
        {
            return quest;
        }

        Debug.LogWarning($"퀘스트를 찾을 수 없음 {questId}");
        return null;
    }

    public List<Quest> GetQuestsByObjective(string objective)
    {
        List<Quest> result = new List<Quest>();

        foreach (var quest in mQuestDatabase.Values)
        {
            if (quest.Objective.Equals(objective, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(quest);
            }
        }

        return result;
    }

    public bool ActivateQuest(string questId)
    {
        if (!mQuestDatabase.TryGetValue(questId, out Quest originalQuest))
        {
            Debug.LogWarning($"활성화할 퀘스트를 찾을 수 없음 {questId}");
            return false;
        }

        if (mActiveQuests.ContainsKey(questId))
        {
            Debug.LogWarning($"이미 활성화된 퀘스트 {questId}");
            return false;
        }
        
        if (mCompletedQuests.ContainsKey(questId))
        {
            Debug.LogWarning($"이미 완료된 퀘스트: {questId}");
            return false;
        }

        Quest newQuest = new Quest
        {
            Id = originalQuest.Id,
            Title = originalQuest.Title,
            RequestInformation = originalQuest.RequestInformation,
            Objective = originalQuest.Objective,
            TargetAmount = originalQuest.TargetAmount,
            RewardSoul = originalQuest.RewardSoul,
            Description = originalQuest.Description,
            CurrentAmount = 0,
            isCompleted = false
        };

        mActiveQuests[questId] = new Quest();
        OnQuestActivated?.Invoke(newQuest);
        return true;

    }

    public void UpdateQuestProgress(string objective, int amount = 1, string targetId = "")
    {
        bool questUpdated = false;

        foreach (var quest in mActiveQuests.Values.ToList())
        {
            if (!string.IsNullOrEmpty(targetId) &&
                !quest.RequestInformation.Equals(targetId, StringComparison.OrdinalIgnoreCase))
                continue;

            int prevAmount = quest.CurrentAmount;
            quest.CurrentAmount += amount;
        }
    }
    #endregion
}
