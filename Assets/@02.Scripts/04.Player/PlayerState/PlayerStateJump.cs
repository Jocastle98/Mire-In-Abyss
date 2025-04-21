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
        
        mPlayerController.Jump();
        
        if (GameManager.Instance.Input.AttackInput)
        {
            mPlayerController?.SetPlayerState(PlayerState.Attack);
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Jump", false);
        mPlayerController = null;
    }
}