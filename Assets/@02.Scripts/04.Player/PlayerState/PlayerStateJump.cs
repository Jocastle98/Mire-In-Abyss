using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateJump : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Jump", true);
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        if (GameManager.Instance.Input.DashInput && mPlayerController.DashTimeoutDelta < 0.0f)
        {
            mPlayerController.SetPlayerState(PlayerState.Dash);
            return;
        }
        
        if (GameManager.Instance.Input.AttackInput || GameManager.Instance.Input.IsAttacking)
        {
            mPlayerController.SetPlayerState(PlayerState.Attack);
            return;
        }
        
        mPlayerController.Jump();
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Jump", false);
        mPlayerController = null;
    }
}