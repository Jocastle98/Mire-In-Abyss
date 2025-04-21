using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateRoll : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Roll");
    }

    public void OnUpdate()
    {
        //mPlayerController.Roll();
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}