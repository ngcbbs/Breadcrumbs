using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Breadcrumbs.Common {
    public class Spawner : MonoBehaviour {
        [Header("Setting")]
        [SerializeField] public SpawnSetting spawnSetting;
        [SerializeField] private GameObject[] prefabs;
        [SerializeField] private int maxSpawnCount = 10;
        [SerializeField] private float spawnInterval = 3f;
        [Header("Debug")]
        [SerializeField] public bool showDebug;
        
        private readonly List<GameObject> _spawnedUnits = new ();
        private int _currentSpawnCount = 0; // 현재 스폰된 유닛 수

        private void Start() {
            StartCoroutine(SpawnRoutine());
        }
        
        private IEnumerator SpawnRoutine() {
            while (true) {
                yield return new WaitForSeconds(spawnInterval);

                // 현재 스폰된 유닛 수가 최대 스폰 수보다 적을 때만 스폰
                if (_currentSpawnCount < maxSpawnCount) {
                    SpawnUnit();
                }

                // 죽은 유닛 리스트에서 제거
                CleanupDeadUnits();
            }
        }
        
        private void SpawnUnit() {
            if (prefabs.Length == 0) {
                Debug.LogWarning("스폰할 유닛 프리팹이 등록되지 않았습니다.");
                return;
            }

            var randomPosition = GetRandomSpawnPosition();
            var selectedPrefab = prefabs[Random.Range(0, prefabs.Length)];
            var spawnedUnit = Instantiate(selectedPrefab, randomPosition, Quaternion.identity);
            _spawnedUnits.Add(spawnedUnit);
            _currentSpawnCount++;

            var unit = spawnedUnit.GetComponent<Unit>();
            if (unit != null) {
                unit.OnUnitDied += HandleUnitDied;
                
                // random direction
                unit.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                // :(
                var autoDeath = unit.GetComponent<AutoDeath>();
                if (autoDeath == null)
                    unit.AddComponent<AutoDeath>();
            }
        }

        private void HandleUnitDied(Unit unit) {
            if (_spawnedUnits.Contains(unit.gameObject)) {
                _currentSpawnCount--;
                _spawnedUnits.Remove(unit.gameObject);
            }
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

        private void CleanupDeadUnits() {
            for (int i = _spawnedUnits.Count - 1; i >= 0; i--) {
                if (_spawnedUnits[i] == null) {
                    _spawnedUnits.RemoveAt(i);
                    _currentSpawnCount--;
                }
            }
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
