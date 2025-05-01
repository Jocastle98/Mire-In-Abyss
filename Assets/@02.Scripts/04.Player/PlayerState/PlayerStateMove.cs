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

        if (mPlayerController.bIsGrounded)
        {
            if (GameManager.Instance.Input.RollInput && mPlayerController.RollTimeoutDelta < 0.0f)
            {
                mPlayerController.SetPlayerState(PlayerState.Roll);
                return;
            }
            
            if (GameManager.Instance.Input.DashInput && mPlayerController.DashTimeoutDelta < 0.0f)
            {
                mPlayerController.SetPlayerState(PlayerState.Dash);
                return;
            }
            
            if (GameManager.Instance.Input.ParryInput)
            {
                mPlayerController.SetPlayerState(PlayerState.Parry);
                return;
            }
            
            if (GameManager.Instance.Input.Skill_1Input && mPlayerController.Skill_1_TimeoutDelta < 0.0f)
            {
                mPlayerController.SetPlayerState(PlayerState.Skill_1);
                return;
            }
            else if (GameManager.Instance.Input.Skill_2Input)
            {
                mPlayerController.SetPlayerState(PlayerState.Skill_2);
                return;
            }
            else if (GameManager.Instance.Input.Skill_3Input && mPlayerController.Skill_3_TimeoutDelta < 0.0f)
            {
                mPlayerController.SetPlayerState(PlayerState.Skill_3);
                return;
            }
            else if (GameManager.Instance.Input.Skill_4Input && mPlayerController.Skill_4_TimeoutDelta < 0.0f)
            {
                mPlayerController.SetPlayerState(PlayerState.Skill_4);
                return;
            }
            
            if (GameManager.Instance.Input.AttackInput || GameManager.Instance.Input.IsAttacking)
            {
                mPlayerController.SetPlayerState(PlayerState.Attack);
                return;
            }
            
            if (GameManager.Instance.Input.DefendInput)
            {
                mPlayerController.SetPlayerState(PlayerState.Defend);
                return;
            }
            
            if (GameManager.Instance.Input.JumpInput && mPlayerController.JumpTimeoutDelta < 0.0f)
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
