using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateDefend : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mCameraForward;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Defend", true);
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }

        if (mPlayerController.IsGrounded)
        {
            mPlayerController.Defend();
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}