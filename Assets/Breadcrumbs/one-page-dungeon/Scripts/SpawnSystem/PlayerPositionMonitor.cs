using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 플레이어 위치 모니터링 컴포넌트 예시
    /// </summary>
    public class PlayerPositionMonitor : MonoBehaviour {
        private Transform _playerTransform;
        
        private SpawnManager _spawnManager;

        private void Start() {
            _spawnManager = FindAnyObjectByType<SpawnManager>();
            _playerTransform = GetComponent<Transform>();
        }

        private const float kCheckInterval = 0.5f; // 0.5초마다 체크
        private float _timer = 0f;

        private void Update() {
            _timer += Time.deltaTime;
            if (_timer < kCheckInterval)
                return;

            _timer = 0f;
            if (_spawnManager != null) {
                _spawnManager.CheckPlayerPositionForSpawn(_playerTransform.position);
            }
        }
    }
}