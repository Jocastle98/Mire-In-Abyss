using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateDash : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mCameraDirection;
    public bool bIsDashing { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Dash");

        mCameraDirection = mPlayerController.GetCameraForwardDirection();
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        mPlayerController?.Dash(mCameraDirection);
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
