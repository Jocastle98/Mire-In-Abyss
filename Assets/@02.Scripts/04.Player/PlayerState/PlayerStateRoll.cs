using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateRoll : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mRollDirection;
    public bool bIsRoll { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Roll");

        mRollDirection = mPlayerController.SetRollDirection();
        mPlayerController.transform.rotation = Quaternion.LookRotation(mRollDirection);
        mPlayerController.StartRoll(mRollDirection);
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
    }

    public void OnExit()
    {
        mPlayerController?.StopRoll();
        mPlayerController = null;
    }
}