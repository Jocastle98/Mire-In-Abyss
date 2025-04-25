using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateRoll : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mTargetDirection;
    public bool bIsRoll { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Roll");

        mTargetDirection = mPlayerController.SetRollDirection();
        mPlayerController.transform.rotation = Quaternion.LookRotation(mTargetDirection);
        mPlayerController.Roll();
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        mPlayerController?.Rolling(mTargetDirection);
    }

    public void OnExit()
    {
        mPlayerController?.StopRoll();
        mPlayerController = null;
    }
}