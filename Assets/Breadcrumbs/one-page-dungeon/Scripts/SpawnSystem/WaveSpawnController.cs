using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 몬스터 웨이브 스폰 시스템 예제
    /// </summary>
    public class WaveSpawnController : MonoBehaviour {
        [System.Serializable]
        public class SpawnWave {
            public string waveName;
            public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
            public float timeBetweenSpawns = 1f;
            public int totalEnemiesInWave = 10;
            public float delayBeforeNextWave = 5f;
            public bool isEndlessWave = false;
        }

        public List<SpawnWave> waves = new List<SpawnWave>();
        public bool autoStartWaves = true;

        private int currentWaveIndex = -1;
        private int enemiesRemainingInCurrentWave = 0;
        private int enemiesSpawnedInCurrentWave = 0;
        private bool isSpawningWave = false;

        private void Start() {
            if (autoStartWaves && waves.Count > 0) {
                StartNextWave();
            }
        }

        /// <summary>
        /// 다음 웨이브 시작
        /// </summary>
        public void StartNextWave() {
            if (isSpawningWave) return;

            currentWaveIndex++;

            if (currentWaveIndex >= waves.Count) {
                // 모든 웨이브 완료
                Debug.Log("모든 웨이브가 완료되었습니다.");
                return;
            }

            SpawnWave wave = waves[currentWaveIndex];
            enemiesRemainingInCurrentWave = wave.totalEnemiesInWave;
            enemiesSpawnedInCurrentWave = 0;

            Debug.Log($"웨이브 {currentWaveIndex + 1} ({wave.waveName}) 시작!");

            // 웨이브 시작 이벤트 발생
            SpawnManager.Instance.TriggerEvent("WaveStart_" + currentWaveIndex);

            // 웨이브 스폰 시작
            StartCoroutine(SpawnWaveCoroutine(wave));
        }

        /// <summary>
        /// 웨이브 스폰 코루틴
        /// </summary>
        private IEnumerator SpawnWaveCoroutine(SpawnWave wave) {
            isSpawningWave = true;

            while ((enemiesSpawnedInCurrentWave < wave.totalEnemiesInWave || wave.isEndlessWave) &&
                   wave.spawnPoints.Count > 0) {
                // 무작위 스폰 포인트 선택
                int randomIndex = UnityEngine.Random.Range(0, wave.spawnPoints.Count);
                SpawnPoint selectedSpawnPoint = wave.spawnPoints[randomIndex];

                if (selectedSpawnPoint != null && selectedSpawnPoint.IsActive()) {
                    selectedSpawnPoint.TriggerSpawn();
                    enemiesSpawnedInCurrentWave++;

                    // 몬스터가 처치되면 감소하는 로직은 별도 구현 필요
                    // 이벤트 시스템 통해 DespawnObject에서 OnMonsterKilled 이벤트 발생시키고 
                    // 여기서 구독해서 카운트 감소
                }

                yield return new WaitForSeconds(wave.timeBetweenSpawns);
            }

            if (!wave.isEndlessWave) {
                // 다음 웨이브까지 대기
                yield return new WaitForSeconds(wave.delayBeforeNextWave);

                // 다음 웨이브 시작
                isSpawningWave = false;
                StartNextWave();
            } else {
                isSpawningWave = false;
            }
        }

        /// <summary>
        /// 몬스터가 처치되었을 때 호출
        /// </summary>
        public void OnEnemyKilled() {
            enemiesRemainingInCurrentWave--;

            if (enemiesRemainingInCurrentWave <= 0 && !waves[currentWaveIndex].isEndlessWave) {
                // 웨이브 완료 이벤트 발생
                SpawnManager.Instance.TriggerEvent("WaveComplete_" + currentWaveIndex);
            }
        }

        /// <summary>
        /// 현재 웨이브 강제 종료
        /// </summary>
        public void EndCurrentWave() {
            if (isSpawningWave) {
                StopAllCoroutines();
                isSpawningWave = false;

                // 웨이브 종료 이벤트 발생
                SpawnManager.Instance.TriggerEvent("WaveForceEnd_" + currentWaveIndex);
            }
        }
    }
}