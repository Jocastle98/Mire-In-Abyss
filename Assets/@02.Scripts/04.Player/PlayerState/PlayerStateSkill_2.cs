using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateSkill_2 : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Skill_2", true);
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Skill_2", false);
        mPlayerController = null;
    }
}
