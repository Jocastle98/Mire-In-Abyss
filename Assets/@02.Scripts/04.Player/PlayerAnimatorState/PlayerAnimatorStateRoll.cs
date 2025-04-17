using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimatorStateRoll : StateMachineBehaviour
{
    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<PlayerController>().SetPlayerState(PlayerState.Idle);
    }
}
