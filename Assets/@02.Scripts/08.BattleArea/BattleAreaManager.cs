using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 필드와 던전을 관리하고 생성하는 매니저
/// </summary>
public class BattleAreaManager : MonoBehaviour
{
    
    //매니저
    public int battleAreaClearLimit = 5;
    
    private int mLevelDesign = 1;
    private GameObject mPlayer;
    private GameObject mCurrentArea;
    private int mBattleAreaClearCount = 0;
    
    //필드
    public GameObject[] battleFields;
    
    //던전
    public int dungeonMinSize;
    public int dungeonDivideWidth;
    public int dungeonMinRoomCount;
    
    private GameObject mBattleDungeons;
    private Vector2Int mDungeonMinSize;

    private BattleArea TestArea;
    
    //매개변수를 어떻게 받을지는 게임매니저에 따라 달라질 수 있음
    public void BattleAreaManagerInit(GameObject player, int levelDesign)
    {
        mPlayer = player;
        mLevelDesign = levelDesign;

        //mBattleDungeons = new GameObject("BattleDungeonManager");
        //mBattleDungeons.AddComponent<DungeonController>();
        Debug.Log("Init succeed!");
    }
    
    /// <summary>
    /// 필드 혹은 던전을 생성하는 함수
    /// 처음 게임 씬이 로드될 때 한번만 실행함.
    /// </summary>
    public void BattleAreaCreate()
    {

        Debug.Log("Creating...");
        if (mBattleAreaClearCount % 2 == 0)
        {
            Debug.Log("...Field");
            mCurrentArea = Instantiate(battleFields[Random.Range(0, battleFields.Length)]);
            FieldController fc = mCurrentArea.GetComponent<FieldController>();
            fc.BattleAreaInit(mPlayer, mLevelDesign);

            fc.OnClearBattleArea -= BattleAreaClear;
            fc.OnClearBattleArea += BattleAreaClear;

            TestArea = fc;
            Debug.Log(mBattleAreaClearCount);

        }
        else
        {
            Debug.Log("...Dungeon");
            // DungeonController dm = mBattleDungeons.GetComponent<DungeonController>();
            // dm.BattleDungeonInit(mPlayer, mLevelDesign, mDungeonMinSize, dungeonMaxSize);
            //
            // dm.OnClearBattleArea -= BattleAreaClear;
            // dm.OnClearBattleArea += BattleAreaClear;
            Debug.Log("Dungeeeeeeeeeeeeeeeeeeeon"+ ++mBattleAreaClearCount);
        }
        Debug.Log("Create succeed!");
    }

    /// <summary>
    /// 필드 혹은 던전 클래스에서 클리어함수가 실행된 후 호출될 함수
    /// </summary>
    public void BattleAreaClear()
    {
        mBattleAreaClearCount++;
        Debug.Log("Clear succeed! : " + mBattleAreaClearCount);
        mLevelDesign++; // 필드 혹은 던전 마다 크기가 달라 몬스터,보물상자 스폰수를 다르게 적용해야하니 직접 컨트롤할 필요 없음
        
        if (mBattleAreaClearCount >= battleAreaClearLimit)
        {
            LetsGoHome();
        }
        
        TestArea = null;
    }
    
    /// <summary>
    /// 집으로가는 함수
    /// </summary>
    public void LetsGoHome()
    {
        //마을 씬로드
        Debug.Log("LetsGoHome");
    }

    public void TestBattleAreaClear()
    {
        TestArea?.BattleAreaClear();
    }

}
 