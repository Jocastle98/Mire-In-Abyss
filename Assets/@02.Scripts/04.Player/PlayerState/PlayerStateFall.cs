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
            
            if (GameManager.Instance.Input.AttackInput || GameManager.Instance.Input.IsAttacking)
            {
                mPlayerController.SetPlayerState(PlayerState.Attack);
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
            else if (GameManager.Instance.Input.Skill_3Input)
            {
                mPlayerController.SetPlayerState(PlayerState.Skill_3);
                return;
            }
            else if (GameManager.Instance.Input.Skill_4Input)
            {
                mPlayerController.SetPlayerState(PlayerState.Skill_4);
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