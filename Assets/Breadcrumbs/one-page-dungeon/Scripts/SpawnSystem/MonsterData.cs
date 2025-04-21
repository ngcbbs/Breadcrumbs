using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰할 몬스터의 기본 정보를 정의하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterData", menuName = "DungeonGame/MonsterData")]
    public class MonsterData : ScriptableObject {
        public string monsterName;
        public GameObject monsterPrefab;
        public int baseHealth;
        public int baseDamage;
        public float baseSpeed;
        public DifficultyLevel minimumDifficulty;
        public List<ItemDropData> possibleDrops;
    }
}