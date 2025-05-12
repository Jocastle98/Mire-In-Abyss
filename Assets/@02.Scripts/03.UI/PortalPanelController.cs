using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PortalPanelController : BaseUIPanel
{
    [SerializeField] private BackButton mBackButton;
    
    protected override void Awake()
    {
        base.Awake();
        if (mBackButton != null)
        {
            mBackButton.SetAfterCloseAction(() =>
            {
                mPlayer?.SetPlayerState(PlayerState.Idle);
            });
        }
    }

    public async void OnClickMoveButton()
    {
        await UIManager.Instance.Pop(); 
        mPlayer.SetPlayerState(PlayerState.Idle);
        LoadTargetScene();
    }

    private void LoadTargetScene()
    {
        SceneLoader.LoadSceneAsync(Constants.AbyssScene);
    }
}
