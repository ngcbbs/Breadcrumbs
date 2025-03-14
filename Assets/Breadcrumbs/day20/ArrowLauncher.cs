using Breadcrumbs.Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace Breadcrumbs.day20 {
    public class ArrowLauncher : MonoBehaviour {
        public Transform firePoint;
        public float arrowSpeed = 20f;

        [SerializeField] private bool autoFire;
        
        [SerializeField]
        [Range(0.05f, 1f)]
        private float launchInterval = 0.2f;
        [SerializeField] private float drag = 0.01f;
        private float _fireInterval = 0.2f;
        
        private Launcher _launcher;

        void Start() {
            _launcher = GetComponent<Launcher>();
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
            var launcherInfo = _launcher != null ? 
                _launcher.GetLauncherInfo() : 
                (firePoint.position, firePoint.rotation, firePoint.forward);

            var arrowItem = ObjectPoolManager.Instance.Get<Arrow>(
                launcherInfo.position,
                launcherInfo.rotation,
                null
            );

            if (arrowItem != null) {
                arrowItem.Activate(arrowSpeed, drag, launcherInfo.forward);
            }
        }
    }
}