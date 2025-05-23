using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PlayerEnums;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerStateAttack : IPlayerState
{
    private PlayerController mPlayerController;
    private int mUpperBodyLayer;
    private Vector3 mAttackDirection;
    private int mMaxCombo = 3;
    private int mCurrentCombo = 0;
    public bool HasReceivedNextAttackInput;
    public bool bIsComboActive;
    
    private float mComboInputWindow = 2.0f;
    private float mComboInputTimer;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mUpperBodyLayer = mPlayerController.PlayerAnimator.GetLayerIndex("UpperBody Layer");
        mPlayerController.PlayerAnimator.SetLayerWeight(mUpperBodyLayer, 1.0f);
        mPlayerController.PlayerAnimator.SetTrigger("Attack");
        
        mAttackDirection = mPlayerController.GetCameraForwardDirection(true);
        mPlayerController.transform.rotation = Quaternion.LookRotation(mAttackDirection);
        
        HasReceivedNextAttackInput = true;
        bIsComboActive = true;
        mComboInputTimer = 0.0f;
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        mComboInputTimer += Time.deltaTime;
        if (bIsComboActive && mComboInputTimer > mComboInputWindow)
        {
            bIsComboActive = false;
            mPlayerController.StopSlashCoroutine();
        }
        
        if ((GameManager.Instance.Input.AttackInput || GameManager.Instance.Input.IsAttacking) 
             && bIsComboActive && !HasReceivedNextAttackInput)
        {
            AttackCount++;
            mPlayerController.PlayerAnimator.SetTrigger("Attack");
            HasReceivedNextAttackInput = true;
            mComboInputTimer = 0.0f; // 콤보 연속 입력 받았으므로 타이머 리셋
        }
        
        mPlayerController.Attack();

        // 콤보가 끝날 때 HasReceivedNextAttackInput을 false로 설정
        if (AttackCount >= mMaxCombo)
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
        mPlayerController.PlayerAnimator.SetLayerWeight(mUpperBodyLayer, 0.0f);
        mPlayerController = null;
    }

    private int AttackCount
    {
        get => mCurrentCombo;
        set
        {
            mCurrentCombo = value;
            if (mPlayerController != null)
            {
                mPlayerController.PlayerAnimator.SetInteger("Attack_Count", value);
            }
        }
    }

    public GameObject AttackEffect()
    {
        GameObject slashEffectObject = null;
        
        switch (AttackCount)
        {
            case 0:
                slashEffectObject = mPlayerController.SlashEffect(SlashEffectType.RightToLeft);
                break;
            case 1:
                slashEffectObject = mPlayerController.SlashEffect(SlashEffectType.LeftToRight);
                break;
            case 2:
                slashEffectObject = mPlayerController.SlashEffect(SlashEffectType.RightToLeft);
                break;
        }

        return slashEffectObject;
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