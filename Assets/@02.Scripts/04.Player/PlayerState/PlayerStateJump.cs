using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateJump : MonoBehaviour, IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Jump");
        mPlayerController.Jump();
    }

    public void OnUpdate()
    {
        mPlayerController.PlayerAnimator.SetFloat("GroundDistance", mPlayerController.GetDistanceToGround());
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
