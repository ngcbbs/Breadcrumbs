using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰 포인트 빌더 클래스
    /// </summary>
    public class SpawnPointBuilder {
        private Vector3 _position;
        private SpawnableObjectType _spawnType = SpawnableObjectType.Monster;
        private GameObject _spawnPrefab;
        private float _spawnDelay = 0f;
        private SpawnTriggerType _spawnTrigger = SpawnTriggerType.None;
        private Bounds _triggerArea = new Bounds(Vector3.zero, new Vector3(5, 5, 5));
        private Quaternion _initialRotation = Quaternion.identity;
        private float _positionRandomRange = 0f;
        private DifficultyLevel _requiredDifficulty = DifficultyLevel.Beginner;
        private bool _respawnAfterDeath = true;
        private float _respawnTime = 30f;
        private int _maxSpawnCount = 1;

        public SpawnPointBuilder(Vector3 position) {
            _position = position;
        }

        public SpawnPointBuilder SetSpawnType(SpawnableObjectType type) {
            _spawnType = type;
            return this;
        }

        public SpawnPointBuilder SetSpawnPrefab(GameObject prefab) {
            _spawnPrefab = prefab;
            return this;
        }

        public SpawnPointBuilder SetSpawnDelay(float delay) {
            _spawnDelay = delay;
            return this;
        }

        public SpawnPointBuilder SetSpawnTrigger(SpawnTriggerType trigger) {
            _spawnTrigger = trigger;
            return this;
        }

        public SpawnPointBuilder SetTriggerArea(Bounds area) {
            _triggerArea = area;
            return this;
        }

        public SpawnPointBuilder SetInitialRotation(Quaternion rotation) {
            _initialRotation = rotation;
            return this;
        }

        public SpawnPointBuilder SetPositionRandomRange(float range) {
            _positionRandomRange = range;
            return this;
        }

        public SpawnPointBuilder SetRequiredDifficulty(DifficultyLevel difficulty) {
            _requiredDifficulty = difficulty;
            return this;
        }

        public SpawnPointBuilder SetRespawnAfterDeath(bool respawn) {
            _respawnAfterDeath = respawn;
            return this;
        }

        public SpawnPointBuilder SetRespawnTime(float time) {
            _respawnTime = time;
            return this;
        }

        public SpawnPointBuilder SetMaxSpawnCount(int count) {
            _maxSpawnCount = count;
            return this;
        }

        public SpawnPoint Build() {
            GameObject spawnPointObject = new GameObject("SpawnPoint");
            spawnPointObject.transform.position = _position;

            SpawnPoint spawnPoint = spawnPointObject.AddComponent<SpawnPoint>();
            spawnPoint.SpawnType = _spawnType;
            spawnPoint.SpawnPrefab = _spawnPrefab;
            spawnPoint.SpawnDelay = _spawnDelay;
            spawnPoint.SpawnTrigger = _spawnTrigger;
            spawnPoint.TriggerArea = _triggerArea;
            spawnPoint.InitialRotation = _initialRotation;
            spawnPoint.PositionRandomRange = _positionRandomRange;
            spawnPoint.RequiredDifficulty = _requiredDifficulty;
            spawnPoint.respawnAfterDeath = _respawnAfterDeath;
            spawnPoint.respawnTime = _respawnTime;
            spawnPoint.maxSpawnCount = _maxSpawnCount;

            return spawnPoint;
        }
    }
}