using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerStateDefend : IPlayerState
{
    private PlayerController mPlayerController;
    public bool bIsDefending { get; set; }
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;

        // 애니메이션 회전 보정
        DefendRotate(45.0f);
        
        mPlayerController.PlayerAnimator.SetBool("Defend", true);
        bIsDefending = true;
    }

    public void OnUpdate()
    {
        if (!GameManager.Instance.Input.IsDefending)
        {
            // 애니메이션 회전 보정
            DefendRotate(-45.0f);
            mPlayerController.SetPlayerState(PlayerState.Idle);
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Defend", false);
        bIsDefending = false;
        mPlayerController = null;
    }

    // 방패 막기 애니메이션이 좌측으로 45도가 돌아감 -> 우측으로 45도 돌려줘야 함
    private void DefendRotate(float angle)
    {
        // 현재 방향
        Vector3 playerForward = mPlayerController.transform.forward;
        playerForward.y = 0f;
        playerForward.Normalize();

        // 45도 우측으로 회전한 방향 만들기
        Vector3 rotatedForward = Quaternion.Euler(0.0f, angle, 0.0f) * playerForward;
        mPlayerController.transform.rotation = Quaternion.LookRotation(rotatedForward);
    }
}
