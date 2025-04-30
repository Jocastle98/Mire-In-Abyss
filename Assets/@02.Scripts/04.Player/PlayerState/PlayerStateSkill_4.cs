using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateSkill_4 : IPlayerState
{
    private PlayerController mPlayerController;
    public bool bIsSkillActive;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.Start_Skill_4();
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
