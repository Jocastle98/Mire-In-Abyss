using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
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
        if (mPlayerController.ActionCheck())
        {
            mPlayerController.SetPlayerState(PlayerState.Fall);
        }
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
