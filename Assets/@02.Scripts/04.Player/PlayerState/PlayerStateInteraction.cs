using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateInteraction : IPlayerState
{
    private PlayerController mPlayerController;
    private Vector3 mInteractablePosition;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Interaction", true);

        InteractableObject interactable = mPlayerController.NearestInteractableObject;
        if (interactable != null)
        {
            mInteractablePosition = mPlayerController.NearestInteractableObject.transform.position;
            Vector3 directionToInteractable = mInteractablePosition - mPlayerController.transform.position;
            directionToInteractable.y = 0;
            mPlayerController.transform.rotation = Quaternion.LookRotation(directionToInteractable);
            
            UIPanelManager.Instance.OpenPanelWithPlayer(interactable.GetPanelType(), playerController);
        }
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Interaction", false);
        mPlayerController = null;
    }
}
