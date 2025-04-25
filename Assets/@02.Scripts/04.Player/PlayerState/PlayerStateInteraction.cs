using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateInteraction : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Interaction", true);
    }

    public void OnUpdate()
    {
        // 임시 기능
        if (GameManager.Instance.Input.InteractionInput)
        {
            mPlayerController.SetPlayerState(PlayerState.Idle);
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Interaction", false);
        mPlayerController = null;
    }
}
