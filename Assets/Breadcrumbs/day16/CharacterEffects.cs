using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Breadcrumbs.day16 {
    public class CharacterEffects : MonoBehaviour {
        [Header("Dash Effects")] [SerializeField]
        private VisualEffect dashVFX;

        [SerializeField] private TrailRenderer dashTrail;
        [SerializeField] private float trailTime = 0.5f;
        [SerializeField] private ParticleSystem dashStartParticles;
        [SerializeField] private ParticleSystem dashEndParticles;

        [Header("Hit Effects")] [SerializeField]
        private ParticleSystem hitParticles;

        [SerializeField] private VisualEffect hitVFX;
        [SerializeField] private float hitEffectDuration = 0.5f;

        [Header("Material Effects")] [SerializeField]
        private Material characterMaterial;

        [SerializeField] private Color dashColor = Color.blue;
        [SerializeField] private float dashEmissionIntensity = 2f;

        private Color originalEmissionColor;
        private float originalEmissionIntensity;
        private List<Vector3> dashTrailPositions;
        private bool isDashing;

        private void Awake() {
            dashTrailPositions = new List<Vector3>();

            if (characterMaterial != null) {
                originalEmissionColor = characterMaterial.GetColor("_EmissionColor");
                originalEmissionIntensity = characterMaterial.GetFloat("_EmissionIntensity");
            }

            // 초기 상태 설정
            if (dashTrail != null) {
                dashTrail.enabled = false;
            }
        }

        public void PlayDashEffect() {
            isDashing = true;

            // VFX 시작
            if (dashVFX != null) {
                dashVFX.Play();
            }

            // 트레일 활성화
            if (dashTrail != null) {
                dashTrail.enabled = true;
                dashTrail.Clear();
            }

            // 시작 파티클
            if (dashStartParticles != null) {
                dashStartParticles.Play();
            }

            // 머티리얼 이펙트
            if (characterMaterial != null) {
                characterMaterial.SetColor("_EmissionColor", dashColor * dashEmissionIntensity);
                characterMaterial.EnableKeyword("_EMISSION");
            }

            dashTrailPositions.Clear();
            StartCoroutine(DashEffectCoroutine());
        }

        public void UpdateDashTrail(Vector3 position) {
            if (!isDashing) return;

            dashTrailPositions.Add(position);

            if (dashVFX != null) {
                dashVFX.SetVector3("Position", position);
            }
        }

        public void StopDashEffect() {
            isDashing = false;

            // VFX 정지
            if (dashVFX != null) {
                dashVFX.Stop();
            }

            // 트레일 비활성화
            if (dashTrail != null) {
                StartCoroutine(FadeTrail());
            }

            // 종료 파티클
            if (dashEndParticles != null) {
                dashEndParticles.Play();
            }

            // 머티리얼 복구
            if (characterMaterial != null) {
                characterMaterial.SetColor("_EmissionColor", originalEmissionColor);
                if (originalEmissionIntensity <= 0) {
                    characterMaterial.DisableKeyword("_EMISSION");
                }
            }
        }

        public void PlayHitEffect(Vector3 hitPoint, Vector3 hitNormal) {
            // 히트 파티클 재생
            if (hitParticles != null) {
                ParticleSystem hitInstance = Instantiate(hitParticles, hitPoint, Quaternion.LookRotation(hitNormal));
                Destroy(hitInstance.gameObject, hitEffectDuration);
            }

            // 히트 VFX 재생
            if (hitVFX != null) {
                VisualEffect hitInstance = Instantiate(hitVFX, hitPoint, Quaternion.LookRotation(hitNormal));
                hitInstance.Play();
                Destroy(hitInstance.gameObject, hitEffectDuration);
            }

            // 머티리얼 히트 효과
            StartCoroutine(HitFlashEffect());
        }

        private IEnumerator DashEffectCoroutine() {
            while (isDashing) {
                if (dashVFX != null) {
                    dashVFX.SetVector3("Velocity", GetComponent<Rigidbody>()?.linearVelocity ?? Vector3.zero);
                }

                yield return null;
            }
        }

        private IEnumerator FadeTrail() {
            if (dashTrail != null) {
                float elapsedTime = 0f;
                while (elapsedTime < trailTime) {
                    elapsedTime += Time.deltaTime;
                    dashTrail.time = Mathf.Lerp(trailTime, 0f, elapsedTime / trailTime);
                    yield return null;
                }

                dashTrail.enabled = false;
            }
        }

        private IEnumerator HitFlashEffect() {
            if (characterMaterial != null) {
                Color hitColor = Color.white;
                characterMaterial.SetColor("_EmissionColor", hitColor * 2f);
                characterMaterial.EnableKeyword("_EMISSION");

                yield return new WaitForSeconds(0.1f);

                characterMaterial.SetColor("_EmissionColor", originalEmissionColor);
                if (originalEmissionIntensity <= 0) {
                    characterMaterial.DisableKeyword("_EMISSION");
                }
            }
        }
    }
}
