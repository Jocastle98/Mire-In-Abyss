using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerStateAttack : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mAttackDirection;
    public bool HasReceivedNextAttackInput;
    public bool bIsComboActive;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Attack");
        
        mAttackDirection = mPlayerController.GetCameraForwardDirection(true);
        mPlayerController.transform.rotation = Quaternion.LookRotation(mAttackDirection);
        
        HasReceivedNextAttackInput = true;
        bIsComboActive = true;
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        if ((GameManager.Instance.Input.AttackInput || GameManager.Instance.Input.IsAttacking) && bIsComboActive && !HasReceivedNextAttackInput)
        {
            AttackCount++;
            mPlayerController.PlayerAnimator.SetTrigger("Attack");
            HasReceivedNextAttackInput = true;
        }
        
        mPlayerController.Attack();

        // 콤보가 끝날 때 HasReceivedNextAttackInput을 false로 설정
        if (AttackCount > 3) // 콤보가 끝났을 때
        {
            HasReceivedNextAttackInput = false;
            bIsComboActive = false;
        }
        
        if (!bIsComboActive)
        {
            if (mPlayerController.bIsGrounded)
            {
                if (GameManager.Instance.Input.MoveInput == Vector2.zero)
                {
                    mPlayerController.SetPlayerState(PlayerState.Idle);
                }
                else
                {
                    mPlayerController.SetPlayerState(PlayerState.Move);
                }
            }
            else
            {
                mPlayerController.SetPlayerState(PlayerState.Fall);
            }
        }
    }

    public void OnExit()
    {
        AttackCount = 0;
        mPlayerController = null;
    }

    private int AttackCount
    {
        get => mPlayerController.PlayerAnimator.GetInteger("Attack_Count");
        set => mPlayerController.PlayerAnimator.SetInteger("Attack_Count", value);
    }

    // todo: 콤보별 공격력 배율(무기 공격력 기준) // 임시 기능(사용할지 말지)
    public float GetComboDamage()
    {
        switch (AttackCount)
        {
            case 1:
                return 1.0f;
            case 2:
                return 1.2f;
            case 3:
                return 1.5f;
            default:
                return 1.0f;
        }
    }
}