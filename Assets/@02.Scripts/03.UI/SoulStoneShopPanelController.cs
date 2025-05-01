using System;
using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;
using UnityEngine.UI;

public class SoulStoneShopPanelController : PopupPanelController
{
    public void OnClickCloseButton()
    {
        PlayerController mPlayerController = FindObjectOfType<PlayerController>();
        Hide(() =>
        {
            mPlayerController.SetPlayerState(PlayerState.Idle);
        });
    }
}
