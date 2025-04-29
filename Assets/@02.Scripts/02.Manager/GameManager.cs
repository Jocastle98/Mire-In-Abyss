using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private InputManager minput = new InputManager();
    public InputManager Input { get { return Instance.minput; } }

    void Awake()
    {
        // Temp
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        minput.OnInputUpdate();
    }
    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }
}