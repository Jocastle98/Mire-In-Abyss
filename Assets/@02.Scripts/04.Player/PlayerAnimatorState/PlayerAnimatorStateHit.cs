using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorStateHit : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        int upperBodyLayer = animator.GetLayerIndex("UpperBody Layer");
        animator.GetComponent<Animator>().SetLayerWeight(upperBodyLayer, 0.0f);
    }
}
