using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateLand : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        if (mPlayerController.bIsGrounded)
        {
            if (GameManager.Instance.Input.MoveInput == Vector2.zero)
            {
                mPlayerController.SetPlayerState(PlayerState.Idle);
                return;
            }
            else
            {
                mPlayerController.SetPlayerState(PlayerState.Move);
                return;
            }
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}