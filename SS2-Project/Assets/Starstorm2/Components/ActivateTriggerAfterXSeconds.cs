using UnityEngine;

public class ActivateTriggerAfterXSeconds : StateMachineBehaviour
{
    public string triggerParameterName;
    public float desiredTriggerTime;

    private float timer;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;
        if (timer >= desiredTriggerTime)
        {
            animator.SetTrigger(triggerParameterName);
            timer = 0f;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
    }
}
