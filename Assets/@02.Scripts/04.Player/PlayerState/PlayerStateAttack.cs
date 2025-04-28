using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;
using UnityEngine.Rendering;

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
        
        // 공격 입력과 콤보 활성화 체크
        if ((GameManager.Instance.Input.AttackInput) && bIsComboActive)
        {
            mPlayerController.PlayerAnimator.SetTrigger("Attack");
            return;
        }

        mPlayerController.Attack();
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}