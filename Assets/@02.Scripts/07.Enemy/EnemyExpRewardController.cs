using System;
using System.Collections;
using System.Collections.Generic;
using EnemyEnums;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 적 타입에 따른 경험치 보상을 관리하는 시스템
/// </summary>
public class EnemyExpRewardController : MonoBehaviour
{
    [System.Serializable]
    public class ExpRewardRange
    {
        public EnemyType enemyType;
        public int minExp;
        public int maxExp;
    }

    [Header("적 타입 별 경험치 보상 범위")] 
    [SerializeField] private ExpRewardRange[] mExpRewards;

    private void Awake()
    {
        if (mExpRewards == null || mExpRewards.Length == 0)
            SetDefaultExpRewards();
    }

    /// <summary>
    /// 기본 경험치 보상 범위 설정
    /// </summary>
    private void SetDefaultExpRewards()
    {
        mExpRewards = new ExpRewardRange[3];

        //일반 몬스터
        mExpRewards[0] = new ExpRewardRange
        {
            enemyType = EnemyType.Common,
            minExp = 10,
            maxExp = 20
        };
        
        //엘리트 몬스터
        mExpRewards[1] = new ExpRewardRange
        {
            enemyType = EnemyType.Elite,
            minExp = 30,
            maxExp = 50
        };
        
        //보스
        mExpRewards[2] = new ExpRewardRange
        {
            enemyType = EnemyType.Boss,
            minExp = 100,
            maxExp = 200
        };
    }

    /// <summary>
    /// 적 타입에 따른 경험치 보상 계산
    /// </summary>
    /// <param name="enemyType">적 타입</param>
    /// <returns>획득할 경험치</returns>
    public int GetExpReward(EnemyType enemyType)
    {
        ExpRewardRange rewardRange = null;

        foreach (var range in mExpRewards)
        {
            if (range.enemyType == enemyType)
            {
                rewardRange = range;
                break;
            }
        }

        if (rewardRange == null)
        {
            Debug.LogWarning($"해당 적타입{enemyType}의 보상 정보가 없습니다");
            return 0;
        }

        int exp = Random.Range(rewardRange.minExp, rewardRange.maxExp + 1);

        return exp;
    }

    /// <summary>
    /// 특정 적 타입의 경험치 보상 범위 설정
    /// </summary>
    /// <param name="enemyType">적 타입</param>
    /// <param name="minExp">최소 경험치</param>
    /// <param name="maxExp">최대 경험치</param>
    public void SetExpRewardRange(EnemyType enemyType, int minExp, int maxExp)
    {
        //기존 범위를 찾아서 수정
        bool found = false;
        for (int i = 0; i < mExpRewards.Length; i++)
        {
            if (mExpRewards[i].enemyType == enemyType)
            {
                mExpRewards[i].minExp = Mathf.Max(0, minExp);
                mExpRewards[i].maxExp = Mathf.Max(minExp, maxExp);
                found = true;
                break;
            }
        }

        //없으면 새로 추가
        if (!found)
        {
            var newArray = new ExpRewardRange[mExpRewards.Length + 1];
            System.Array.Copy(mExpRewards, newArray, mExpRewards.Length);

            newArray[mExpRewards.Length] = new ExpRewardRange
            {
                enemyType = enemyType,
                minExp = Mathf.Max(0, minExp),
                maxExp = Mathf.Max(minExp, maxExp)
            };

            mExpRewards = newArray;
        }
    }
}
