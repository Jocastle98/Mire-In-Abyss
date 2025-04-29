using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
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