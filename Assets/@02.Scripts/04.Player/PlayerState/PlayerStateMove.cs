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

        if (mPlayerController.bIsGrounded)
        {
            if (GameManager.Instance.Input.Skill_4Input)
            {
                mPlayerController.SetPlayerState(PlayerState.Skill_4);
                return;
            }
            else if (GameManager.Instance.Input.Skill_3Input)
            {
                mPlayerController.SetPlayerState(PlayerState.Skill_3);
                return;
            }

            if (GameManager.Instance.Input.RollInput && mPlayerController.RollTimeoutDelta < 0.0f)
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
