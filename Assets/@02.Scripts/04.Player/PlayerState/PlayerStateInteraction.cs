using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateInteraction : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController = null;
    }
}
