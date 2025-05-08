using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 게임 내 모든 퀘스트 정보를 CSV 파일에서 로드하고 관리하는 클래스
/// </summary>
public class QuestDatabase : MonoBehaviour
{
    public TextAsset questCsvFile;                          //인스펙터에서 할당될 CSV파일
    private string mQuestCsvFilePath = "Quests/quest_list"; //Resources 폴더 내 CSV파일 경로
    
    private Dictionary<string, Quest> mQuestDatabase = new Dictionary<string, Quest>();     //모든 퀘스트 정보를 저장하는 데이터베이스
    private Dictionary<string, Quest> mActiveQuests = new Dictionary<string, Quest>();      //활성화된 퀘스트 목록
    private Dictionary<string, Quest> mCompletedQuests = new Dictionary<string, Quest>();   //완료한 퀘스트 목록

    public int QuestCount => mQuestDatabase.Count;  //데이터 베이스에 등록된 퀘스트 수
    public List<string> QuestIds => mQuestDatabase.Keys.ToList();   //모든 퀘스트 ID 목록

    public event Action<Quest> OnQuestActivated;    //퀘스트 활성화 시 발생하는 이벤트
    public event Action<Quest> OnQuestUpdated;      //퀘스트 진행 상황 업데이트 시 발생하는 이벤트
    public event Action<Quest> OnQuestCompleted;    //퀘스트 완료 시 발생하는 이벤트
    
    private void Awake()
    {
        if (questCsvFile != null) //인스펙터 할당이 되어 있다면
        {
            LoadQuestsFromCSV();
        }
        else //인스펙터 할당이 안돼있다면
        {
            LoadQuestsFromResources();
        }
        
    }

    #region CSV 파싱 관련

    /// <summary>
    /// Resources 폴더에서 퀘스트 CSV파일 로드
    /// </summary>
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
    
    /// <summary>
    /// 인스펙터에 할당된 CSV파일에서 정보 로드
    /// </summary>
    private void LoadQuestsFromCSV()
    {
        ParseCsvContent(questCsvFile);
    }

