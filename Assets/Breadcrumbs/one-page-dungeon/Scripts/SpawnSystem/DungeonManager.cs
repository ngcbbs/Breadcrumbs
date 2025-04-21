using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 던전 매니저 예시
    /// </summary>
    public class DungeonManager : MonoBehaviour {
        public void OnRoomEntered(string roomId) {
            // 방 입장 시 해당 방의 스폰 포인트 그룹 활성화
            SpawnManager.Instance.ActivateSpawnPointGroup(roomId);
        }

        public void OnRoomExited(string roomId) {
            // 방 퇴장 시 해당 방의 스폰 포인트 그룹 비활성화 (선택 사항)
            SpawnManager.Instance.DeactivateSpawnPointGroup(roomId);
        }

        public void OnBossDefeated(string bossId) {
            // 보스 처치 이벤트 트리거
            SpawnManager.Instance.TriggerEvent("BossDefeated_" + bossId);
        }
    }
}