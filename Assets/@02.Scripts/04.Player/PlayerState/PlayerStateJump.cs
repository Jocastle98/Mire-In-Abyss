using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateJump : IPlayerState
{
    private PlayerController mPlayerController;
    public bool bIsJumping = false;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Jump");
        mPlayerController.Jump();
    }

    public void OnUpdate()
    {
        if (mPlayerController.GetComponent<Rigidbody>().velocity.y < 0)
        {
            mPlayerController.SetPlayerState(PlayerState.Fall);
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
