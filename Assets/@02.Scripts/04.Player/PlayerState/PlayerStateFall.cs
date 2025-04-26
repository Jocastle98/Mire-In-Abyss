using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateFall : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Fall", true);
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        if (!mPlayerController.bIsGrounded)
        {
            if (GameManager.Instance.Input.DashInput && mPlayerController.DashTimeoutDelta < 0.0f)
            {
                mPlayerController.SetPlayerState(PlayerState.Dash);
                return;
            }
            
            if (GameManager.Instance.Input.AttackInput)
            {
                mPlayerController.SetPlayerState(PlayerState.Attack);
                return;
            }
            
            mPlayerController.Fall();
        }
        else
        {
            mPlayerController.SetPlayerState(PlayerState.Land);
            return;
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Fall", false);
        mPlayerController = null;
    }
}