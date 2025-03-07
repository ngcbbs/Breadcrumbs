using UnityEngine;

namespace Breadcrumbs.day16 {
    public class SpinObject : MonoBehaviour {
        public Vector3 rotation = new Vector3(0, 30, 0); // 초당 회전 각도
        public float speed = 1;

        void Update() {
            transform.Rotate(rotation * (speed * Time.deltaTime));
        }
    }
}
