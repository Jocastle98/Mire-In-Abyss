using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateFall : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Fall", true);
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        if (mPlayerController.IsGrounded)
        {
            mPlayerController.SetPlayerState(PlayerState.Land);
        }
        else
        {
            /*if (GameManager.Instance.Input.AttackInput)
            {
                mPlayerController.SetPlayerState(Enums.PlayerState.Attack);
            }*/
            
            mPlayerController.Fall();
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Fall", false);
        mPlayerController = null;
    }
}
