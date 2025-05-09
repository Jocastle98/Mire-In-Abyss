using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static float Progress { get; private set; }
    public static Scene CurrentGameplayScene { get; internal set; }

    public static async UniTask LoadAsync(string targetSceneName)
    {
        Progress = 0f;

        /* 0) 로딩 오버레이 Additive */
        var overlayOp = SceneManager.LoadSceneAsync(Constants.LoadingOverlayScene,
                                                    LoadSceneMode.Additive);
        await overlayOp.ToUniTask();
        Scene overlayScene = SceneManager.GetSceneByName(Constants.LoadingOverlayScene);

        /* 1) 이전 Gameplay 씬 언로드 (메모리 세이브) */
        if (CurrentGameplayScene.IsValid() && CurrentGameplayScene.isLoaded)
            await SceneManager.UnloadSceneAsync(CurrentGameplayScene).ToUniTask();

        /* 2) 새 Gameplay 씬 Additive */
        var loadOp = SceneManager.LoadSceneAsync(targetSceneName,
                                                 LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            Progress = loadOp.progress;   // 0 → 0.9
            await UniTask.Yield();
        }

        Scene newScene = SceneManager.GetSceneByName(targetSceneName);
        SceneManager.SetActiveScene(newScene);
        CurrentGameplayScene = newScene;
        Progress = 1f;

        /* 3) 오버레이 언로드 */
        if (overlayScene.IsValid() && overlayScene.isLoaded)
            await SceneManager.UnloadSceneAsync(overlayScene).ToUniTask();
    }
}

