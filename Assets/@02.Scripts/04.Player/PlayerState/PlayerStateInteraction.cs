using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlayerEnums;
using UIPanelEnums;
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

            UIManager.Instance.Push(interactable.GetPanelType(), mPlayerController);
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
