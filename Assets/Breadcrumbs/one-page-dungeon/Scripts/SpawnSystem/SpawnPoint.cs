using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰 포인트 기본 클래스
    /// </summary>
    public class SpawnPoint : MonoBehaviour {
        [Header("기본 설정")]
        public SpawnableObjectType SpawnType;
        public GameObject SpawnPrefab;

        [Header("스폰 조건")]
        public SpawnTriggerType SpawnTrigger = SpawnTriggerType.None;
        public float SpawnDelay = 0f;
        public DifficultyLevel RequiredDifficulty = DifficultyLevel.Beginner;
        public bool respawnAfterDeath = true;
        public float respawnTime = 30f;

        [Header("스폰 속성")]
        public Quaternion InitialRotation = Quaternion.identity;
        public float PositionRandomRange = 0f;
        public Bounds TriggerArea = new Bounds(Vector3.zero, new Vector3(5, 5, 5));

        [Header("스폰 제한")]
        public int maxSpawnCount = 1;
        public bool isActive = true;

        // 내부 상태 변수
        private int _currentSpawnCount = 0;
        private bool _triggerActivated = false;
        private float _lastSpawnTime = 0f;
        private List<GameObject> _spawnedObjects = new List<GameObject>();

        private void OnDrawGizmos() {
            // 에디터에서 스폰 영역 시각화
            Gizmos.color = isActive ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(transform.position + TriggerArea.center, TriggerArea.size);
        }

        /// <summary>
        /// 스폰 포인트가 활성화되었는지 확인합니다.
        /// </summary>
        public bool IsActive() {
            return isActive && _currentSpawnCount < maxSpawnCount;
        }

        /// <summary>
        /// 특정 난이도 조건을 충족하는지 확인합니다.
        /// </summary>
        public bool MeetsDifficultyRequirement(DifficultyLevel currentDifficulty) {
            return (int)currentDifficulty >= (int)RequiredDifficulty;
        }

        /// <summary>
        /// 주어진 위치가 트리거 영역 내에 있는지 확인합니다.
        /// </summary>
        public bool IsInTriggerArea(Vector3 position) {
            return TriggerArea.Contains(position - transform.position);
        }

        /// <summary>
        /// 스폰 프로세스를 시작합니다.
        /// </summary>
        public void TriggerSpawn() {
            if (!_triggerActivated && IsActive()) {
                _triggerActivated = true;

                if (SpawnDelay > 0) {
                    StartCoroutine(SpawnWithDelay());
                } else {
                    SpawnObject();
                }
            }
        }

        /// <summary>
        /// 딜레이 후 스폰을 실행합니다.
        /// </summary>
        private IEnumerator SpawnWithDelay() {
            yield return new WaitForSeconds(SpawnDelay);
            SpawnObject();
            _triggerActivated = false;
        }

        /// <summary>
        /// 오브젝트 스폰을 실행합니다.
        /// </summary>
        private void SpawnObject() {
            Vector3 spawnPosition = transform.position;

            // 랜덤 위치 적용 (설정된 경우)
            if (PositionRandomRange > 0) {
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(-PositionRandomRange, PositionRandomRange),
                    0,
                    UnityEngine.Random.Range(-PositionRandomRange, PositionRandomRange)
                );
                spawnPosition += randomOffset;
            }

            // 오브젝트 풀링 사용 (SpawnManager를 통해)
            GameObject spawnedObject = SpawnManager.Instance.GetObjectFromPool(SpawnPrefab);
            spawnedObject.transform.position = spawnPosition;
            spawnedObject.transform.rotation = InitialRotation;
            spawnedObject.SetActive(true);

            // ISpawnable 인터페이스 호출
            ISpawnable spawnable = spawnedObject.GetComponent<ISpawnable>();
            if (spawnable != null) {
                spawnable.OnSpawned(spawnPosition, InitialRotation);
            }

            _spawnedObjects.Add(spawnedObject);
            _currentSpawnCount++;
            _lastSpawnTime = Time.time;

            // SpawnManager에게 관리 등록
            SpawnManager.Instance.RegisterSpawnedObject(this, spawnedObject);
        }

        /// <summary>
        /// 스폰된 오브젝트 제거를 처리합니다.
        /// </summary>
        public void HandleObjectDespawn(GameObject despawnedObject) {
            if (_spawnedObjects.Contains(despawnedObject)) {
                _spawnedObjects.Remove(despawnedObject);
                _currentSpawnCount--;

                // ISpawnable 인터페이스 호출
                ISpawnable spawnable = despawnedObject.GetComponent<ISpawnable>();
                if (spawnable != null) {
                    spawnable.OnDespawned();
                }

                // 리스폰 설정이 되어 있으면 리스폰 예약
                if (respawnAfterDeath && isActive) {
                    StartCoroutine(RespawnAfterDelay(respawnTime));
                }

                // 오브젝트를 비활성화하고 풀에 반환
                despawnedObject.SetActive(false);
                SpawnManager.Instance.ReturnObjectToPool(SpawnPrefab, despawnedObject);
            }
        }

        /// <summary>
        /// 지정된 딜레이 후 리스폰을 실행합니다.
        /// </summary>
        private IEnumerator RespawnAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);

            if (isActive && _currentSpawnCount < maxSpawnCount) {
                SpawnObject();
            }
        }

        /// <summary>
        /// 스폰 포인트를 활성화/비활성화합니다.
        /// </summary>
        public void SetActive(bool active) {
            isActive = active;
        }

        /// <summary>
        /// 이벤트에 반응하여 스폰을 시도합니다.
        /// </summary>
        public void OnEventTriggered(string eventName) {
            if (SpawnTrigger == SpawnTriggerType.Event && isActive) {
                TriggerSpawn();
            }
        }
    }
}