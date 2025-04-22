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
        
        if (mPlayerController.IsGrounded)
        {
            if (GameManager.Instance.Input.MoveInput != Vector2.zero)
            {
                mPlayerController.SetPlayerState(PlayerState.Move);
            }
            else
            {
                mPlayerController.SetPlayerState(PlayerState.Idle);
            }
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
