using UnityEngine;

namespace Breadcrumbs.day18 {
    [ExecuteAlways]
    public class Rotate : MonoBehaviour {
        [SerializeField] private float rotateSpeed = 1f;
        [SerializeField] private float distance = 2f;
        [SerializeField] private bool test;

        private float angle = 0f;
        private float t = 0f;

        void Update()
        {
            t += Time.deltaTime;
            angle += Time.deltaTime * 90f * rotateSpeed;
            if (test)
                angle += Mathf.Sin(angle) * 32;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            transform.localPosition = new Vector3(Mathf.Sin(t) * distance, Mathf.Cos(t) * distance, 0f);
        }
    }
}
