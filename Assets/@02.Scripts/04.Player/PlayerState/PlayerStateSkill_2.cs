using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateSkill_2 : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetLayerWeight(2, 1.0f);
        mPlayerController.PlayerAnimator.SetTrigger("Skill");
        mPlayerController.PlayerAnimator.SetInteger("Skill_Index", 2);
    }

    public void OnUpdate()
    {
        if (mPlayerController.bIsGrounded)
        {
            if (GameManager.Instance.Input.RollInput && mPlayerController.RollTimeoutDelta < 0.0f)
            {
                mPlayerController.SetPlayerState(PlayerState.Roll);
                return;
            }
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetLayerWeight(2, 0.0f);
        mPlayerController.PlayerAnimator.SetInteger("Skill_Index", 0);
        mPlayerController = null;
    }
}