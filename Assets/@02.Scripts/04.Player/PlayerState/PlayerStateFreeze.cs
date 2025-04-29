using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFreeze : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        // 빙결 관련 초기화(현재 없음)
    }

    public void OnUpdate()
    {
        // 아무것도 조작도 못하는 상태, 비워두면 될듯
    }

    public void OnExit()
    {
        // 빙결 상태에서 벗어나기 전 필요한 처리(현재 없음)
        mPlayerController = null;
    }
}