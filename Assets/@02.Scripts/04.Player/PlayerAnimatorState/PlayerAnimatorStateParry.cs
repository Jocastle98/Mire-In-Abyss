using UnityEngine;
using PlayerEnums;

public class PlayerAnimatorStateParry : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<PlayerController>().SetPlayerState(PlayerState.Idle);
    }
}
