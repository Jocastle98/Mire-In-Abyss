using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackStateAnimation : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var endAttack = animator.GetComponent<EnemyBTController>();
        if (endAttack != null)
            endAttack.OnAttackAnimationExit();
    }
}
