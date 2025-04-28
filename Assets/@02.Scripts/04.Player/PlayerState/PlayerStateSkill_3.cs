using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateSkill_3 : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Skill_3");
        mPlayerController.PlayerAnimator.SetBool("IsSkill_3", true);
        mPlayerController.WhirlwindTest();
    }

    public void OnUpdate()
    {
        mPlayerController.Move();
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("IsSkill_3", false);
        mPlayerController = null;
    }
}
