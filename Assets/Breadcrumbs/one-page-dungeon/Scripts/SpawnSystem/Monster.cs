using System;
using Breadcrumbs.EventSystem;
using Breadcrumbs.SpawnSystem.Events;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 몬스터 클래스 예시
    /// </summary>
    public class Monster : EventBehaviour, ISpawnable {
        public MonsterData monsterData;

        private int _currentHealth;
        private bool _isAlive = false;
        
        private SpawnManager _spawnManager;

        private void Start() {
            _spawnManager = FindAnyObjectByType<SpawnManager>();
        }

        protected override void RegisterEventHandlers() {
            // 몬스터 관련 이벤트 핸들러 등록
        }

        public void OnSpawned(Vector3 position, Quaternion rotation) {
            _isAlive = true;

            // 난이도에 따른 스탯 적용
            if (_spawnManager != null && _spawnManager.CurrentDifficultySettings != null) {
                var difficulty = _spawnManager.CurrentDifficultySettings;
                _currentHealth = Mathf.RoundToInt(monsterData.baseHealth * difficulty.monsterHealthMultiplier);
            } else {
                _currentHealth = monsterData.baseHealth;
            }

            // 추가 초기화 로직
            Debug.Log($"{monsterData.monsterName}이(가) {position}에 스폰되었습니다. 체력: {_currentHealth}");
        }

        public GameObject SpawnableGameObject => gameObject;

        public void OnSpawned(SpawnPoint spawnPoint) {
            Debug.Log($"{monsterData.monsterName}이(가) 스폰 되었습니다.");
            /*
            // for what?
            throw new NotImplementedException();
            // */
        }

        public void OnDespawned() {
            _isAlive = false;
            Debug.Log($"{monsterData.monsterName}이(가) 디스폰 되었습니다.");
        }

        public void TakeDamage(int damage) {
            if (!_isAlive) return;

            _currentHealth -= damage;
            if (_currentHealth <= 0) {
                Die();
            }
        }

        private void Die() {
            // 아이템 드롭 처리
            DropItems();

            // 죽음 처리 - SpawnManager에 알림
            Dispatch(new DespawnEvent(gameObject));
        }

        private void DropItems() {
            // todo: 구현
            /*
            if (monsterData == null || monsterData.possibleDrops == null)
                return;

            DifficultySettings difficulty = SpawnManager.Instance?.currentDifficultySettings;
            if (difficulty == null) return;

            var dropItems = monsterData.possibleDrops.RollDrops(DungeonDifficulty.Normal);
            foreach (var item in dropItems) {
                // note: 필드 드랍인가?
                var fieldItem = Instantiate(item.item.prefab, transform.position, Quaternion.identity);
                Debug.Log($"{item.item.itemName}이(가) 드롭되었습니다.");
            }
            // */
        }
    }
}