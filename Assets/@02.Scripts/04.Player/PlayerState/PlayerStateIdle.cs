using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStateIdle : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        
        mPlayerController.PlayerAnimator.SetBool("Idle", true);
        
        mPlayerController.GetComponent<Rigidbody>().velocity = Vector3.zero;
        mPlayerController.PlayerAnimator.SetFloat("Vertical", 0.0f);
    }

    public void OnUpdate()
    {
        MoveCheck();
        WalkAndRunSpeedLess();
        JumpCheck();
        mPlayerController?.FallCheck();
        RollCheck();
        AttackCheck();
        DefendCheck();
        ParryCheck();
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Idle", false);
        mPlayerController = null;
    }

    private void MoveCheck()
    {
        Vector2 moveInput = GameManager.Instance.Input.MoveInput;
        
        if ((moveInput.x != 0 || moveInput.y != 0) && mPlayerController.ActionCheck())
        {
            mPlayerController.SetPlayerState(PlayerState.Move);
            return;
        }
    }

    private void WalkAndRunSpeedLess()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        float mSpeed = mPlayerController.walkAndRunSpeed;
        if (mSpeed > 0.0f)
        {
            mSpeed -= Time.deltaTime * 0.5f;
            mSpeed = Mathf.Clamp01(mSpeed);
            
            mPlayerController.SetWalkAndRunSpeed(mSpeed);
            mPlayerController.PlayerAnimator.SetFloat("Speed", mSpeed);
        }
    }

    private void JumpCheck()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        if (GameManager.Instance.Input.JumpInput && mPlayerController.ActionCheck())
        {
            mPlayerController.SetPlayerState(PlayerState.Jump);
            return;
        }
    }

    private void RollCheck()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        if (GameManager.Instance.Input.RollInput && mPlayerController.ActionCheck())
        {
            mPlayerController.SetPlayerState(PlayerState.Roll);
            return;
        }
    }

    private void AttackCheck()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        if (GameManager.Instance.Input.AttackInput && mPlayerController.ActionCheck())
        {
            mPlayerController.SetPlayerState(PlayerState.Attack);
            return;
        }
    }

    private void DefendCheck()
    {
        if (mPlayerController == null)
        {
            return;
        }

        if (GameManager.Instance.Input.DefendInput && mPlayerController.ActionCheck())
        {
            mPlayerController.SetPlayerState(PlayerState.Defend);
            return;
        }
    }

    private void ParryCheck()
    {
        if (mPlayerController == null)
        {
            return;
        }

        if (GameManager.Instance.Input.ParryInput && mPlayerController.ActionCheck())
        {
            mPlayerController.SetPlayerState(PlayerState.Parry);
            return;
        }
    }
}