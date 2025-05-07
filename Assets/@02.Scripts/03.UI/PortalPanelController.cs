using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalPanelController : PopupPanelController
{
    [SerializeField] private string targetSceneName = "BattleArea_Hong";
    
    public void OnClickCloseButton()
    {
        Hide(() =>
        {
            mPlayer.SetPlayerState(PlayerState.Idle);
        });
    }

    public void OnClickMoveButton()
    {
        Hide(() =>
        {
            LoadTargetScene();
        });
    }

    private void LoadTargetScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}
