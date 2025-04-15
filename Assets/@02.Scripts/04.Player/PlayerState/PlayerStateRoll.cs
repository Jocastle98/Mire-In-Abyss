using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateRoll : IPlayerState
{
    private PlayerController mPlayerController;
    public bool bIsRolling = false;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController?.PlayerAnimator.SetTrigger("Roll");
        mPlayerController?.Roll();
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
