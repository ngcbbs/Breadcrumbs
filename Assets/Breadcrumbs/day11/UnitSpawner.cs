using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.day11 {
    public class UnitSpawner : MonoBehaviour {
        [Header("스폰 설정")] [SerializeField] private GameObject[] unitPrefabs; // 스폰 가능한 유닛 프리팹 배열
        [SerializeField] private Transform spawnCenter; // 스폰 중심 위치
        [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f); // 스폰 영역 크기 (x, y, z)
        [SerializeField] private int maxSpawnCount = 10; // 최대 스폰 가능 수
        [SerializeField] private float spawnInterval = 3f; // 스폰 간격 (초)

        [Header("디버그")] [SerializeField] private bool showSpawnArea = true; // 스폰 영역 시각화

        // 내부 변수
        private List<GameObject> spawnedUnits = new List<GameObject>(); // 현재 스폰된 유닛 리스트
        private Transform playerTransform; // 플레이어 트랜스폼
        private int currentSpawnCount = 0; // 현재 스폰된 유닛 수

        private void Start() {
            // 플레이어 찾기
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) {
                Debug.LogError("플레이어를 찾을 수 없습니다. Player 태그가 있는지 확인하세요.");
            }

            // 스폰 중심 위치가 지정되지 않았다면 현재 오브젝트 위치 사용
            if (spawnCenter == null) {
                spawnCenter = transform;
            }

            // 코루틴 시작
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine() {
            while (true) {
                yield return new WaitForSeconds(spawnInterval);

                // 현재 스폰된 유닛 수가 최대 스폰 수보다 적을 때만 스폰
                if (currentSpawnCount < maxSpawnCount) {
                    SpawnUnit();
                }

                // 죽은 유닛 리스트에서 제거
                CleanupDeadUnits();
            }
        }

        private void SpawnUnit() {
            if (unitPrefabs.Length == 0) {
                Debug.LogWarning("스폰할 유닛 프리팹이 등록되지 않았습니다.");
                return;
            }

            // 랜덤한 스폰 위치 계산
            Vector3 randomPosition = GetRandomSpawnPosition();

            // 랜덤한 유닛 프리팹 선택
            GameObject selectedPrefab = unitPrefabs[Random.Range(0, unitPrefabs.Length)];

            // 유닛 생성
            GameObject spawnedUnit = Instantiate(selectedPrefab, randomPosition, Quaternion.identity);
            spawnedUnits.Add(spawnedUnit);
            currentSpawnCount++;

            // EnemyUnit 컴포넌트가 있다면 플레이어 설정
            EnemyUnit enemyUnit = spawnedUnit.GetComponent<EnemyUnit>();
            if (enemyUnit != null && playerTransform != null) {
                enemyUnit.SetTarget(playerTransform);
                enemyUnit.OnUnitDied += HandleUnitDied;
            }
            else {
                Debug.LogWarning("유닛에 EnemyUnit 컴포넌트가 없거나 플레이어가 설정되지 않았습니다.");
            }
        }

        private void HandleUnitDied(EnemyUnit unit) {
            // 유닛이 죽었을 때 호출되는 콜백
            if (spawnedUnits.Contains(unit.gameObject)) {
                currentSpawnCount--;
                spawnedUnits.Remove(unit.gameObject);
            }
        }

        private Vector3 GetRandomSpawnPosition() {
            // 지정된 영역 내에서 랜덤한 위치 계산
            float randomX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float randomY = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
            float randomZ = Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f);

            // 스폰 중심 위치에 랜덤 오프셋 적용
            Vector3 spawnPosition = spawnCenter.position + new Vector3(randomX, randomY, randomZ);

            return spawnPosition;
        }

        private void CleanupDeadUnits() {
            // null 참조가 된 유닛(이미 파괴된 유닛)을 리스트에서 제거
            for (int i = spawnedUnits.Count - 1; i >= 0; i--) {
                if (spawnedUnits[i] == null) {
                    spawnedUnits.RemoveAt(i);
                    currentSpawnCount--;
                }
            }
        }

        private void OnDrawGizmos() {
            // 스폰 영역 시각화
            if (showSpawnArea && spawnCenter != null) {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 반투명 빨간색
                Gizmos.DrawCube(spawnCenter.position, spawnAreaSize);

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(spawnCenter.position, spawnAreaSize);
            }
        }
    }
}
