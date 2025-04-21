using PlayerEnums;
using UnityEngine;

public class PlayerStateIdle : IPlayerState
{
    private PlayerController mPlayerController;
    
    public void OnEnter(PlayerController playerController)
    {
        mPlayerController = playerController;
        mPlayerController.PlayerAnimator.SetBool("Idle", true);
    }

    public void OnUpdate()
    {
        if (mPlayerController == null)
        {
            return;
        }

        if (mPlayerController.IsGrounded)
        {
            if (GameManager.Instance.Input.MoveInput == Vector2.zero)
            {
                mPlayerController.Idle();
            }
            else
            {
                mPlayerController.SetPlayerState(PlayerState.Move);
            }

            if (GameManager.Instance.Input.JumpInput)
            {
                mPlayerController.SetPlayerState(PlayerState.Jump);
            }
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Idle", false);
        mPlayerController = null;
    }
}