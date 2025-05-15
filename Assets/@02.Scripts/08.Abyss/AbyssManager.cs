using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using EnemyEnums;
using Events.Gameplay;
using SceneEnums;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


/// <summary>
/// 필드와 던전을 관리하고 생성하는 매니저
/// </summary>
public class AbyssManager : Singleton<AbyssManager>
{

    [Header("매니저")] 
    public int levelDesign = 1;
    public int abyssClearLimit = 3;
    private int abyssClearCount = 0;
    private int tempSoulStone = 0;
    
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
    
    [Header("필드")] public List<FieldDataSO> abyssFields;
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
        Instance.abyssClearLimit = battleAreaClearLimit;

        BattleAreaCreate();
    }

    /// <summary>
    /// 필드 혹은 던전을 생성하는 함수
    /// </summary>
    private static void BattleAreaCreate()
    {
        int lastAbyss = Instance.abyssClearLimit - 1;
        if (Instance.abyssClearCount % 2 == lastAbyss % 2)
        {
            SceneLoader.LoadSceneAsync(Constants.AbyssFieldScene).Forget();
        }
        else
        {
            SceneLoader.LoadSceneAsync(Constants.AbyssDungeonScene).Forget();
        }
    }

    /// <summary>
    /// 필드 혹은 던전 클래스에서 클리어함수가 실행된 후 호출될 함수
    /// </summary>
    public void BattleAreaClear()
    {
        levelDesign++;
        abyssClearCount++;
        if (tempSoulStone > 0)
        {
            PlayerHub.Instance.Inventory.AddSoul(tempSoulStone);
            R3EventBus.Instance.Publish(new Events.HUD.ToastPopup($"스테이지 클리어 보상: {tempSoulStone} 영혼석 획득", 3f, Color.magenta));
            Debug.Log($"스테이지 클리어 보상: {tempSoulStone} 영혼석 획득!");
            tempSoulStone = 0;
        }
        if (abyssClearCount >= abyssClearLimit)
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
    public static void LetsGoHome()
    {
        //마을 씬로드
        Instance.abyssClearCount = 0;
        Instance.levelDesign = 0;
        
        //AchievementManager.Instance.AddProgress("A011", 1); TODO: 게임 클리어에 업적해금하도록 이동예정
        var playerStats = Instance.player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.ResetStatsFromAbyssToTown();
        }
        
        SceneLoader.LoadSceneAsync(Constants.TownScene).Forget();
    }

    public void AddSoulStoneFromEnemy(EnemyType enemyType)
    {
        int soulAmount = 0;
        switch (enemyType)
        {
            case EnemyType.Common:
                soulAmount = Random.Range(1, 3);
                break;
            case EnemyType.Elite:
                soulAmount = Random.Range(3, 6);
                break;
            case EnemyType.Boss:
                soulAmount = Random.Range(30, 50);
                break;
        }

        tempSoulStone += soulAmount;
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}
 