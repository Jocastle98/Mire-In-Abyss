using Cysharp.Threading.Tasks;
using Events.Gameplay;
using GameEnums;
using R3;
using SceneEnums;
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

    public GameState CurrentGameState => mStateRP.Value;
    private GameState mPreviousGameState;
    public ReadOnlyReactiveProperty<GameState> ObserveState => mStateRP;
    private readonly ReactiveProperty<GameState> mStateRP = new(GameState.Gameplay);

    private bool mIsPause = false;


    protected override void Awake()
    {
        base.Awake();
        mPlayerInput = GetComponent<PlayerInput>();
        Input.Init(mPlayerInput);
        mStateRP.Subscribe(onStateChanged);
    }
    
    private void Start()
    {
        subscribeEvents();
    }

    private void subscribeEvents()
    {
        R3EventBus.Instance.Receive<GameplaySceneChanged>()
            .Subscribe(e => updateGameStateForScene(e));
    }

    private void Update()
    {
        mInput.OnInputUpdate();
    }

    public void SetGameState(GameState next)
    {
        if (CurrentGameState != next) 
        {
            mPreviousGameState = CurrentGameState;
            mStateRP.Value = next; 
        }
    }

    public void ChangePreviousGameState()
    {
        SetGameState(mPreviousGameState);
    }

    public async UniTask PauseForSeconds(float sec)
    {
        SetGameState(GameState.GameplayPause);
        Time.timeScale = 0;
        await UniTask.Delay(System.TimeSpan.FromSeconds(sec));
        Time.timeScale = 1;
        SetGameState(GameState.Gameplay);
    }

    public void SetPause(bool isPaused)
    {
        if (isPaused)
        {
            mIsPause = true;
            // TODO: 일시정지
        }
        else
        {
            mIsPause = false;
            // TODO: 일시정지 해제
        }
    }

    private void updateGameStateForScene(GameplaySceneChanged e)
    {
        GameState next;
        if(e.NewScene == GameScene.MainMenu)
        {
            next = GameState.MainMenu;
        }
        else // if(e.NewMode == GameScene.Town && e.NewMode == GameScene.Abyss)
        {
            next = GameState.Gameplay;
        }

        if(CurrentGameState != next)
        {
            SetGameState(next);
        }
    }

    private void setMouseCursor(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
    /* 예: 한 곳에서 부수효과 관리 */
    void onStateChanged(GameState s)
    {
        switch (s)
        {
            case GameState.MainMenu:
                SetPause(false);
                setMouseCursor(true);
                break;
            case GameState.UI:   
                SetPause(true);
                setMouseCursor(true);
                break;
            case GameState.Gameplay:   
                SetPause(false);
                setMouseCursor(false);
                break;
            case GameState.GameplayPause: 
                SetPause(true);    break;
        }
    }
    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }
}