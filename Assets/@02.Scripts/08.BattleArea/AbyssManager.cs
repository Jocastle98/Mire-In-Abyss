using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks;


/// <summary>
/// 필드와 던전을 관리하고 생성하는 매니저
/// </summary>
public class AbyssManager : Singleton<AbyssManager>
{

    [Header("매니저")] 
    public static int levelDesign = 1;
    private static int battleAreaClearLimit = 5;
    private static int battleAreaClearCount = 0;
    public static GameObject portal;
    
    public static GameObject player;

    [Space(10)] [Header("던전")]
    [SerializeField]private int mCellSize = 3;
    [SerializeField]private int mMinDungeonRoomSize = 10;
    [SerializeField]private int mEventRoomChance = 10;
    
    [Space(10)]
    [SerializeField][Range(0, 4)] private int mDivideLineWidth = 2;
    [SerializeField][Range(2, 20)]private int mMinRoomCount = 6;
    
    [Space(10)] [Header("드래그 할당")] 
    [Header("포탈")] [SerializeField]private GameObject mPortalPrefab;
    
    [Header("필드")] public static List<FieldDataSO> battleFields;
    [Header("던전")] public static SODungeonList dungeonListSO;
    
    /// <summary>
    /// 매니저 초기화 및 필드,던전을 생성하는 함수
    /// </summary>
    /// <param name="player"></param>
    /// <param name="levelDesign"></param>
    /// <param name="battleAreaClearLimit"></param>
    public void BattleAreaManagerInit(GameObject player, int levelDesign, int battleAreaClearLimit)
    {
        AbyssManager.player = player;
        AbyssManager.levelDesign = levelDesign;
        AbyssManager.battleAreaClearLimit = battleAreaClearLimit;
        
        portal = Instantiate(mPortalPrefab);

        BattleAreaCreate();
        Debug.Log("Init succeed!");
    }

    /// <summary>
    /// 필드 혹은 던전을 생성하는 함수
    /// </summary>
    private static async void BattleAreaCreate()
    {
        if (battleAreaClearCount % 2 == battleAreaClearLimit % 2 - 1)
        {
            SceneLoader.LoadAsync(Constants.AbyssFieldScene).Forget();
        }
        else
        {
            //생성 및 초기화
            GameObject dungeon = new GameObject("Dungeon");
            DungeonController dCon = dungeon.AddComponent<DungeonController>();
            dCon.SetPortal(portal);
            dCon.DungeonInit(mCellSize, mMinDungeonRoomSize, mDivideLineWidth, mMinRoomCount, mLevelDesign,
                     mEventRoomChance, dungeonListSO, mPlayer);
            
            //클리어 델리게이트 부착
            dCon.OnClearBattleArea -= BattleAreaClear;
            dCon.OnClearBattleArea += BattleAreaClear;
            
            SceneLoader.LoadAsync(Constants.AbyssDungeonScene).Forget();
        }

        Debug.Log("Create succeed!");
    }

    /// <summary>
    /// 필드 혹은 던전 클래스에서 클리어함수가 실행된 후 호출될 함수
    /// </summary>
    public static void BattleAreaClear()
    {
        levelDesign++;
        battleAreaClearCount++;
        Debug.Log("Clear succeed! : " + battleAreaClearCount);

        if (battleAreaClearCount >= battleAreaClearLimit)
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
    private static void LetsGoHome()
    {
        //마을 씬로드
        Debug.Log("LetsGoHome");
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}
 