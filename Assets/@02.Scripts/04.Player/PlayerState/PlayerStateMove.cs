using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateMove : IPlayerState
{
    private PlayerController mPlayerController;
    
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
        
        if (GameManager.Instance.Input.AttackInput)
        {
            mPlayerController.SetPlayerState(PlayerState.Attack);
            return;
        }

        if (GameManager.Instance.Input.DashInput)
        {
            mPlayerController.SetPlayerState(PlayerState.Dash);
            return;
        }

        if (mPlayerController.bIsGrounded)
        {
            if (GameManager.Instance.Input.RollInput)
            {
                mPlayerController.SetPlayerState(PlayerState.Roll);
                return;
            }
            
            if (GameManager.Instance.Input.ParryInput)
            {
                mPlayerController.SetPlayerState(PlayerState.Parry);
                return;
            }
            
            if (GameManager.Instance.Input.DefendInput)
            {
                mPlayerController.SetPlayerState(PlayerState.Defend);
                return;
            }
            
            if (GameManager.Instance.Input.JumpInput)
            {
                mPlayerController.SetPlayerState(PlayerState.Jump);
                return;
            }
            
            if (GameManager.Instance.Input.InteractionInput && mPlayerController.NearestInteractableObject != null)
            {
                mPlayerController.SetPlayerState(PlayerState.Interaction);
                return;
            }
            
            if (GameManager.Instance.Input.MoveInput != Vector2.zero)
            {
                mPlayerController.Move();
            }
            else
            {
                mPlayerController.SetPlayerState(PlayerState.Idle);
            }
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Move", false);
        mPlayerController = null;
    }
}
