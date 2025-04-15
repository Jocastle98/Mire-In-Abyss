using System.Collections;
using System.Collections.Generic;
using PlayerEnums;
using UnityEngine;

public class PlayerAnimatorStateAttack : StateMachineBehaviour
{
    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController playerController = animator.GetComponent<PlayerController>();
        
        if (!GameManager.Instance.Input.AttackInput && !playerController.CheckEnableCombo())
        {
            playerController.SetPlayerStateDelayed(PlayerState.Idle, 0.3f);
        }
    }
}
