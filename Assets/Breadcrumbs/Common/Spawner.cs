using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Breadcrumbs.Common {
    public class Spawner : MonoBehaviour {
        [Header("Setting")]
        [SerializeField] public SpawnSetting spawnSetting;
        [SerializeField] private GameObject[] prefabs; // 음...
        [SerializeField] private int maxSpawnCount = 10;
        [SerializeField] private float spawnInterval = 3f;
        [Header("Debug")]
        [SerializeField] public bool showDebug;
        
        // ObjectPoolManager 에서 사용 가능한 프리팹만.. (인스팩터에 등록 할때 체크 하는편이..)
        private void CheckPrefabs() {
            var usePrefabs = new List<GameObject>();
            foreach (var prefab in prefabs) {
                var components = prefab.GetComponents<Component>();
                if (components.Any(x => x is IPoolable) == false) {
                    Debug.LogWarning($"Prefab({prefab.name}) 오브젝트 풀에서 사용 가능한 프리팹이 아님.");
                    continue;
                }

                var type = components.FirstOrDefault(x => x is IPoolable);
                if (type == null)
                    continue;
                usePrefabs.Add(prefab);
            }

            prefabs = usePrefabs.ToArray();
        }

        private void Start() {
            CheckPrefabs();
            StartCoroutine(SpawnRoutine());
        }
        
        private IEnumerator SpawnRoutine() {
            while (true) {
                yield return new WaitForSeconds(spawnInterval);

                // 현재 스폰된 유닛 수가 최대 스폰 수보다 적을 때만 스폰
                var unitCount = ObjectPoolManager.Instance.Count<Unit>();
                if (ObjectPoolManager.Instance.Count<Unit>() < maxSpawnCount) {
                    SpawnUnit();
                }
            }
        }
        
#if UNITY_EDITOR
        private void OnGUI() {
            var unitCount = ObjectPoolManager.Instance.Count<Unit>();
            GUI.Label(new Rect(10, 10, 200, 40), $"{unitCount}/{maxSpawnCount}");
        }
#endif
        
        private void SpawnUnit() {
            if (prefabs.Length == 0) {
                Debug.LogWarning("스폰할 유닛 프리팹이 등록되지 않았습니다.");
                return;
            }

            var randomPosition = GetRandomSpawnPosition();
            //var selectedPrefab = prefabs[Random.Range(0, prefabs.Length)]; // todo: fixme
            // note: 타입으로 풀링 오브젝트를 특정하니 약간의 불편함이..
            var unit = ObjectPoolManager.Instance.Get<Unit>(
                randomPosition, Quaternion.identity
            );

            if (unit == null)
                return;
            
            unit.Initialize();
            
            // random direction
            unit.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            var autoDeath = unit.GetOrAdd<AutoDeath>();
            autoDeath.Initialize();
        }

        private Vector3 GetRandomSpawnPosition() {
            var center = transform.position;
            var size = spawnSetting.size;
            switch (spawnSetting.type) {
                case SpawnTypes.Box:
                    var randomX = Random.Range(-size.x / 2f, size.x / 2f);
                    var randomY = Random.Range(-size.y / 2f, size.y / 2f);
                    var randomZ = Random.Range(-size.z / 2f, size.z / 2f);
                    return center + new Vector3(randomX, randomY, randomZ);
                case SpawnTypes.Circle:
                    var randomCircle = Random.insideUnitCircle * new Vector2(size.x, size.z);
                    return center + new Vector3(randomCircle.x, 0, randomCircle.y);
                case SpawnTypes.Sphere:
                    return center + Vector3.Scale(Random.insideUnitSphere, size);
                case SpawnTypes.Point:
                    return center;
            }

            return Vector3.zero;
        }
        
        private void OnDrawGizmos() {
            if (showDebug == false || spawnSetting == null)
                return;
            
            var center = transform.position;
            var size = spawnSetting.size;

            switch (spawnSetting.type) {
                case SpawnTypes.Box:
                    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                    Gizmos.DrawCube(center, size);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(center, size);
                    break;
                
                case SpawnTypes.Circle:
                    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                    var lines = new List<Vector3>();
                    var radius = 360f / 32f * Mathf.Deg2Rad;
                    for (int i = 0; i < 32; ++i) {
                        lines.Add(new Vector3(
                            center.x + Mathf.Sin(radius * i) * size.x,
                            center.y,
                            center.z + Mathf.Cos(radius * i) * size.z
                        ));
                    }

                    Gizmos.DrawLineStrip(lines.ToArray(), true);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLineStrip(lines.ToArray(), true);
                    break;
                
                case SpawnTypes.Sphere:
                    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                    Gizmos.DrawSphere(center, size.x);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(center, size.x);
                    break;
                
                case SpawnTypes.Point:
                    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                    Gizmos.DrawCube(center, Vector3.one * 0.1f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(center, Vector3.one * 0.1f);
                    break;
            }
        }
    }
}
