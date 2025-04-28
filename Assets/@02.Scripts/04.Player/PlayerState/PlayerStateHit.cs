using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateHit : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Hit");
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
