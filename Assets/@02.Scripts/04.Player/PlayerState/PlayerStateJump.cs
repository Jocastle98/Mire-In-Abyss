using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateJump : IPlayerState
{
    private PlayerController mPlayerController;
    private bool mIsDelayJumpCheck;
    private float mJumpTimer = 0f;
    private float mJumpGroundCheckIgnoreTime = 0.3f;
    
    public bool bIsJumping { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;

        mIsDelayJumpCheck = true;
        mJumpTimer = 0.0f;
        bIsJumping = false;
        
        mPlayerController.PlayerAnimator.SetTrigger("Jump");
        mPlayerController.Jump();
    }

    public void OnUpdate()
    {
        if (mIsDelayJumpCheck)
        {
            mJumpTimer += Time.deltaTime;
            
            if (mJumpTimer >= mJumpGroundCheckIgnoreTime)
            {
                mIsDelayJumpCheck = false;
            }
        }
        else
        {
            if (mPlayerController.GetComponent<Rigidbody>().velocity.y < -0.1f || mPlayerController.mPlayerGroundChecker.bIsGrounded)
            {
                mPlayerController.SetPlayerState(PlayerState.Fall);
            }
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}