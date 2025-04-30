using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateSkill_4 : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mSkillDirection;
    public bool bIsSkillActive;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        
        mSkillDirection = mPlayerController.GetCameraForwardDirection(true);
        mPlayerController.transform.rotation = Quaternion.LookRotation(mSkillDirection);
        mPlayerController.Start_Skill_4();
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController.Stop_Skill_4();
        mPlayerController.PlayerAnimator.SetInteger("Skill_Index", 0);
        mPlayerController = null;
    }
}
