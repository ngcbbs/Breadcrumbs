using System.Collections.Generic;
using Breadcrumbs.LootingSystem;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    public class EnhancedFieldItemUI : MonoBehaviour {
        [SerializeField]
        private RectTransform markerContainer;
        [SerializeField]
        private GameObject itemMarkerPrefab;

        // 아이템 마커 딕셔너리 (아이템 인스턴스 ID -> 마커)
        private Dictionary<int, ItemMarker> activeMarkers = new Dictionary<int, ItemMarker>();

        // 카메라 참조
        private Camera mainCamera;

        private void Awake() {
            mainCamera = Camera.main;
        }

        private void LateUpdate() {
            // 모든 활성 마커 업데이트
            foreach (var marker in activeMarkers.Values) {
                UpdateMarkerPosition(marker);
            }
        }

        // 필드 아이템 마커 생성/업데이트
        public void RegisterFieldItem(FieldItem item) {
            if (item == null) return;

            // 이미 마커가 있는지 확인
            if (activeMarkers.TryGetValue(item.instanceId, out ItemMarker marker)) {
                // 기존 마커 업데이트
                marker.UpdateItem(item);
            } else {
                // 새 마커 생성
                GameObject markerObj = Instantiate(itemMarkerPrefab, markerContainer);
                ItemMarker newMarker = markerObj.GetComponent<ItemMarker>();

                if (newMarker != null) {
                    newMarker.Initialize(item);
                    activeMarkers.Add(item.instanceId, newMarker);
                }
            }
        }

        // 필드 아이템 마커 제거
        public void UnregisterFieldItem(int itemInstanceId) {
            if (activeMarkers.TryGetValue(itemInstanceId, out ItemMarker marker)) {
                Destroy(marker.gameObject);
                activeMarkers.Remove(itemInstanceId);
            }
        }

        // 마커 위치 업데이트
        private void UpdateMarkerPosition(ItemMarker marker) {
            if (marker == null || marker.targetItem == null || mainCamera == null) return;

            // 아이템의 월드 위치를 스크린 위치로 변환
            Vector3 screenPos = mainCamera.WorldToScreenPoint(marker.targetItem.transform.position);

            // 화면 밖에 있는지 확인
            bool isOffScreen = screenPos.z < 0 ||
                               screenPos.x < 0 ||
                               screenPos.x > Screen.width ||
                               screenPos.y < 0 ||
                               screenPos.y > Screen.height;

            if (isOffScreen) {
                // 화면 밖일 때 화면 가장자리에 위치시키기
                if (screenPos.z < 0) {
                    // 카메라 뒤에 있을 경우 방향 반전
                    screenPos.x = Screen.width - screenPos.x;
                    screenPos.y = Screen.height - screenPos.y;
                }

                // 화면 가장자리로 위치 제한
                float borderOffset = 50f;
                screenPos.x = Mathf.Clamp(screenPos.x, borderOffset, Screen.width - borderOffset);
                screenPos.y = Mathf.Clamp(screenPos.y, borderOffset, Screen.height - borderOffset);

                // 화면 밖 모드 활성화
                marker.SetOffScreenMode(true);
            } else {
                // 화면 안 모드 활성화
                marker.SetOffScreenMode(false);
            }

            // 마커 위치 설정
            marker.SetPosition(screenPos);

            // 거리에 따른 마커 크기/투명도 조정
            float distance = Vector3.Distance(Camera.main.transform.position, marker.targetItem.transform.position);
            marker.UpdateByDistance(distance);
        }
    }
}