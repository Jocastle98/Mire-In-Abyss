using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateAttack : IPlayerState
{
    private PlayerController mPlayerController;
    public bool bIsAttacking { get; set; }
    public bool bIsCombo { get; set; }

    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Attack");
        
        bIsCombo = true;
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