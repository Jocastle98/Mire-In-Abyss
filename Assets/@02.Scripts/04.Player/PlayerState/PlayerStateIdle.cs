using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStateIdle : MonoBehaviour, IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Idle", true);
        
        mPlayerController.PlayerAnimator.SetFloat("Horizontal", 0.0f);
        mPlayerController.PlayerAnimator.SetFloat("Vertical", 0.0f);
    }

    public void OnUpdate()
    {
        MoveCheck();
        JumpCheck();
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Idle", false);
        mPlayerController = null;
    }

    private void MoveCheck()
    {
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;
        if ((moveInput.x != 0 || moveInput.y != 0) && mPlayerController.IsGrounded)
        {
            mPlayerController.Movement(moveInput.x, moveInput.y);
            mPlayerController.SetPlayerState(PlayerState.Move);
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
