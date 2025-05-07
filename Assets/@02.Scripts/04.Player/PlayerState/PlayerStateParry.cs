using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateParry : IPlayerState
{
    private PlayerController mPlayerController;
    private int upperBodyLayer;
    private Vector3 mParryDirection;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        upperBodyLayer = mPlayerController.PlayerAnimator.GetLayerIndex("UpperBody Layer");
        mPlayerController.PlayerAnimator.SetLayerWeight(upperBodyLayer, 1.0f);
        mPlayerController.PlayerAnimator.SetTrigger("Parry");
        
        mParryDirection = mPlayerController.GetCameraForwardDirection(true);
        mPlayerController.transform.rotation = Quaternion.LookRotation(mParryDirection);
        mPlayerController.StartParry();
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }

        if (mPlayerController.bIsGrounded)
        {
            mPlayerController.Parry();
        }
    }

    public void OnExit()
    {
        mPlayerController.StopParry();
        mPlayerController.PlayerAnimator.SetLayerWeight(upperBodyLayer, 0.0f);
        mPlayerController = null;
    }
}