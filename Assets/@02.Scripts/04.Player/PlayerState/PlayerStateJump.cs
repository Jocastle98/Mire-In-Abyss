using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateJump : IPlayerState
{
    private PlayerController mPlayerController;
    public bool bIsJumping { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        
        bIsJumping = false;
        
        mPlayerController.PlayerAnimator.SetTrigger("Jump");
        mPlayerController.Jump();
    }

    public void OnUpdate()
    {
        if (mPlayerController.GetComponent<Rigidbody>().velocity.y < 0)
        {
            mPlayerController.SetPlayerState(PlayerState.Fall);
            return;
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}