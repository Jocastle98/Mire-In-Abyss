using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// 필드와 던전을 관리하고 생성하는 매니저
/// </summary>
public class BattleAreaManager : MonoBehaviour
{

    [Header("매니저")] 
    [SerializeField]private int mLevelDesign = 1;
    [SerializeField]private int mBattleAreaClearLimit = 5;
    [SerializeField]private int mBattleAreaClearCount = 0;
    private GameObject mPortal;
    
    [SerializeField]private GameObject mPlayer;
    [SerializeField]private GameObject mCurrentArea;

    [Space(10)] [Header("던전")]
    [SerializeField]private int mCellSize = 3;
    [SerializeField]private int mMinDungeonRoomSize = 10;
    [SerializeField]private int mEventRoomChance = 10;
    
    [Space(10)]
    [SerializeField][Range(0, 4)] private int mDivideLineWidth = 2;
    [SerializeField][Range(2, 20)]private int mMinRoomCount = 6;
    
    [Space(10)] [Header("드래그 할당")] 
    [Header("포탈")] [SerializeField]private GameObject mPortalPrefab;
    [Header("필드")] [SerializeField]private List<FieldDataSO> mBattleFields;
    [Header("던전")] [SerializeField]private SODungeonList mDungeonListSO;

    private void Start()
    {
        BattleAreaManagerInit(mPlayer, 1, 5);
    }

    /// <summary>
    /// 매니저 초기화 및 필드,던전을 생성하는 함수
    /// </summary>
    /// <param name="player"></param>
    /// <param name="levelDesign"></param>
    /// <param name="battleAreaClearLimit"></param>
    public void BattleAreaManagerInit(GameObject player, int levelDesign, int battleAreaClearLimit)
    {
        mPlayer = player;
        mLevelDesign = levelDesign;
        mBattleAreaClearLimit = battleAreaClearLimit;
        mPortal = Instantiate(mPortalPrefab);

        BattleAreaCreate();
        Debug.Log("Init succeed!");
    }

    /// <summary>
    /// 필드 혹은 던전을 생성하는 함수
    /// </summary>
    private void BattleAreaCreate()
    {
        Debug.Log("Creating...");
        if (mBattleAreaClearCount % 2 == 1)
        {
            Debug.Log("...Field");
            //생성 및 초기화
            FieldDataSO fieldData = mBattleFields[Random.Range(0, mBattleFields.Count)];
            mCurrentArea = Instantiate(fieldData.battleFields);
            FieldController fc = mCurrentArea.GetComponent<FieldController>();
            fc.SetPortal(mPortal);
            fc.FieldInit(mPlayer, mLevelDesign, fieldData);
            
            //클리어 델리게이트 부착
            fc.OnClearBattleArea -= BattleAreaClear;
            fc.OnClearBattleArea += BattleAreaClear;
        }
        else
        {
            Debug.Log("...Dungeon");
            //생성 및 초기화
            GameObject dungeon = new GameObject("Dungeon");
            DungeonController dCon = dungeon.AddComponent<DungeonController>();
            dCon.SetPortal(mPortal);
            dCon.DungeonInit(mCellSize, mMinDungeonRoomSize, mDivideLineWidth, mMinRoomCount, mLevelDesign,
                     mEventRoomChance, mDungeonListSO, mPlayer);
            
            //클리어 델리게이트 부착
            dCon.OnClearBattleArea -= BattleAreaClear;
            dCon.OnClearBattleArea += BattleAreaClear;
        }

        Debug.Log("Create succeed!");
    }

    /// <summary>
    /// 필드 혹은 던전 클래스에서 클리어함수가 실행된 후 호출될 함수
    /// </summary>
    private void BattleAreaClear()
    {
        mLevelDesign++;
        mBattleAreaClearCount++;
        Debug.Log("Clear succeed! : " + mBattleAreaClearCount);

        if (mBattleAreaClearCount >= mBattleAreaClearLimit)
        {
            LetsGoHome();
        }
        else
        {
            BattleAreaCreate();
        }

    }

    /// <summary>
    /// 집으로가는 함수
    /// </summary>
    private void LetsGoHome()
    {
        //마을 씬로드
        Debug.Log("LetsGoHome");
    }
}
 