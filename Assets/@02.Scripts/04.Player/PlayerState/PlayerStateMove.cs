using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateMove : MonoBehaviour, IPlayerState
{
    private PlayerController mPlayerController;
    private float mSpeed; // 0 = 일반 속도, 1 = 전력질주
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Move", true);
        mSpeed = 0.0f;
    }

    public void OnUpdate()
    {
        Moving();
        JumpCheck();
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Move", false);
        mPlayerController = null;
    }

    private void Moving()
    {
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;
        
        if ((moveInput.x != 0 || moveInput.y != 0) && mPlayerController.IsGrounded)
        {
            mPlayerController.Movement(moveInput.y, moveInput.x);
            
            // 스플린트
            if (moveInput.y > 0.0f)
            {
                if (GameManager.Instance.Input.SprintInput)
                {
                    if (mSpeed < 1.0f)
                    {
                        mSpeed += Time.deltaTime;
                        mSpeed = Mathf.Clamp01(mSpeed);
                    }
                }
                else
                {
                    if (mSpeed > 0.0f)
                    {
                        mSpeed -= Time.deltaTime;
                        mSpeed = Mathf.Clamp01(mSpeed);
                    }
                }
            }
            
            mPlayerController.PlayerAnimator.SetFloat("Horizontal", moveInput.x);
            mPlayerController.PlayerAnimator.SetFloat("Vertical", moveInput.y);
            mPlayerController.PlayerAnimator.SetFloat("Speed", mSpeed);
        }
        else
        {
            mPlayerController.SetPlayerState(PlayerState.Idle);
            return;
        }
    }
    
    private void JumpCheck()
    {
        if (GameManager.Instance.Input.JumpInput && mPlayerController.IsGrounded)
        {
            mPlayerController.SetPlayerState(PlayerState.Jump);
            return;
        }
    }
    
    private void AttackCheck()
    {
        if (GameManager.Instance.Input.AttackInput && mPlayerController.IsGrounded)
        {
            mPlayerController.SetPlayerState(PlayerState.Attack);
            return;
        }
    }
}
