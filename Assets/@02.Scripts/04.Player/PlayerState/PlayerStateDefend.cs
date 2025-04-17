using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateDefend : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mCameraForward;
    public bool bIsDefending { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        
        
        bIsDefending = true;
        
        mPlayerController.PlayerAnimator.SetBool("Defend", true);
    }

    public void OnUpdate()
    {
        if (!GameManager.Instance.Input.IsDefending)
        {
            mPlayerController.SetPlayerState(PlayerState.Idle);
            return;
        }
        else
        {
            mCameraForward = mPlayerController.GetCameraForwardDirection();
            mPlayerController.SetCameraForwardRotate(mCameraForward, 45.0f);
        }
    }

    public void OnExit()
    {
        bIsDefending = false;
        
        mPlayerController.PlayerAnimator.SetBool("Defend", false);
        
        mPlayerController.SetCameraForwardRotate(mCameraForward, -45.0f);
        mPlayerController = null;
    }
}
