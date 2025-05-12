using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Events.Gameplay;
using SceneEnums;
using UnityEngine.SceneManagement;


/// <summary>
/// 필드와 던전을 관리하고 생성하는 매니저
/// </summary>
public class AbyssManager : Singleton<AbyssManager>
{

    [Header("매니저")] 
    public int levelDesign = 1;
    private int battleAreaClearLimit = 5;
    private int battleAreaClearCount = 0;
    public GameObject portal;
    
    public GameObject player;

    [Space(10)] [Header("던전")]
    public int cellSize = 3;
    public int minDungeonRoomSize = 10;
    public int eventRoomChance = 10;
    
    [Space(10)]
    [Range(0, 4)] public int divideLineWidth = 2;
    [Range(2, 20)]public int minRoomCount = 6;
    
    [Space(10)] [Header("드래그 할당")] 
    [Header("포탈")] [SerializeField]public GameObject portalPrefab;
    
    [Header("필드")] public List<FieldDataSO> battleFields;
    [Header("던전")] public SODungeonList dungeonListSO;
    
    /// <summary>
    /// 매니저 초기화 및 필드,던전을 생성하는 함수
    /// </summary>
    /// <param name="player"></param>
    /// <param name="levelDesign"></param>
    /// <param name="battleAreaClearLimit"></param>
    public void BattleAreaManagerInit(GameObject player, int levelDesign, int battleAreaClearLimit)
    {
        Instance.player = player;
        Instance.levelDesign = levelDesign;
        Instance.battleAreaClearLimit = battleAreaClearLimit;
        
        portal = Instantiate(portalPrefab);

        BattleAreaCreate();
        Debug.Log("Init succeed!");
    }

    /// <summary>
    /// 필드 혹은 던전을 생성하는 함수
    /// </summary>
    private static void BattleAreaCreate()
    {
        if (Instance.battleAreaClearCount % 2 == Instance.battleAreaClearLimit % 2 - 1)
        {
            SceneLoader.LoadSceneAsync(Constants.AbyssFieldScene).Forget();
        }
        else
        {
            SceneLoader.LoadSceneAsync(Constants.AbyssDungeonScene).Forget();
        }

        Debug.Log("Create succeed!");
    }

    /// <summary>
    /// 필드 혹은 던전 클래스에서 클리어함수가 실행된 후 호출될 함수
    /// </summary>
    public void BattleAreaClear()
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
 