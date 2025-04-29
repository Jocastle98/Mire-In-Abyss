using UnityEngine;
using PlayerEnums;

public class PlayerAnimatorStateSkill : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController playerController = animator.gameObject.GetComponent<PlayerController>();
        
        if (GameManager.Instance.Input.MoveInput == Vector2.zero)
        {
            playerController.SetPlayerState(PlayerState.Idle);
        }
        else
        {
            playerController.SetPlayerState(PlayerState.Move);
        }
    }
}
