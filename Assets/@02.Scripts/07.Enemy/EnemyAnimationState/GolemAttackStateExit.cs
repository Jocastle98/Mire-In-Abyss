using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemAttackStateExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<GolemBTController>().OnAttackAnimationExit();
    }
}
