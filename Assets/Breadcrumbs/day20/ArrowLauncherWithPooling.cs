using System.Collections.Generic;
using Breadcrumbs.Common;
using UnityEngine;

namespace Breadcrumbs.day20 {
    public class ArrowLauncherWithPooling : MonoBehaviour {
        public GameObject arrowPrefab;
        public Transform firePoint;
        public float arrowSpeed = 20f;
        public int poolSize = 20;

        private List<GameObject> arrowPool;
        private int currentIndex = 0;

        [SerializeField] private bool autoFire;
        
        [SerializeField]
        [Range(0.05f, 1f)]
        private float launchInterval = 0.2f;
        [SerializeField] private float drag = 0.01f;
        private float _fireInterval = 0.2f;
        
        private Launcher _launcher;

        void Start() {
            _launcher = GetComponent<Launcher>();
            arrowPool = new List<GameObject>();
            for (int i = 0; i < poolSize; i++) {
                GameObject arrow = Instantiate(arrowPrefab);
                arrow.SetActive(false);
                // Arrow 스크립트에 launcher 참조 추가
                arrow.GetComponent<Arrow>().launcher = this;
                arrowPool.Add(arrow);
            }
        }

        void Update() {
            if (Input.GetMouseButton(0) || autoFire) {
                _fireInterval += Time.deltaTime;
                if (_fireInterval < launchInterval)
                    return;
                _fireInterval -= launchInterval;
                LaunchArrow();
            }
            else {
                _fireInterval = 0f;
            }
        }

        void LaunchArrow() {
            GameObject arrow = GetPooledArrow();
            if (arrow != null) {
                var launcherInfo = _launcher != null ? 
                    _launcher.GetLauncherInfo() : 
                    (firePoint.position, firePoint.rotation, firePoint.forward);
                arrow.transform.position = launcherInfo.position;
                arrow.transform.rotation = launcherInfo.rotation;

                /*
                Rigidbody rb = arrow.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = (firePoint.forward + firePoint.up * 0.1f) * arrowSpeed;
                // */

                arrow.SetActive(true);
                // 화살 활성화 시 타이머 시작
                arrow.GetComponent<Arrow>().Activate(arrowSpeed, drag, launcherInfo.forward);
            }
        }

        GameObject GetPooledArrow() {
            for (int i = 0; i < poolSize; i++) {
                int index = (currentIndex + i) % poolSize;
                if (!arrowPool[index].activeInHierarchy) {
                    currentIndex = (index + 1) % poolSize;
                    return arrowPool[index];
                }
            }

//            Debug.LogWarning("사용 가능한 화살이 없습니다. 풀 크기를 늘리세요!");
            return null;
        }

        public void DisableArrow(GameObject arrow) {
            arrow.SetActive(false);
        }
    }
}