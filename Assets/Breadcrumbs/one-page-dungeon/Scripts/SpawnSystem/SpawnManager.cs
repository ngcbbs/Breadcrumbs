using System.Collections.Generic;
using Breadcrumbs.Singletons;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰 매니저 - 싱글톤 패턴 적용
    /// </summary>
    public class SpawnManager : PersistentSingleton<SpawnManager> {
        //public static SpawnManager Instance { get; private set; }

        [Header("난이도 설정")]
        public DifficultySettings currentDifficultySettings;
        public DifficultyLevel CurrentDifficulty => currentDifficultySettings.difficultyLevel;

        [Header("스폰 그룹")]
        [SerializeField]
        private List<SpawnPointGroup> spawnPointGroups = new List<SpawnPointGroup>();

        // 오브젝트 풀링 관리 Dictionary
        private Dictionary<int, Queue<GameObject>> _objectPools = new Dictionary<int, Queue<GameObject>>();

        // 스폰된 오브젝트 관리 Dictionary
        private Dictionary<GameObject, SpawnPoint> _spawnedObjects = new Dictionary<GameObject, SpawnPoint>();

        private void Start() {
            // 모든 스폰 포인트 그룹 초기화
            foreach (var group in spawnPointGroups) {
                group.Initialize();
            }
        }

        [SerializeField] private int maxPoolSize = 100; // 풀당 최대 오브젝트 수

        public GameObject GetObjectFromPool(GameObject prefab)
        {
            int prefabID = prefab.GetInstanceID();
    
            if (!_objectPools.ContainsKey(prefabID))
            {
                _objectPools[prefabID] = new Queue<GameObject>();
            }
    
            GameObject obj;
    
            if (_objectPools[prefabID].Count > 0)
            {
                obj = _objectPools[prefabID].Dequeue();
            }
            else
            {
                obj = Instantiate(prefab);
                obj.name = prefab.name + "_" + prefabID;
            }
    
            return obj;
        }

        public void ReturnObjectToPool(GameObject prefab, GameObject obj)
        {
            int prefabID = prefab.GetInstanceID();
    
            if (!_objectPools.ContainsKey(prefabID))
            {
                _objectPools[prefabID] = new Queue<GameObject>();
            }
    
            // 풀 크기 제한 확인
            if (_objectPools[prefabID].Count < maxPoolSize)
            {
                _objectPools[prefabID].Enqueue(obj);
            }
            else
            {
                // 풀이 꽉 찼을 때 오브젝트 제거
                Destroy(obj);
            }
        }

        /// <summary>
        /// 스폰된 오브젝트를 등록합니다.
        /// </summary>
        public void RegisterSpawnedObject(SpawnPoint spawnPoint, GameObject spawnedObject) {
            _spawnedObjects[spawnedObject] = spawnPoint;
        }

        /// <summary>
        /// 오브젝트가 제거될 때 처리합니다.
        /// </summary>
        public void DespawnObject(GameObject obj) {
            if (_spawnedObjects.TryGetValue(obj, out SpawnPoint spawnPoint)) {
                spawnPoint.HandleObjectDespawn(obj);
                _spawnedObjects.Remove(obj);
            }
        }

        /// <summary>
        /// 플레이어 위치를 기반으로 스폰 포인트를 활성화합니다.
        /// </summary>
        public void CheckPlayerPositionForSpawn(Vector3 playerPosition) {
            foreach (var group in spawnPointGroups) {
                if (group.IsActive) {
                    foreach (var spawnPoint in group.SpawnPoints) {
                        if (spawnPoint.SpawnTrigger == SpawnTriggerType.PlayerEnter &&
                            spawnPoint.IsInTriggerArea(playerPosition) &&
                            spawnPoint.MeetsDifficultyRequirement(CurrentDifficulty)) {
                            spawnPoint.TriggerSpawn();
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 특정 이벤트가 발생했을 때 호출됩니다.
        /// </summary>
        public void TriggerEvent(SpawnTriggerType eventType, string eventId) {
            string eventKey = eventType.ToString();
            if (!string.IsNullOrEmpty(eventId))
            {
                eventKey += "_" + eventId;
            }
            
            foreach (var group in spawnPointGroups) {
                if (group.IsActive) {
                    foreach (var spawnPoint in group.SpawnPoints) {
                        spawnPoint.OnEventTriggered(eventKey);
                    }
                }
            }
        }

        /// <summary>
        /// 특정 이벤트가 발생했을 때 호출됩니다.
        /// </summary>
        public void TriggerEvent(string eventName) {
            foreach (var group in spawnPointGroups) {
                if (group.IsActive) {
                    foreach (var spawnPoint in group.SpawnPoints) {
                        spawnPoint.OnEventTriggered(eventName);
                    }
                }
            }
        }

        /// <summary>
        /// 특정 영역의 스폰 포인트 그룹을 활성화합니다.
        /// </summary>
        public void ActivateSpawnPointGroup(string groupId) {
            foreach (var group in spawnPointGroups) {
                if (group.GroupId == groupId) {
                    group.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 특정 영역의 스폰 포인트 그룹을 비활성화합니다.
        /// </summary>
        public void DeactivateSpawnPointGroup(string groupId) {
            foreach (var group in spawnPointGroups) {
                if (group.GroupId == groupId) {
                    group.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 게임 난이도를 변경합니다.
        /// </summary>
        public void ChangeDifficulty(DifficultySettings newDifficultySettings) {
            currentDifficultySettings = newDifficultySettings;

            // 난이도 변경에 따른 추가 로직 실행 가능
            Debug.Log($"난이도가 {newDifficultySettings.difficultyName}(으)로 변경되었습니다.");
        }
    }
}