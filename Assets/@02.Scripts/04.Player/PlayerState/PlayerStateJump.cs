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
        
        if (GameManager.Instance.Input.Skill_1Input && mPlayerController.Skill_1_TimeoutDelta < 0.0f)
        {
            mPlayerController.SetPlayerState(PlayerState.Skill_1);
            return;
        }
        else if (GameManager.Instance.Input.Skill_2Input && mPlayerController.Skill_2_TimeoutDelta < 0.0f)
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

        mPlayerController.Jump();
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Jump", false);
        mPlayerController = null;
    }
}