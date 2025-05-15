using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
    [SerializeField] private PlayerInput mPlayerInput;

    private InputManager mInput = new InputManager();
    private PoolManager mPool = new PoolManager();
    private ResourceManager mResource = new ResourceManager();

    public InputManager Input { get { return Instance.mInput; } }
    public PoolManager Pool { get { return Instance.mPool; } }
    public ResourceManager Resource { get { return Instance.mResource; } }

    public GameState CurrentGameState => mStateRP.Value;
    private GameState mPreviousGameState;
    public ReadOnlyReactiveProperty<GameState> ObserveState => mStateRP;
    private readonly ReactiveProperty<GameState> mStateRP = new(GameState.Gameplay);

    private bool mIsPause = false;


    protected override void Awake()
    {
        base.Awake();
        mPlayerInput = GetComponent<PlayerInput>();
        mStateRP.Subscribe(onStateChanged);

        Init();
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

    private void Init()
    {
        Instance.Input.Init(mPlayerInput);
        Instance.Pool.Init();
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
            DOTween.defaultTimeScaleIndependent = true;
            Time.timeScale = 0;
            // TODO: 일시정지
        }
        else
        {
            mIsPause = false;
            Time.timeScale = 1;
            DOTween.defaultTimeScaleIndependent = false;
            // TODO: 일시정지 해제
        }
    }

    private void updateGameStateForScene(GameplaySceneChanged e)
    {
        GameState next;
        if (e.NewScene == GameScene.MainMenu)
        {
            next = GameState.MainMenu;
        }
        else // if(e.NewMode == GameScene.Town && e.NewMode == GameScene.Abyss)
        {
            next = GameState.Gameplay;
        }

        if (CurrentGameState != next)
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
                bool isPause = SceneLoader.CurrentSceneType == GameScene.Abyss;
                SetPause(isPause);
                setMouseCursor(true);
                break;
            case GameState.Gameplay:
                SetPause(false);
                setMouseCursor(false);
                break;
            case GameState.GameplayPause:
                SetPause(true); break;
        }
    }

    public void Clear()
    {
        Instance.Pool.Clear();
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }
}