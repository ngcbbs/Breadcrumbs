using UnityEngine;

namespace Breadcrumbs.Day13 {
    public class day13 : MonoBehaviour {
        public GameObject player;

        private SensorComponent sensor;

        void Start() {
            sensor = player.GetComponent<SensorComponent>();
        }

        // Update is called once per frame
        void Update() {
            if (sensor == null)
                return;
            if (Input.GetKeyDown(KeyCode.Space)) {
                Debug.Log("Yop!");
                sensor
                    .WithinDistance(10f)
                    //.WithinDirection(90f, 45f)
                    //.WithinBox(new Vector3(10f, 10f, 10f))
                    //.OnLayer(LayerMask.GetMask("Default"))
                    .WithTag("Enemy")
                    //.NoObstacles()
                    .Detect();
                var detectedObjects = sensor.SelectMany();
                foreach (var obj in detectedObjects) {
                    Debug.Log(obj.name);
                }
            }
        }
    }
}