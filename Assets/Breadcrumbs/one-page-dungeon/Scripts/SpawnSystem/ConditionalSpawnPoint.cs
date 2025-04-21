using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 조건부 스폰 확장 - 여러 조건을 조합한 스폰 시스템
    /// </summary>
    public class ConditionalSpawnPoint : SpawnPoint {
        [System.Serializable]
        public class SpawnCondition {
            public enum ConditionType {
                PlayerLevel,
                TimePlayed,
                EventOccurred,
                ItemOwned,
                EnemiesKilled,
                PlayerHealth
            }

            public ConditionType conditionType;
            public string parameterName;
            public float minValue;
            public float maxValue = float.MaxValue;
            public bool inverse = false;
        }

        [Header("조건부 스폰 설정")]
        public List<SpawnCondition> conditions = new List<SpawnCondition>();
        public bool requireAllConditions = true; // true: AND, false: OR

        // GameManager 또는 다른 시스템과 연동하여 조건 확인 로직 구현이 필요
        public bool CheckAllConditions() {
            if (conditions.Count == 0) return true;

            bool result = requireAllConditions;

            foreach (var condition in conditions) {
                bool conditionMet = EvaluateCondition(condition);

                if (requireAllConditions) {
                    // AND 연산 - 하나라도 실패하면 전체 실패
                    result &= conditionMet;
                    if (!result) break;
                } else {
                    // OR 연산 - 하나라도 성공하면 전체 성공
                    result |= conditionMet;
                    if (result) break;
                }
            }

            return result;
        }

        private bool EvaluateCondition(SpawnCondition condition) {
            bool result = false;

            // 실제 구현에서는 GameManager, PlayerStats 등의 시스템과 연동
            // 아래는 예시 로직
            switch (condition.conditionType) {
                case SpawnCondition.ConditionType.PlayerLevel:
                    // 예: var playerLevel = GameManager.Instance.Player.Level;
                    int playerLevel = 1; // 임시값
                    result = (playerLevel >= condition.minValue && playerLevel <= condition.maxValue);
                    break;

                case SpawnCondition.ConditionType.TimePlayed:
                    float timePlayed = Time.timeSinceLevelLoad;
                    result = (timePlayed >= condition.minValue && timePlayed <= condition.maxValue);
                    break;

                case SpawnCondition.ConditionType.EventOccurred:
                    // 예: result = GameManager.Instance.EventSystem.HasEventOccurred(condition.parameterName);
                    result = false; // 임시값
                    break;

                case SpawnCondition.ConditionType.ItemOwned:
                    // 예: result = GameManager.Instance.Player.Inventory.HasItem(condition.parameterName);
                    result = false; // 임시값
                    break;

                case SpawnCondition.ConditionType.EnemiesKilled:
                    // 예: var enemiesKilled = GameManager.Instance.EnemiesKilledCount;
                    int enemiesKilled = 0; // 임시값
                    result = (enemiesKilled >= condition.minValue && enemiesKilled <= condition.maxValue);
                    break;

                case SpawnCondition.ConditionType.PlayerHealth:
                    // 예: var healthPercentage = GameManager.Instance.Player.HealthPercentage;
                    float healthPercentage = 100f; // 임시값
                    result = (healthPercentage >= condition.minValue && healthPercentage <= condition.maxValue);
                    break;
            }

            // inverse가 true면 결과를 반전
            return condition.inverse ? !result : result;
        }

        // SpawnPoint의 TriggerSpawn 메서드를 오버라이드
        public new void TriggerSpawn() {
            if (CheckAllConditions()) {
                base.TriggerSpawn();
            }
        }
    }
}