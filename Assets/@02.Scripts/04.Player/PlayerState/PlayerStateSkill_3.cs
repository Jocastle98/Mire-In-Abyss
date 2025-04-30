using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateSkill_3 : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mSkillDirection;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Skill");
        mPlayerController.PlayerAnimator.SetInteger("Skill_Index", 3);
        
        mSkillDirection = mPlayerController.GetCameraForwardDirection(true);
        mPlayerController.transform.rotation = Quaternion.LookRotation(mSkillDirection);
        
        mPlayerController.Skill_3();
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetInteger("Skill_Index", 0);
        mPlayerController = null;
    }
}
