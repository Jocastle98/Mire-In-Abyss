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
        if (mPlayerController.ActionCheck())
        {
            mPlayerController.SetPlayerState(PlayerState.Land);
        }
        else
        {
            DashCheck();
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Fall", false);
        mPlayerController = null;
    }
    
    private void DashCheck()
    {
        if (mPlayerController == null)
        {
            return;
        }

        if (GameManager.Instance.Input.DashInput)
        {
            mPlayerController.SetPlayerState(PlayerState.Dash);
            return;
        }
    }
}
