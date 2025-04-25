using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateStun : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Stun", true);
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Stun", false);
        mPlayerController = null;
    }
}
