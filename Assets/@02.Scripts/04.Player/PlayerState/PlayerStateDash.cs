using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateDash : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mCameraCenterDirection;
    private Vector3 mLookDirection;
    public bool bIsDashing { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Dash");

        mCameraCenterDirection = mPlayerController.SetDashDirection();
        mLookDirection = new Vector3(mCameraCenterDirection.x, 0, mCameraCenterDirection.z);
        
        mPlayerController.transform.rotation = Quaternion.LookRotation(mLookDirection);
        mPlayerController.Dash();
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        mPlayerController?.Dashing(mCameraCenterDirection);
    }

    public void OnExit()
    {
        mPlayerController?.StopDash();
        mPlayerController = null;
    }
}
