using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 플레이어 캐릭터의 위치 변화에 따른 던전 영역 관리 예시
    /// </summary>
    public class PlayerAreaManager : MonoBehaviour {
        private string currentRoomId = "";
        private DungeonManager dungeonManager;

        private void Start() {
            dungeonManager = FindObjectOfType<DungeonManager>();
        }

        // 플레이어가 새로운 방에 들어왔을 때 호출
        public void OnPlayerEnterRoom(string roomId) {
            if (currentRoomId != roomId) {
                // 이전 방 나가기
                if (!string.IsNullOrEmpty(currentRoomId)) {
                    dungeonManager.OnRoomExited(currentRoomId);
                }

                // 새 방 입장
                currentRoomId = roomId;
                dungeonManager.OnRoomEntered(roomId);
            }
        }
    }
}