using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰 포인트 감지 트리거 - 영역 기반 트리거용 컴포넌트
    /// </summary>
    public class SpawnTriggerArea : MonoBehaviour {
        [SerializeField]
        private string triggerId;
        [SerializeField]
        private LayerMask triggerLayers;

        private void OnTriggerEnter(Collider other) {
            // 해당 레이어가 트리거 대상인지 확인
            if (((1 << other.gameObject.layer) & triggerLayers) != 0) {
                // 플레이어인 경우
                if (other.CompareTag("Player")) {
                    // 영역 진입 이벤트 발생
                    SpawnManager.Instance.TriggerEvent("PlayerEnter_" + triggerId);

                    // 룸 매니저가 있는 경우 룸 정보 갱신
                    PlayerAreaManager areaManager = other.GetComponent<PlayerAreaManager>();
                    if (areaManager != null) {
                        areaManager.OnPlayerEnterRoom(triggerId);
                    }
                }
            }
        }
    }
}