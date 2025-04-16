using Unity.VisualScripting;
using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts {
    public class PlayerSkillExample : MonoBehaviour {
        [Header("Player Skill Simulation")] public KeyCode skillKey = KeyCode.Space;
        public float skillRadius = 5f;
        public LayerMask enemyLayer;

        private readonly Collider[] _enemiesInRange = new Collider[16];
        private Transform _player;

        private void Start() {
            _player = transform;
        }

        private void Update() {
            // 플레이어 스킬 사용 시뮬레이션
            if (Input.GetKeyDown(skillKey)) {
                UsePlayerSkill();
            }
        }

        // 스킬 사용 (예시: 주변 적들에게 도망치게 만듦)
        private void UsePlayerSkill() {
            int enemyCount = Physics.OverlapSphereNonAlloc(
                _player.position,
                skillRadius,
                _enemiesInRange,
                enemyLayer
            );
            
            Debug.Log($"Player used skill! (find enemy count = {enemyCount})");

            for (int i = 0; i < enemyCount; i++) {
                var enemyStateMachine = _enemiesInRange[i].GetComponent<AIMovementStateMachine>();
                if (enemyStateMachine == null)
                    continue;
                Debug.Log($"skill to {enemyStateMachine.name}");
                enemyStateMachine.FleeFromPlayerSkill();
            }

            // 스킬 시각 효과 (예시)
            Debug.DrawRay(_player.position, Vector3.up * 3f, Color.cyan, 1f);
        }

        private void OnDrawGizmos() {
            if (_player != null) {
                // 스킬 범위 시각화
                Gizmos.color = new Color(0, 1, 1, 0.2f);
                Gizmos.DrawWireSphere(_player.position, skillRadius);
            }
        }
    }
}