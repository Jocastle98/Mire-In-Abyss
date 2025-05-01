using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PortalPanelController : PopupPanelController
{
    
    
    public void OnClickCloseButton()
    {
        Hide(() =>
        {
            mPlayer.SetPlayerState(PlayerState.Idle);
        });
    }
}
