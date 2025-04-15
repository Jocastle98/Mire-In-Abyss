using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateAttack : IPlayerState
{
    private PlayerController mPlayerController;
    public bool bIsAttacking { get; set; }
    public bool bIsComboEnable { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController?.PlayerAnimator.SetTrigger("Attack");
    }

    public void OnUpdate()
    {
        if (GameManager.Instance.Input.AttackInput && mPlayerController.bIsGrounded && !bIsAttacking && bIsComboEnable)
        {
            mPlayerController?.PlayerAnimator.SetTrigger("Attack");
            return;
        }

        if (bIsAttacking)
        {
            Vector3 forward = mPlayerController.transform.forward;
            mPlayerController.transform.position += forward * (mPlayerController.MoveSpeed * Time.deltaTime);
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
