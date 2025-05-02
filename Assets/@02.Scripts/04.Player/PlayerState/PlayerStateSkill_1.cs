using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateSkill_1 : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mSkillDirection;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetLayerWeight(2, 1.0f);
        mPlayerController.PlayerAnimator.SetTrigger("Skill");
        mPlayerController.PlayerAnimator.SetInteger("Skill_Index", 1);
        
        mSkillDirection = mPlayerController.GetCameraForwardDirection(true);
        mPlayerController.transform.rotation = Quaternion.LookRotation(mSkillDirection);
        
        mPlayerController.Skill_1();
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
