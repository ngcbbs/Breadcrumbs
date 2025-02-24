using System.Collections;
using UnityEngine;

namespace day5_scrap {
    public class DamageReceiver : MonoBehaviour {
        [Header("Health Settings")] [SerializeField]
        private float maxHealth = 100f;

        [SerializeField] private float currentHealth;

        [Header("Defense Settings")] [SerializeField]
        private float defense = 10f;

        [SerializeField] private float invincibilityDuration = 0.5f;

        [Header("Hit Feedback")] [SerializeField]
        private float hitStunDuration = 0.2f;

        [SerializeField] private float knockbackForce = 5f;

        public bool isInvincible { get; set; }
        private bool isDead;
        private CharacterEffects characterEffects;
        private ActionCharacterController characterController;

        // 이벤트 선언
        public delegate void HealthChangedHandler(float currentHealth, float maxHealth);

        public event HealthChangedHandler OnHealthChanged;

        public delegate void CharacterDeathHandler();

        public event CharacterDeathHandler OnDeath;

        private void Awake() {
            currentHealth = maxHealth;
            characterEffects = GetComponent<CharacterEffects>();
            characterController = GetComponent<ActionCharacterController>();
        }

        public void TakeDamage(DamageInfo damageInfo) {
            if (isInvincible || isDead) return;

            // 데미지 계산
            float finalDamage = CalculateDamage(damageInfo);

            // 체력 감소
            currentHealth -= finalDamage;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // 히트 이펙트 재생
            if (characterEffects != null) {
                characterEffects.PlayHitEffect(damageInfo.hitPoint, damageInfo.hitNormal);
            }

            // 히트 스턴 & 넉백
            if (finalDamage > 0) {
                StartCoroutine(HitStunCoroutine(damageInfo));
            }

            // 사망 처리
            if (currentHealth <= 0 && !isDead) {
                Die();
            }
            else {
                StartCoroutine(InvincibilityCoroutine());
            }
        }

        private float CalculateDamage(DamageInfo damageInfo) {
            float finalDamage = damageInfo.damage;

            // 방어력 적용
            finalDamage = Mathf.Max(0, finalDamage - defense);

            // 크리티컬 히트 처리
            if (damageInfo.isCritical) {
                finalDamage *= damageInfo.criticalMultiplier;
            }

            return finalDamage;
        }

        private IEnumerator HitStunCoroutine(DamageInfo damageInfo) {
            // 캐릭터 컨트롤러 일시 정지
            if (characterController != null) {
                characterController.enabled = false;
            }

            // 넉백 적용
            if (GetComponent<Rigidbody>() is Rigidbody rb) {
                Vector3 knockbackDirection = (transform.position - damageInfo.attackerPosition).normalized;
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(hitStunDuration);

            // 컨트롤러 재활성화
            if (characterController != null) {
                characterController.enabled = true;
            }
        }

        private IEnumerator InvincibilityCoroutine() {
            isInvincible = true;
            yield return new WaitForSeconds(invincibilityDuration);
            isInvincible = false;
        }

        private void Die() {
            isDead = true;
            OnDeath?.Invoke();

            // 사망 처리 로직
            if (characterController != null) {
                characterController.enabled = false;
            }

            // 사망 애니메이션 & 이펙트
            if (TryGetComponent<Animator>(out var animator)) {
                animator.SetTrigger("Die");
            }
        }

        public void Heal(float amount) {
            if (isDead) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    // 데미지 정보를 담는 구조체
    public struct DamageInfo
    {
        public float damage;
        public Vector3 hitPoint;
        public Vector3 hitNormal;
        public Vector3 attackerPosition;
        public bool isCritical;
        public float criticalMultiplier;
    
        public DamageInfo(float damage, Vector3 hitPoint, Vector3 hitNormal, Vector3 attackerPosition, 
            bool isCritical = false, float criticalMultiplier = 2f)
        {
            this.damage = damage;
            this.hitPoint = hitPoint;
            this.hitNormal = hitNormal;
            this.attackerPosition = attackerPosition;
            this.isCritical = isCritical;
            this.criticalMultiplier = criticalMultiplier;
        }
    }
}