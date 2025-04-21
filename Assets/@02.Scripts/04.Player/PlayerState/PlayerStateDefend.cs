using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateDefend : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mCameraForward;
    public bool bIsDefending { get; set; }
    public bool bIsDefendEnd { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Defend", true);
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
