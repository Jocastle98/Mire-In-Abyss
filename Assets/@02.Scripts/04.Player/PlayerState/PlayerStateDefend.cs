using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateDefend : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mDefendDirection;
    
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

        if (mPlayerController.bIsGrounded)
        {
            mDefendDirection = mPlayerController.GetCameraForwardDirection(true);
            mPlayerController.transform.rotation = Quaternion.LookRotation(mDefendDirection);

            if (GameManager.Instance.Input.RollInput && mPlayerController.RollTimeoutDelta < 0.0f)
            {
                mPlayerController.SetPlayerState(PlayerState.Roll);
                return;
            }

            if (GameManager.Instance.Input.ParryInput && mPlayerController.ParryTimeoutDelta < 0.0f)
            {
                mPlayerController.SetPlayerState(PlayerState.Parry);
                return;
            }
            
            mPlayerController.Defend();
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Defend", false);
        mPlayerController = null;
    }
}