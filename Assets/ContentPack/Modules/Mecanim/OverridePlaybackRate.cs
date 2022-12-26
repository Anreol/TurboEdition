using UnityEngine;

namespace TurboEdition.Mecanim
{
    public class OverridePlaybackRate : StateMachineBehaviour
    {
        [Tooltip("Seconds to pass before it overrides.")]
        public float stopwatchUntilOverride;

        [Tooltip("Amount of seconds to override the playback rate by.")]
        public float overrideDuration;

        [Tooltip("The name of the parameter that we will be modifying for the duration that will be set once stop duration is up.")]
        public string playbackParameterName;

        [Tooltip("The trigger that will be set once stop duration is up.")]
        public string playbackRateParameterName;

        [Tooltip("The trigger that will be set once stop duration is up.")]
        public string timerUpTriggerParameterName;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.StopPlayback();
        }
    }
}