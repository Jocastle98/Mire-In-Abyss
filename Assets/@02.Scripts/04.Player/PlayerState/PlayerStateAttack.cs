using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateAttack : IPlayerState
{
    private PlayerController mPlayerController;
    public bool bIsAttacking { get; set; }
    public bool bIsComboEnable { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        bIsAttacking = false;
        bIsComboEnable = false;
        mPlayerController.PlayerAnimator.SetTrigger("Attack");
    }

    public void OnUpdate()
    {
        AttackRotate();
        
        if (GameManager.Instance.Input.AttackInput && bIsComboEnable)
        {
            mPlayerController.PlayerAnimator.SetTrigger("Attack");
            return;
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
    
    private void AttackRotate()
    {
        // 카메라 설정
        var cameraTransform = Camera.main.transform;
        var cameraForward = cameraTransform.forward;
        
        // Y값을 0으로 설정해서 수평 방향만 고려
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        mPlayerController.transform.rotation = Quaternion.Slerp(mPlayerController.transform.rotation, 
            targetRotation, mPlayerController.TurnSpeed * Time.deltaTime);
    }
}