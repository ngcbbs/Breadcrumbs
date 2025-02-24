using UnityEngine;

namespace day5_scrap {
    public class HitEffectManager : MonoBehaviour
    {
        public ParticleSystem hitEffect;
        public float hitStopDuration = 0.05f;
        public float knockbackForce = 10f;

        public void OnHit(Vector3 hitPoint, Vector3 hitNormal)
        {
            // 히트 이펙트 재생
            ParticleSystem effect = Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(effect.gameObject, 2f);

            // 히트스톱 효과
            StartCoroutine(HitStop());
        }

        private System.Collections.IEnumerator HitStop()
        {
            Time.timeScale = 0.1f;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            Time.timeScale = 1f;
        }
    }
}
