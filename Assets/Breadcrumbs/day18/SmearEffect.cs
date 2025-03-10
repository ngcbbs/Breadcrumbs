using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.day18 {
    public class SmearEffect : MonoBehaviour {
        public Material material;
        public GameObject smearPrefab; // 잔상으로 사용할 프리팹
        public int maxSmearCount = 5; // 최대 잔상 개수
        public float smearLifetime = 0.2f; // 잔상이 유지되는 시간
        public float smearInterval = 0.05f; // 잔상 생성 간격

        private Queue<GameObject> smearPool = new Queue<GameObject>(); // 오브젝트 풀
        private float timer = 0f;
        
        private float lastRotationZ;

        void Start() {
            lastRotationZ = transform.eulerAngles.z;
            // 미리 오브젝트 풀을 생성하여 최적화
            for (int i = 0; i < maxSmearCount; i++) {
                GameObject smear = Instantiate(smearPrefab);
                smear.GetComponent<SpriteRenderer>().material = Instantiate(material);
                smear.SetActive(false);
                smearPool.Enqueue(smear);
            }
        }

        void Update() {
            timer += Time.deltaTime;

            if (timer >= smearInterval) {
                timer = 0f;
                float rotation = transform.eulerAngles.z;
                CreateSmear(rotation - lastRotationZ);
                lastRotationZ = rotation;
            }
        }

        void CreateSmear(float rotation) {
            GameObject smear;

            if (smearPool.Count > 0) {
                smear = smearPool.Dequeue();
            }
            else {
                smear = Instantiate(smearPrefab);
            }
            
            SpriteRenderer mainRenderer = GetComponent<SpriteRenderer>();;
            SpriteRenderer smearRenderer = smear.GetComponent<SpriteRenderer>();

            smear.transform.position = transform.position;
            smear.transform.rotation = transform.rotation;
            smear.SetActive(true);

            // 현재 스프라이트 정보 복사
            smearRenderer.sprite = mainRenderer.sprite;
            smearRenderer.color = new Color(1f, 1f, 1f, 0.5f); // 초기에 반투명
            smearRenderer.sortingOrder = mainRenderer.sortingOrder - 1; // 캐릭터보다 뒤쪽에 렌더링
            
            smearRenderer.material.SetFloat("_Strength", Mathf.Cos(1f));
            smearRenderer.material.SetFloat("_Alpha", 1f);

            // 잔상 서서히 사라지는 코루틴 실행
            StartCoroutine(FadeSmear(smear));
        }

        IEnumerator FadeSmear(GameObject smear) {
            SpriteRenderer renderer = smear.GetComponent<SpriteRenderer>();
            float elapsedTime = 0f;

            while (elapsedTime < smearLifetime) {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0.5f, 0f, elapsedTime / smearLifetime); // 점점 투명해짐
                renderer.color = new Color(1f, 1f, 1f, alpha);
                renderer.material.SetFloat("_Strength", Mathf.Cos(alpha));
                renderer.material.SetFloat("_Alpha", alpha);
                yield return null;
            }

            smear.SetActive(false);
            smearPool.Enqueue(smear); // 다시 풀에 추가
        }
    }
}
