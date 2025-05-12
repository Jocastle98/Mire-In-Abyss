using System;
using System.Collections;
using System.Collections.Generic;
using Events.Player;
using UnityEngine;

/// <summary>
/// 플레이어의 레벨과 경험치를 관리하는 컨트롤러
/// </summary>
public class PlayerLevelController : MonoBehaviour
{
    [System.Serializable]
    public class LevelData
    {
        public int level;
        public int expRequired;
        public float hpBonus;
        public float attackBonus;
        public float defenceBonus;
    }

    [Header("레벨 설정")] 
    [SerializeField] private LevelData[] mLevelTable;
    [SerializeField] private int mMaxLevel = 30;
    [SerializeField] private bool mApplyBonusOnLevelUp = true;

    [Header("경험치 설정")] 
    [SerializeField] private int mCurrentExp = 0;
    [SerializeField] private float mExpMultiplier = 1.0f;

    [Header("참조")] 
    [SerializeField] private PlayerStats mPlayerStats;

    private int mCurrentLevel = 1;

    public int CurrentLevel => mCurrentLevel;
    public int CurrentExp => mCurrentExp;
    public int MaxExp => GetRequiredExpForLevel(mCurrentLevel);

    public event Action<int, int> OnExpGained;  //현재 경험치, 최대 경험치
    public event Action<int> OnLevelUp;         //새 레벨

    private void Awake()
    {
        if (mLevelTable == null || mLevelTable.Length == 0)
        {
            GenerateDefaultLevelTable();
        }
    }

    private void Start()
    {
        //초기 레벨 및 경험치 정보 UI에 표시
        PublishLevelInfo();
        PublishExpInfo();
    }

    /// <summary>
    /// 기본 레벨 테이블을 자동 생성
    /// </summary>
    private void GenerateDefaultLevelTable()
    {
        mLevelTable = new LevelData[mMaxLevel];

        for (int i = 0; i < mMaxLevel; i++)
        {
            int level = i + 1;
            int expRequired = 20 * level; //필요 경험치

            mLevelTable[i] = new LevelData
            {
                //TODO: 수치 조정
                level = level,
                expRequired = expRequired,
                hpBonus = level * 5f,
                attackBonus = level * 2f,
                defenceBonus = level * 1f
            };
        }
    }

    /// <summary>
    /// 특정 레벨에 필요한 경험치 반환
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public int GetRequiredExpForLevel(int level)
    {
        if (level <= 0 || level > mMaxLevel || mLevelTable == null || mLevelTable.Length == 0)
            return 0;
        return mLevelTable[level - 1].expRequired;
    }

    /// <summary>
    /// 경험치 획득
    /// </summary>
    /// <param name="amount"></param>
    public void GainExperience(int amount)
    {
        if (mCurrentLevel >= mMaxLevel) //최대 레벨이면 종료
            return;
        
        //경험치 증폭 적용
        int actualAmount = Mathf.RoundToInt(amount * mExpMultiplier);
        mCurrentExp += actualAmount;
        Debug.Log($"경험치 획득 {actualAmount}");
        //레벨업 체크
        CheckForLevelUp();

        //경험치 정보 이벤트
        PublishExpInfo();
    }

    /// <summary>
    /// 레벨업 조건 확인 및 처리
    /// </summary>
    private void CheckForLevelUp()
    {
        if (mCurrentLevel >= mMaxLevel)
            return;
        int requiredExp = GetRequiredExpForLevel(mCurrentLevel);

        while (mCurrentExp >= requiredExp && mCurrentLevel < mMaxLevel)
        {
            mCurrentExp -= requiredExp;
            mCurrentLevel++;

            //레벨업 보너스 적용
            if (mApplyBonusOnLevelUp)
            {
                ApplyLevelUpBonus(); //TODO: 수치 계산 수정 필요
            }
            
            //이벤트
            OnLevelUp?.Invoke(mCurrentLevel);
            PublishLevelInfo();

            Debug.Log($"레벨업 {mCurrentLevel}");
            //다음 레벨 필요 경험치 갱신
            requiredExp = GetRequiredExpForLevel(mCurrentLevel);
        }
    }

    /// <summary>
    /// 레벨 업 시 스탯 보너스
    /// </summary>
    private void ApplyLevelUpBonus()
    {
        if (mCurrentLevel <= 0 || mCurrentLevel > mLevelTable.Length)
            return;
        LevelData levelData = mLevelTable[mCurrentLevel - 1];

        if (mPlayerStats != null)
        {
            mPlayerStats.ModifyMaxHP(levelData.hpBonus, "add");
            mPlayerStats.ModifyAttackPower(levelData.attackBonus, "add");
            mPlayerStats.ModifyDefence(levelData.defenceBonus, "add");
            
            //레벨업 시 최대 체력으로 회복
            mPlayerStats.Heal(mPlayerStats.GetMaxHP());
            
            R3EventBus.Instance.Publish(new Events.HUD.ToastPopup($"레벨 업!", 3f, Color.cyan));
        }
    }

    /// <summary>
    /// 경험치 증폭 효과 설정
    /// </summary>
    /// <param name="multiplier"></param>
    public void SetExpMultiplier(float multiplier)
    {
        mExpMultiplier = Mathf.Max(0f, multiplier);
    }

    /// <summary>
    /// 레벨 정보
    /// </summary>
    private void PublishLevelInfo()
    {
        R3EventBus.Instance.Publish(new PlayerLevelChanged(mCurrentLevel));
    }

    /// <summary>
    /// 경험치 정보
    /// </summary>
    private void PublishExpInfo()
    {
        R3EventBus.Instance.Publish(new PlayerExpChanged(mCurrentExp, GetRequiredExpForLevel(mCurrentLevel)));
    }

    public void SetLevelAndExp(int level, int exp)
    {
        mCurrentLevel = Mathf.Clamp(level, 1, mMaxLevel);
        mCurrentExp = Mathf.Max(0, exp);
        
        PublishLevelInfo();
        PublishExpInfo();
    }
}
