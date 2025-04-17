using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateAttack : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mCameraForward;
    public bool bIsComboEnable { get; set; }
    public bool bIsComboInputCheck { get; set; }
    public bool bIsAttacking { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mCameraForward = mPlayerController.GetCameraForwardDirection();
        
        bIsComboEnable = true;
        bIsComboInputCheck = false;
        bIsAttacking = false;
        
        mPlayerController.PlayerAnimator.SetTrigger("Attack");
    }

    public void OnUpdate()
    {
        mPlayerController.SetCameraForwardRotate(mCameraForward, 0.0f);
        
        if (GameManager.Instance.Input.AttackInput && bIsComboEnable)
        {
            mCameraForward = mPlayerController.GetCameraForwardDirection();
            
            bIsComboInputCheck = true;
            
            mPlayerController.PlayerAnimator.SetTrigger("Attack");
            return;
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}