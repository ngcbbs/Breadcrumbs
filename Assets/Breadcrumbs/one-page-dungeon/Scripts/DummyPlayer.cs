using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    public class DummyPlayer : MonoBehaviour, IDamageable {
        public float knockbackScale = 0.1f;
        public void OnDamage(int damage, Vector3 hitDirection) {
            /*
            // 피격 리액션 처리를 위함 스크립트
            // * Animator 파라미터 설정
            //  > HitDirectionX : float
            //  > HitDirectionY : float
            //  > Hit : Trigger
            
            Vector3 localHitDir = transform.InverseTransformDirection(hitDirection);

            animator.SetFloat("HitDirectionX", localHitDir.x);
            animator.SetFloat("HitDirectionY", localHitDir.z);

            animator.SetTrigger("Hit");
            // */
            Debug.Log($"{gameObject.name} damaged. (dmg={damage})");
            
            // 넉백 리액션 처리
            var knockback = hitDirection * knockbackScale;
            transform.position += knockback;
            
            // 애니메이션 트리거 또는 상태변경
        }
    }
}
