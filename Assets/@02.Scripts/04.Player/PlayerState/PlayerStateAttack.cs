using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateAttack : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mAttackDirection;
    public bool bIsComboActive;

    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Attack");
        
        mAttackDirection = mPlayerController.GetCameraForwardDirection(true);
        mPlayerController.transform.rotation = Quaternion.LookRotation(mAttackDirection);
        
        bIsComboActive = true;
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        if ((GameManager.Instance.Input.AttackInput || GameManager.Instance.Input.IsAttacking) && bIsComboActive)
        {
            mPlayerController.PlayerAnimator.SetTrigger("Attack");
        }
        
        mPlayerController.Attack();
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.ResetTrigger("Attack");
        mPlayerController = null;
    }
}