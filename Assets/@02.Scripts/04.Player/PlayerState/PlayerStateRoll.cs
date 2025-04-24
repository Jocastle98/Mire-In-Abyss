using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateRoll : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mTargetDirection;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Roll");

        mTargetDirection = mPlayerController.SetTargetDirection();
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        mPlayerController?.Roll(mTargetDirection);
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}