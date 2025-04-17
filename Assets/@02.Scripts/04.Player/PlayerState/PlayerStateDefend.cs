using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateDefend : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mCameraForward;
    public bool bIsDefending { get; set; }
    public bool bIsDefendEnd { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.GetCameraForwardDirection();
        
        bIsDefending = true;
        bIsDefendEnd = false;
        
        mPlayerController.PlayerAnimator.SetBool("Defend", true);
    }

    public void OnUpdate()
    {
        mCameraForward = mPlayerController.GetCameraForwardDirection();
        mPlayerController.SetCameraForwardRotate(mCameraForward, 45.0f);

        if (!GameManager.Instance.Input.IsDefending)
        {
            mPlayerController.PlayerAnimator.SetBool("Defend", false);
            mPlayerController.SetPlayerState(PlayerState.Idle);
            return;
        }
    }

    public void OnExit()
    {
        mPlayerController.SetCameraForwardRotate(mCameraForward, -45.0f);
        mPlayerController = null;
    }
}
