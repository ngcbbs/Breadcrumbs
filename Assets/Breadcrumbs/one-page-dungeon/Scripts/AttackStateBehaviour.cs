using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    public class AttackStateBehaviour : StateMachineBehaviour {
        private static readonly int AttackCurve = Animator.StringToHash("AttackCurve");

        private AttackHitbox _hitbox;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _hitbox = animator.GetComponentInChildren<AttackHitbox>();
            if (_hitbox == null)
                Debug.LogWarning("AttackStateBehaviour: can't find AttackHitbox component.");
            else
                _hitbox.ResetHitTargets();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_hitbox == null)
                return;
            var curve = animator.GetFloat(AttackCurve);
            if (curve > 0.5f) {
                _hitbox.CheckCollision();
            }
        }
    }
}