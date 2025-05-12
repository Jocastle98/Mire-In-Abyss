using Cysharp.Threading.Tasks;
using GameEnums;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput))]
public class GameManager : Singleton<GameManager>
{
    [Header("Input")]
    [SerializeField] PlayerInput mPlayerInput;
    public InputManager Input { get { return Instance.mInput; } }
    private InputManager mInput = new InputManager();

    public GameState CurrentGameState { get; private set; }
    public ReadOnlyReactiveProperty<GameState> ObserveState => mStateRP;
    private readonly ReactiveProperty<GameState> mStateRP = new(GameState.Gameplay);

    private bool mIsPause = false;


    protected override void Awake()
    {
        base.Awake();
        Input.Init(mPlayerInput);
        CurrentGameState = mStateRP.Value;
        mStateRP.Subscribe(OnStateChanged);
        // Temp
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        mInput.OnInputUpdate();
    }

    public void Set(GameState next)
    {
        if (CurrentGameState != next) 
        {
            mStateRP.Value = next; 
        }
    }

    public async UniTask PauseForSeconds(float sec)
    {
        Set(GameState.GameplayPause);
        Time.timeScale = 0;
        await UniTask.Delay(System.TimeSpan.FromSeconds(sec));
        Time.timeScale = 1;
        Set(GameState.Gameplay);
    }

    public void SetPause(bool isPaused)
    {
        if (isPaused)
        {
            mIsPause = true;
            // TODO: 일시정지
            Debug.Log("일시정지");
        }
        else
        {
            mIsPause = false;
            // TODO: 일시정지 해제
            Debug.Log("일시정지 해제");
        }
    }
    /* 예: 한 곳에서 부수효과 관리 */
    void OnStateChanged(GameState s)
    {
        CurrentGameState = s;
        switch (s)
        {
            case GameState.UI:   SetPause(true);             break;
            case GameState.Gameplay:    SetPause(false);     break;
            case GameState.GameplayPause: SetPause(true);    break;
        }
    }
    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }
}