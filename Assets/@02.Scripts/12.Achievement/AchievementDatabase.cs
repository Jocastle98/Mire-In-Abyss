using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events.HUD;
using Events.Player.Modules;
using UnityEngine;

public class AchievementDatabase : MonoBehaviour
{
    public TextAsset achievementCsvFile;
    private string mAchievementCsvFilePath = "Achievement/achievement_list";

    private Dictionary<string, Achievement> mAchievementDatabase = new Dictionary<string, Achievement>();   //전체 업적 데이터
    private Dictionary<string, Achievement> mUnlockAchievements = new Dictionary<string, Achievement>();    //해금된 업적

    public int AchievementCount => mAchievementDatabase.Count;
    public int UnlockCount => mUnlockAchievements.Count;
    public List<string> AchievementIds => mAchievementDatabase.Keys.ToList();
    public List<Achievement> AllAchievements => mAchievementDatabase.Values.ToList();

    public event Action<Achievement> OnAchievementUnlocked;
    public event Action<Achievement> OnAchievementProgress;

    private void Awake()
    {
        if (achievementCsvFile != null)
        {
            LoadAchievementsFromCSV();
        }
        else
        {
            LoadAchievementsFromResources();
        }

        LoadUnlockedAchievements();

    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        //이벤트 구독
    }

    #region CSV 파싱 관련

    private void LoadAchievementsFromResources()
    {
        TextAsset csvAsset = Resources.Load<TextAsset>(mAchievementCsvFilePath);

        if (csvAsset == null) return;
        ParseCsvContent(csvAsset);
    }

    private void LoadAchievementsFromCSV()
    {
        ParseCsvContent(achievementCsvFile);
    }

    private void ParseCsvContent(TextAsset csvAsset)
    {
        string[] lines = csvAsset.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) 
                continue;

            Achievement achievement = ParseAchievementFromCSV(line);
            if (achievement != null)
            {
                mAchievementDatabase[achievement.Id] = achievement;
            }
        }
    }

    private Achievement ParseAchievementFromCSV(string line)
    {
        try
        {
            List<string> values = CSVParser(line);

            if (values.Count < 6)
            {
                Debug.LogWarning($"CSV라인 형식이 올바르지 않음 {line}");
                return null;
            }

            Achievement achievement = new Achievement
            {
                Id = values[0],
                Title = values[1],
                Info = values[2],
                Description = values[3],
                IllustrationComment = values[4],
                TargetAmount = int.Parse(values[5]),
                isUnlocked = false,
                Progress = 0f
            };

            return achievement;
        }
        catch (Exception e)
        {
            Debug.LogError($"업적 파싱 중 오류 발생 {e.Message}, 라인 {line}");
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

    #region 업적 관리 관련

    public Achievement GetAchievementById(string achievementId)
    {
        if (mAchievementDatabase.TryGetValue(achievementId, out Achievement achievement))
        {
            return achievement;
        }

        Debug.LogWarning($"업적을 찾을 수 없음 {achievementId}");
        return null;
    }

    public List<Achievement> GetUnlockedAchievements()
    {
        return mUnlockAchievements.Values.ToList();
    }

    public void UpdateAchievementProgress(string achievementId, float progress)
    {
        if (!mAchievementDatabase.TryGetValue(achievementId, out Achievement achievement))
        {
            Debug.LogWarning($"업적을 찾을 수 없음 : {achievementId}");
            return;
        }

        if (achievement.isUnlocked)
            return;

        achievement.Progress = Mathf.Clamp01(progress);
        
        OnAchievementProgress?.Invoke(achievement);

        if (achievement.Progress >= 1.0f)
        {
            UnlockAchievement(achievementId);
        } 
    }

    public void UnlockAchievement(string achievementId)
    {
        if (!mAchievementDatabase.TryGetValue(achievementId, out Achievement achievement))
        {
            Debug.LogWarning($"업적을 찾을 수 없음: {achievementId}");
            return;
        }

        if (achievement.isUnlocked)
            return;
        achievement.isUnlocked = true;
        achievement.Progress = 1.0f;

        mUnlockAchievements[achievementId] = achievement;
        
        OnAchievementUnlocked?.Invoke(achievement);
        
        R3EventBus.Instance.Publish(new ToastPopup(
            $"업적 : {achievement.Title}\n{achievement.Description}", 3f, Color.yellow));

        SaveUnlockedAchievements();
    }

    private void SaveUnlockedAchievements()
    {
        //임시 
        string unlockedIds = string.Join(",", mUnlockAchievements.Keys);
        PlayerPrefs.SetString("UnlockedAchievements", unlockedIds);
        PlayerPrefs.Save();
    }

    private void LoadUnlockedAchievements()
    {
        string unlockedIds = PlayerPrefs.GetString("UnlockedAchievements", "");

        if (string.IsNullOrEmpty(unlockedIds))
            return;
        string[] ids = unlockedIds.Split(',');

        foreach (var id in ids)
        {
            if (mAchievementDatabase.TryGetValue(id, out Achievement achievement))
            {
                achievement.isUnlocked = true;
                achievement.Progress = 1.0f;
                mUnlockAchievements[id] = achievement;
            }
        }
    }

    #endregion

    #region 이벤트 핸들러

    private void OnEnemyKilled(){}
    private void OnQuestCompleted(QuestCompleted e){}
    private void CheckSingleEventAchievement(string achievementId){}
    private void IncrementCountAchievement(string achievementId, int targetCount){}
    
    #endregion

    #region 디버깅 관련

    public void DebugPrintAllAchievements()
    {
        Debug.Log($"===== 업적 데이터베이스 총 {mAchievementDatabase.Count}개 업적 =====");
        foreach (var achievement in mAchievementDatabase.Values)
        {
            Debug.Log($"ID: {achievement.Id}, 제목: {achievement.Title}");
            Debug.Log($"해금 전 안내: {achievement.Info}");
            Debug.Log($"달성 시 팝업: {achievement.Description}");
            Debug.Log($"도감 설명: {achievement.IllustrationComment}");
            Debug.Log($"상태: {(achievement.isUnlocked ? "해금됨" : "미해금")} ({achievement.Progress * 100}%)");
            Debug.Log("-------------------------------------");
        }
    }

    public void DebugUnlockAllAchievements()
    {
        foreach (string id in mAchievementDatabase.Keys)
        {
            UnlockAchievement(id);
        }
        Debug.Log("모든 업적이 해금되었습니다.");
    }

    
    public void DebugResetAllAchievements()
    {
        foreach (var achievement in mAchievementDatabase.Values)
        {
            achievement.isUnlocked = false;
            achievement.Progress = 0f;
        }
        
        mUnlockAchievements.Clear();
        PlayerPrefs.DeleteKey("UnlockedAchievements");
        
        // 모든 카운터 초기화
        foreach (string id in mAchievementDatabase.Keys)
        {
            PlayerPrefs.DeleteKey($"Achievement_{id}_Count");
        }
        
        PlayerPrefs.Save();
        Debug.Log("모든 업적이 초기화되었습니다.");
    }
    #endregion
}
