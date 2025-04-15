using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateRoll : MonoBehaviour, IPlayerState
{
    private PlayerController _playerController;
    
    public void OnEnter(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        
    }
}
