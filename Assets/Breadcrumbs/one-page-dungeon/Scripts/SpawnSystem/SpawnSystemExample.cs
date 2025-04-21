using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰 시스템 사용 예시
    /// </summary>
    public class SpawnSystemExample : MonoBehaviour {
        [SerializeField]
        private GameObject monsterPrefab;
        [SerializeField]
        private GameObject itemPrefab;
        [SerializeField]
        private DifficultySettings beginnerDifficulty;
        [SerializeField]
        private DifficultySettings advancedDifficulty;

        // 런타임에 스폰 포인트 생성 예시
        public void CreateSpawnPointsAtRuntime() {
            // 몬스터 스폰 포인트 생성
            SpawnPoint monsterSpawnPoint = new SpawnPointBuilder(new Vector3(10, 0, 10))
                .SetSpawnType(SpawnableObjectType.Monster)
                .SetSpawnPrefab(monsterPrefab)
                .SetSpawnTrigger(SpawnTriggerType.PlayerEnter)
                .SetTriggerArea(new Bounds(Vector3.zero, new Vector3(8, 4, 8)))
                .SetPositionRandomRange(2.0f)
                .SetMaxSpawnCount(3)
                .SetRespawnAfterDeath(true)
                .SetRespawnTime(15f)
                .Build();

            // 아이템 스폰 포인트 생성
            SpawnPoint itemSpawnPoint = new SpawnPointBuilder(new Vector3(15, 0, 15))
                .SetSpawnType(SpawnableObjectType.Item)
                .SetSpawnPrefab(itemPrefab)
                .SetSpawnTrigger(SpawnTriggerType.Event)
                .SetSpawnDelay(2.0f)
                .SetRequiredDifficulty(DifficultyLevel.Intermediate)
                .Build();

            // 생성된 스폰 포인트를 스폰 매니저에 등록하는 로직은
            // SpawnManager를 확장하거나 별도 메서드를 구현해야 함
        }

        // 난이도 변경 예시
        public void ChangeDifficultyExample(bool toAdvanced) {
            if (toAdvanced) {
                SpawnManager.Instance.ChangeDifficulty(advancedDifficulty);
            } else {
                SpawnManager.Instance.ChangeDifficulty(beginnerDifficulty);
            }
        }

        // 이벤트 트리거 예시
        public void TriggerEventExample() {
            // 보스 처치 후 아이템 스폰 트리거
            SpawnManager.Instance.TriggerEvent("BossDefeated_Dragon");
        }
    }
}