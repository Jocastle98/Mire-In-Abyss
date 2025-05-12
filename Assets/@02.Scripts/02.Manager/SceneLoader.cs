using Cysharp.Threading.Tasks;
using Events.Gameplay;
using SceneEnums;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static Scene CurrentGameplayScene { get; internal set; }
    public static GameScene CurrentSceneType { get; internal set; }

    // MainMenu, Town, Abyss(Field, Dungeon) 이동 시에만 사용
    public static async UniTask LoadSceneAsync(string targetSceneName)
    {
        /* 0) 로딩 오버레이 Additive */
        var overlayOp = SceneManager.LoadSceneAsync(Constants.LoadingOverlayScene,
                                                    LoadSceneMode.Additive);
        await overlayOp.ToUniTask();
        Scene overlayScene = SceneManager.GetSceneByName(Constants.LoadingOverlayScene);

        /* 1) 이전 Gameplay 씬 언로드 (메모리 세이브) */
        if (CurrentGameplayScene.IsValid() && CurrentGameplayScene.isLoaded)
        {
            await SceneManager.UnloadSceneAsync(CurrentGameplayScene).ToUniTask();
        }

        /* 2) 씬 변경에 따른 GameplayShared 씬 처리 및 이벤트 발행 */
        await ChangeSceneAsync(getGameplayMode(targetSceneName));

        /* 3) 새 Gameplay 씬 Additive */
        await SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive).ToUniTask();
        Scene newScene = SceneManager.GetSceneByName(targetSceneName);
        SceneManager.SetActiveScene(newScene);
        CurrentGameplayScene = newScene;

        /* 4) 오버레이 언로드 */
        if (overlayScene.IsValid() && overlayScene.isLoaded)
        {
            await SceneManager.UnloadSceneAsync(overlayScene).ToUniTask();
        }
    }

    private static async UniTask ChangeSceneAsync(GameScene newSceneType)
    { 
        if(CurrentSceneType == newSceneType)
        {
            return;
        }

        // GameplayShared 씬 로드 (메인메뉴 -> 타운)
        if(CurrentSceneType == GameScene.MainMenu && newSceneType == GameScene.Town)
        {
            await SceneManager.LoadSceneAsync(Constants.GameplaySharedScene,
                                                     LoadSceneMode.Additive).ToUniTask();
        }
        
        // GameplayShared 씬 언로드 (타운 or Abyss -> 메인메뉴)
        else if(newSceneType == GameScene.MainMenu)
        {
            Scene sharedScene = SceneManager.GetSceneByName(Constants.GameplaySharedScene);
            if(sharedScene.IsValid() && sharedScene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(sharedScene).ToUniTask();
            }
        }

        // 게임모드 변경 이벤트 발행
        CurrentSceneType = newSceneType;
        R3EventBus.Instance.Publish(new GameplaySceneChanged(newSceneType));
    }

    private static GameScene getGameplayMode(string sceneName)
    {
        GameScene ret = sceneName switch
        {
            Constants.TownScene => GameScene.Town,
            Constants.MainMenuScene => GameScene.MainMenu,
            _ => GameScene.Abyss,
        };
        return ret;
    }
}

