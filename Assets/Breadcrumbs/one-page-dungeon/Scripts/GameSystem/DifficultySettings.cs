using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 난이도별 게임 설정을 정의하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultySettings", menuName = "Breadcrumbs/DifficultySettings")]
    public class DifficultySettings : ScriptableObject {
        [Header("기본 설정")]
        public DifficultyLevel difficultyLevel;
        public string difficultyName;
        [TextArea(2, 4)]
        public string description;

        [Header("몬스터 설정")]
        public float monsterHealthMultiplier = 1.0f;
        public float monsterDamageMultiplier = 1.0f;
        public float monsterSpeedMultiplier = 1.0f;
        public float monsterSpawnRateMultiplier = 1.0f;
        public int maxMonstersPerRoom = 5;

        [Header("아이템 설정")]
        public float commonItemDropRate = 0.4f;
        public float uncommonItemDropRate = 0.3f;
        public float rareItemDropRate = 0.2f;
        public float epicItemDropRate = 0.08f;
        public float legendaryItemDropRate = 0.02f;
    }
}