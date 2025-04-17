using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateParry : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mCameraForward;
    public bool bIsParrying { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mCameraForward = mPlayerController.GetCameraForwardDirection();
        
        bIsParrying = false;
        
        mPlayerController.PlayerAnimator.SetTrigger("Parry");
    }

    public void OnUpdate()
    {
        mPlayerController.SetCameraForwardRotate(mCameraForward, 0.0f);
        
        if (bIsParrying)
        {
            // if (공격 받으면) { 강한 공격으로 반격; }
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
