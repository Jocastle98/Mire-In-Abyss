using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateSkill_2 : IPlayerState
{
    private PlayerController mPlayerController;
    private int mSkillLayer;
    private Vector3 mSkillDirection;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mSkillLayer = mPlayerController.PlayerAnimator.GetLayerIndex("Skill Layer");
        mPlayerController.PlayerAnimator.SetLayerWeight(mSkillLayer, 1.0f);
        mPlayerController.PlayerAnimator.SetTrigger("Skill");
        mPlayerController.PlayerAnimator.SetInteger("Skill_Index", 2);
        
        mSkillDirection = mPlayerController.GetCameraForwardDirection(true);
        mPlayerController.transform.rotation = Quaternion.LookRotation(mSkillDirection);

        mPlayerController.Start_Skill_2();
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController.Stop_Skill_2();
        mPlayerController.PlayerAnimator.SetLayerWeight(mSkillLayer, 0.0f);
        mPlayerController.PlayerAnimator.SetInteger("Skill_Index", 0);
        mPlayerController = null;
    }
}