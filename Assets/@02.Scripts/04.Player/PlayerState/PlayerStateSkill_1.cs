using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateSkill_1 : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Skill");
        mPlayerController.PlayerAnimator.SetInteger("Skill_Index", 1);
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
