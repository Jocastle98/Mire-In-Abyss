using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateStun : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Stun");
        mPlayerController.PlayerAnimator.SetBool("IsStunned", true);
    }

    public void OnUpdate()
    {
        // 아무것도 조작도 못하는 상태, 비워두면 될듯
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("IsStunned", false);
        mPlayerController = null;
    }
}
