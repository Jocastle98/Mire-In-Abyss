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
        mPlayerController.PlayerAnimator.SetLayerWeight(2, 1.0f);
        mPlayerController.PlayerAnimator.SetTrigger("Roll");

        mRollDirection = mPlayerController.GetActionDirection(true, true);
        mPlayerController.transform.rotation = Quaternion.LookRotation(mRollDirection);
        mPlayerController.StartRoll(mRollDirection);
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetLayerWeight(2, 0.0f);
        mPlayerController?.StopRoll();
        mPlayerController = null;
    }
}