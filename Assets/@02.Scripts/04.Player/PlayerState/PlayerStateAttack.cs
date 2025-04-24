using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateAttack : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mAttackDirection;
    public bool bIsAttacking { get; set; }
    public bool bIsCombo { get; set; }

    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Attack");
        
        bIsCombo = true;
        
        mAttackDirection = mPlayerController.GetCameraForwardDirection();
        mPlayerController.transform.rotation = Quaternion.LookRotation(mAttackDirection);
    }

    public void OnUpdate()
    {
        mPlayerController?.Attack();
        if (GameManager.Instance.Input.AttackInput && bIsCombo)
        {
            mPlayerController?.PlayerAnimator.SetTrigger("Attack");
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.ResetTrigger("Attack");
        mPlayerController = null;
    }
}