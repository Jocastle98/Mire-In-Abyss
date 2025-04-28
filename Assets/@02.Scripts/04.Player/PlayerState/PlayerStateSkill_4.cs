using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateSkill_4 : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Skill_4");
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
