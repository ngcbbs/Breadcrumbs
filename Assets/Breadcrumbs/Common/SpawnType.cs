using System;
using UnityEditor;
using UnityEngine;

namespace Breadcrumbs.Common {
    public enum SpawnTypes
    {
        None,
        Box,
        Circle,
        Sphere,
        Point,
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Create SpawnSetting", fileName = "SpawnSetting", order = 0)]
    public class SpawnSetting : ScriptableObject {
        public SpawnTypes type;
        public Vector3 size;
    }

    public class SpawnProvider {
        private SpawnSetting _setting;
        private float _time;
        
        public SpawnProvider(SpawnSetting setting) {
            _setting = setting;
            _time = 0f;
        }

        public Vector3 GetPosition() {
            return Vector3.zero;
        }

        public void Update() {
            _time -= Time.deltaTime;
            if (_time < 0f) {
                // Spawn
            }
        }
    }
}
