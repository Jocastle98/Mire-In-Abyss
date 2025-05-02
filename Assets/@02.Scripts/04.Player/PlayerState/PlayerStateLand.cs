using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class PlayerStateLand : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }
        
        mPlayerController.Land();
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}