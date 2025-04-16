using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateParry : IPlayerState
{
    private PlayerController mPlayerController;
    public bool bIsParrying { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetTrigger("Parry");
    }

    public void OnUpdate()
    {
        if (bIsParrying)
        {
            // if (공격 받으면) { 강한 공격으로 반격; }
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
