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
                mPlayerController?.Idle();
            }
            else
            {
                mPlayerController?.SetPlayerState(PlayerState.Move);
            }

            if (GameManager.Instance.Input.JumpInput)
            {
                mPlayerController?.SetPlayerState(PlayerState.Jump);
            }

            if (GameManager.Instance.Input.RollInput)
            {
                mPlayerController?.SetPlayerState(PlayerState.Roll);
            }

            if (GameManager.Instance.Input.DefendInput)
            {
                mPlayerController?.SetPlayerState(PlayerState.Defend);
            }

            if (GameManager.Instance.Input.ParryInput)
            {
                mPlayerController?.SetPlayerState(PlayerState.Parry);
            }

            if (mPlayerController?.NearestInteractableObject != null)
            {
                if (GameManager.Instance.Input.InteractionInput)
                {
                    mPlayerController?.SetPlayerState(PlayerState.Interaction);
                    return;
                }
            }
        }

        if (GameManager.Instance.Input.AttackInput)
        {
            mPlayerController?.SetPlayerState(PlayerState.Attack);
        }

        if (GameManager.Instance.Input.DashInput)
        {
            mPlayerController?.SetPlayerState(PlayerState.Dash);
        }
    }

    public void OnExit()
    {
        mPlayerController.PlayerAnimator.SetBool("Idle", false);
        mPlayerController = null;
    }
}