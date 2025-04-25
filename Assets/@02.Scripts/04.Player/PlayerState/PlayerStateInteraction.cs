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
        
        mInteractablePosition = mPlayerController.NearestInteractableObject.transform.position;
        Vector3 directionToInteractable = mInteractablePosition - mPlayerController.transform.position;
        directionToInteractable.y = 0;
        mPlayerController.transform.rotation = Quaternion.LookRotation(directionToInteractable);
    }

    public void OnUpdate()
    {
        // 임시 기능, 상호작용이 끝나면 대기 상태로 전환 하는 메서드 필요함
        // ex) 상점패널 닫기를 누르면 mPlayerController.SetPlayerState(PlayerState.Idle) 호출
        if (GameManager.Instance.Input.InteractionInput)
        {
            mPlayerController.SetPlayerState(PlayerState.Idle);
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Interaction", false);
        mPlayerController = null;
    }
}
