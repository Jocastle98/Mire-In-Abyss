using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput))]
public class GameManager : Singleton<GameManager>
{
    [SerializeField] PlayerInput mPlayerInput;
    private InputManager mInput = new InputManager();
    public InputManager Input { get { return Instance.mInput; } }

    protected override void Awake()
    {
        base.Awake();
        Input.Init(mPlayerInput);
        // Temp
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        mInput.OnInputUpdate();
    }
    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }
}