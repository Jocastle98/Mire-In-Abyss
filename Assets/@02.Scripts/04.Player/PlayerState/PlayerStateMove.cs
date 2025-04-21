using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateMove : IPlayerState
{
    private PlayerController mPlayerController;
    private float originMoveSpeed;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Move", true);
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
                mPlayerController.Move();
            }
            else
            {
                mPlayerController.SetPlayerState(PlayerState.Idle);
            }

            if (GameManager.Instance.Input.JumpInput)
            {
                mPlayerController.SetPlayerState(PlayerState.Jump);
            }
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Move", false);
        mPlayerController = null;
    }
}
