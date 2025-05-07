using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateSkill_3 : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mSkillDirection;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetLayerWeight(2, 1.0f);
        
        mSkillDirection = mPlayerController.GetCameraForwardDirection(true);
        mPlayerController.transform.rotation = Quaternion.LookRotation(mSkillDirection);
        mPlayerController.Start_Skill_3();
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController.Stop_Skill_3();
        mPlayerController.PlayerAnimator.SetLayerWeight(2, 0.0f);
        mPlayerController.PlayerAnimator.SetInteger("Skill_Index", 0);
        mPlayerController = null;
    }
}
