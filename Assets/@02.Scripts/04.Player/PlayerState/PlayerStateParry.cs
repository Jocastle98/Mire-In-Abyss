using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateParry : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mParryDirection;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Parry");
        
        mParryDirection = mPlayerController.GetCameraForwardDirection();
        mPlayerController.transform.rotation = Quaternion.LookRotation(mParryDirection);
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }

        if (mPlayerController.IsGrounded)
        {
            mPlayerController?.Parry();
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Idle", false);
        mPlayerController.PlayerAnimator.SetBool("Move", false);
        mPlayerController = null;
    }
}