using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using R3;

using Events.Data;
using GameEnums;
using TMPro;

public sealed class BootLoader : MonoBehaviour
{
    [SerializeField] TMP_Text mProgressText;
    public static float Progress { get; private set; }

    async void Start()
    {
        /* A) Systems 씬 Additive */
        await SceneManager.LoadSceneAsync(Constants.SystemsScene, LoadSceneMode.Additive).ToUniTask();

        /* B) 전역 초기화 대기 */
        var initTasks = new UniTask[]
        {
            GameDB.Instance.InitializeAsync(),
            ManagersHub.Instance.InitializeAsync(),
            UserData.Instance.InitializeAsync()
        };
        await WaitWithProgress(initTasks, "전역 초기화 중...(1/2)");   // Progress 값 0~1
        R3EventBus.Instance.Publish(new Preloaded());

        var userDataInitTasks = new UniTask[]
        {
            ManagersHub.Instance.InitializeUserDataAsync()
        };
        await WaitWithProgress(userDataInitTasks, "유저 데이터 불러오는 중...(2/2)");

        /* C) MainMenu 씬 Additive */
        var menuOp = SceneManager.LoadSceneAsync(Constants.MainMenuScene,
                                                 LoadSceneMode.Additive);
        await menuOp.ToUniTask();

        /*   ── Scene 객체 얻기 ──  */
        Scene menuScene = SceneManager.GetSceneByName(Constants.MainMenuScene);
        if (!menuScene.IsValid() || !menuScene.isLoaded)
        {
            Debug.LogError("MainMenu 씬을 찾지 못했습니다!");
            return;
        }

        SceneManager.SetActiveScene(menuScene);
        SceneLoader.CurrentGameplayScene = menuScene;
        SceneLoader.Init();
        GameManager.Instance.SetGameState(GameState.MainMenu);

        /* D) Boot 씬 언로드 */
        Scene bootScene = gameObject.scene;
        await SceneManager.UnloadSceneAsync(bootScene).ToUniTask();
    }

    /* 진행률 헬퍼 */
    static async UniTask WaitWithProgress(UniTask[] tasks, string progressText)
    {
        int total = tasks.Length, done = 0;
        var wrapped = tasks.Select(async t =>
        {
            await t; Progress = ++done / (float)total;
        });
        await UniTask.WhenAll(wrapped);
    }
}