    /// <summary>
    /// CSV 파일의 내용을 줄별로 파싱하여 퀘스트 데이터 베이스에 추가
    /// </summary>
    /// <param name="csvAsset">파싱할 CSV파일</param>
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

    }

    /// <summary>
    /// CSV한 줄을 파싱하여 Quest객체로 변환
    /// </summary>
    /// <param name="line">파싱된 CSV의 한줄</param>
    /// <returns>생성된 Quest객체</returns>
    private Quest ParseQuestFromCSV(string line)
    {
        try
        {
            List<string> values = CSVParser(line);

            if (values.Count < 8)
            {
                Debug.LogWarning($"CSV 라인 형식이 올바르지 않음 {line}");
                return null;
            }

            Quest quest = new Quest
            {
                Id = values[0],
                Title = values[1],
                RequestInformation = values[2],
                Goal = values[3],
                Objective = values[4],
                TargetAmount = int.Parse(values[5]),
                RewardSoul = int.Parse(values[6]),
                Description = values[7],
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

    
    /// <summary>
    /// CSV라인에서 쉼표로 구분된 값들을 파싱하고 따옴표로 묶인 내용을 올바르게 처리
    /// </summary>
    /// <param name="line">CSV의 한줄</param>
    /// <returns>파싱된 값들의 리스트</returns>
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

    /// <summary>
    /// ID로 퀘스트 정보 조회
    /// </summary>
    /// <param name="questId">찾을 퀘스트 ID</param>
    /// <returns>퀘스트 객체</returns>
    public Quest GetQuestById(string questId)
    {
        if (mQuestDatabase.TryGetValue(questId, out Quest quest))
        {
            return quest;
        }

        Debug.LogWarning($"퀘스트를 찾을 수 없음 {questId}");
        return null;
    }

    /// <summary>
    /// 특정 목표 유형에 해당하는 모든 퀘스트 조회
    /// </summary>
    /// <param name="objective"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 퀘스트 활성화 (수락)
    /// </summary>
    /// <param name="questId">활성화할 퀘스트 ID</param>
    /// <returns>활성화 성공 여부</returns>
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

    /// <summary>
    /// 활성화된 퀘스트의 진행상황 업데이트
    /// </summary>
    /// <param name="objective">퀘스트 목표 유형</param>
    /// <param name="amount">증가량 기본값1</param>
    /// <param name="targetId">대상 ID</param>
    public void UpdateQuestProgress(string objective, int amount = 1, string targetId = "")
    {
        bool questUpdated = false;

        foreach (var quest in mActiveQuests.Values.ToList())
        {
            if (quest.Objective == objective)
            {
                if (!string.IsNullOrEmpty(targetId) &&
                    !quest.RequestInformation.Equals(targetId, StringComparison.OrdinalIgnoreCase))
                    continue;

                int prevAmount = quest.CurrentAmount;
                quest.CurrentAmount += amount;

                if (quest.CurrentAmount < 0)
                    quest.CurrentAmount = 0;
                if (quest.CurrentAmount > quest.TargetAmount)
                    quest.CurrentAmount = quest.TargetAmount;

                if (prevAmount != quest.CurrentAmount)
                {
                    questUpdated = true;
                    Debug.Log($"퀘스트 진행 상황업데이트 {quest.Title} - {quest.CurrentAmount}/{quest.TargetAmount}");
                
                    OnQuestUpdated?.Invoke(quest);

                    if (quest.CurrentAmount >= quest.TargetAmount)
                    {
                        quest.isCompleted = true;
                        CompleteQuest(quest.Id);
                    }
                }
            }
        }

        if (!questUpdated)
        {
            Debug.Log($"업데이트 할 활성 퀘스트 없음 목표 {objective}, 타겟 {targetId}");
        }
        
    }

    /// <summary>
    /// 퀘스트 완료 처리
    /// </summary>
    /// <param name="questId">완료할 퀘스트 ID</param>
    /// <returns>완료처리</returns>
    public bool CompleteQuest(string questId)
    {
        if (!mActiveQuests.TryGetValue(questId, out Quest quest))
        {
            Debug.LogWarning($"완료할 활성 퀘스트를 찾을 수 없음 : {questId}");
            return false;
        }

        quest.isCompleted = true;

        mActiveQuests.Remove(questId);
        mCompletedQuests[questId] = quest;

        Debug.Log($"퀘스트 완료 {quest.Title}, 보상 {quest.RewardSoul}");
        
        OnQuestCompleted?.Invoke(quest);
        
        //TODO: 보상 지급

        return true;
    }

    /// <summary>
    /// 현재 활성화된 모든 퀘스트 조회
    /// </summary>
    /// <returns></returns>
    public List<Quest> GetActiveQuests()
    {
        return mActiveQuests.Values.ToList();
    }

    /// <summary>
    /// 완료된 모든 퀘스트 조회
    /// </summary>
    /// <returns></returns>
    public List<Quest> GetCompletedQuests()
    {
        return mCompletedQuests.Values.ToList();
    }
    #endregion

    #region 디버깅 관련

    public void DebugPrintAllQuest()
    {
        Debug.Log($"===== 퀘스트 데이터베이스 총 {mQuestDatabase.Count}개 퀘스트 =====");
        foreach (var quest in mQuestDatabase.Values)
        {
            Debug.Log($"ID: {quest.Id}, 제목: {quest.Title}");
            Debug.Log($"목표: {quest.Objective}, 대상: {quest.RequestInformation}, 목표량: {quest.TargetAmount}");
            Debug.Log($"보상: {quest.RewardSoul} 소울");
            Debug.Log($"설명: {quest.Description}");
            Debug.Log("-------------------------------------");
        }
    }

    public void DebugPrintActiveQuests()
    {
        Debug.Log($"===== 활성화된 퀘스트 {mActiveQuests.Count}개 =====");
        foreach (var quest in mActiveQuests.Values)
        {
            Debug.Log($"제목: {quest.Title}, 진행도: {quest.CurrentAmount}/{quest.TargetAmount}");
        }
    }

    #endregion
    
}
