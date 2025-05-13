using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// 필요한 만큼 추가
public sealed class ManagersHub : Singleton<ManagersHub>, IInitializable
{
    GameManager mGameManager;
    UIManager mUIManager;
    AudioManager mAudioManager;
    

    void Start()
    {
        mGameManager = GameManager.Instance;
        mUIManager = UIManager.Instance;
        mAudioManager = AudioManager.Instance;
    }

    /* IInitializable – BootLoader 가 호출 */
    public async UniTask InitializeAsync()
    {
        var tasks = new UniTask[]
        {
            // mGameManager.InitializeAsync(),
            // mUIManager.InitializeAsync(),
        };
        await UniTask.WhenAll(tasks);
    }

    public async UniTask InitializeUserDataAsync()
    {
        mAudioManager.InitAudioDataFromUserData();

        var tasks = new UniTask[]
        {

        };
        await UniTask.WhenAll(tasks);
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}
