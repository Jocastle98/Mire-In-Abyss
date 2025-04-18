using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerAnimatorStateDash : StateMachineBehaviour
{
    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController playerController = animator.gameObject.GetComponent<PlayerController>();
        
        if (playerController.mPlayerGroundChecker.bIsGrounded)
        {
            playerController.SetPlayerState(PlayerState.Idle);
        }
        else
        {
            playerController.SetPlayerState(PlayerState.Fall);
        }
    }
}
