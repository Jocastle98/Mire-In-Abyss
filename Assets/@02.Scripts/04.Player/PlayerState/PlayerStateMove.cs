using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateMove : IPlayerState
{
    private PlayerController mPlayerController;
    private float mSpeed; // 0 = 걷기, 1 = 달리기
    private float originMoveSpeed;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController?.PlayerAnimator.SetBool("Move", true);
        mSpeed = 0.0f;
        originMoveSpeed = mPlayerController.MoveSpeed;
    }

    public void OnUpdate()
    {
        Moving();
        JumpCheck();
        RollCheck();
        AttackCheck();
    }

    public void OnExit()
    {
        mPlayerController?.SetPlayerMoveSpeed(originMoveSpeed);
        mPlayerController?.PlayerAnimator.SetBool("Move", false);
        mPlayerController = null;
    }

    private void Moving()
    {
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;
        
        if ((moveInput.x != 0 || moveInput.y != 0) && mPlayerController.bIsGrounded)
        {
            mPlayerController?.Movement(moveInput.y, moveInput.x);
            
            if (mSpeed < 1.0f)
            {
                mSpeed += Time.deltaTime * 0.5f;
                mSpeed = Mathf.Clamp01(mSpeed);
                mPlayerController?.SetPlayerMoveSpeed(originMoveSpeed + (originMoveSpeed * 2.0f * (mSpeed / 1.0f)));
            }
            
            mPlayerController?.PlayerAnimator.SetFloat("Horizontal", moveInput.x);
            mPlayerController?.PlayerAnimator.SetFloat("Vertical", moveInput.y);
            mPlayerController?.PlayerAnimator.SetFloat("Speed", mSpeed);
        }
        else
        {
            mPlayerController?.SetPlayerMoveSpeed(originMoveSpeed);
            mPlayerController?.SetPlayerState(PlayerState.Idle);
            return;
        }
    }
    
    private void JumpCheck()
    {
        if (GameManager.Instance.Input.JumpInput && mPlayerController.bIsGrounded && !mPlayerController.CheckJumping())
        {
            mPlayerController?.SetPlayerState(PlayerState.Jump);
            return;
        }
    }
    
    private void RollCheck()
    {
        if (GameManager.Instance.Input.RollInput && mPlayerController.bIsGrounded && !mPlayerController.CheckRolling())
        {
            mPlayerController?.SetPlayerState(PlayerState.Roll);
            return;
        }
    }
    
    private void AttackCheck()
    {
        if (GameManager.Instance.Input.AttackInput && mPlayerController.bIsGrounded)
        {
            mPlayerController?.SetPlayerState(PlayerState.Attack);
            return;
        }
    }
}
