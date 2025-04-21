using Breadcrumbs.LootingSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Breadcrumbs.ItemSystem {
    // 아이템 마커 클래스
    public class ItemMarker : MonoBehaviour {
        [SerializeField]
        private Image itemIconImage;
        [SerializeField]
        private Image rarityBorderImage;
        [SerializeField]
        private TextMeshProUGUI itemNameText;
        [SerializeField]
        private GameObject offScreenIndicator;

        [Header("Distance Settings")]
        [SerializeField]
        private float maxVisibleDistance = 30f;
        [SerializeField]
        private float minScale = 0.6f;
        [SerializeField]
        private float maxScale = 1.2f;

        // 타겟 아이템 참조
        public FieldItem targetItem { get; private set; }

        // 초기화
        public void Initialize(FieldItem item) {
            targetItem = item;
            UpdateItem(item);
        }

        // 아이템 정보 업데이트
        public void UpdateItem(FieldItem item) {
            if (item == null || item.itemData == null) return;

            // 아이콘 설정
            itemIconImage.sprite = item.itemData.icon;

            // 희귀도 테두리 색상 설정
            rarityBorderImage.color = item.itemData.GetRarityColor();

            // 아이템 이름 설정
            itemNameText.text = item.itemData.itemName;
            itemNameText.color = item.itemData.GetRarityColor();
        }

        // 화면 밖 모드 설정
        public void SetOffScreenMode(bool isOffScreen) {
            // 화면 밖 표시기 활성화/비활성화
            offScreenIndicator.SetActive(isOffScreen);
        }

        // 마커 위치 설정
        public void SetPosition(Vector3 position) {
            transform.position = position;
        }

        // 거리에 따른 마커 업데이트
        public void UpdateByDistance(float distance) {
            // 거리에 따른 스케일 계산
            float scaleRatio = Mathf.Clamp01(1f - (distance / maxVisibleDistance));
            float currentScale = Mathf.Lerp(minScale, maxScale, scaleRatio);

            // 거리에 따른 투명도 계산
            float alpha = Mathf.Clamp01(1f - (distance / maxVisibleDistance));

            // 적용
            transform.localScale = new Vector3(currentScale, currentScale, 1f);

            // 투명도 적용 (CanvasGroup이 있다면)
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null) {
                canvasGroup.alpha = alpha;
            }
        }
    }
}