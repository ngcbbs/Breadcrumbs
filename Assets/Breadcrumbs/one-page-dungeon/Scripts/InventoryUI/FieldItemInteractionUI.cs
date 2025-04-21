using Breadcrumbs.LootingSystem;
using TMPro;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    public class FieldItemInteractionUI : MonoBehaviour {
        [SerializeField]
        private RectTransform interactionPanel;
        [SerializeField]
        private TextMeshProUGUI itemNameText;
        [SerializeField]
        private TextMeshProUGUI promptText;

        private FieldItem currentTarget;

        private void Awake() {
            // 기본 상태는 숨김
            interactionPanel.gameObject.SetActive(false);
        }

        // 필드 아이템 상호작용 패널 표시
        public void ShowInteractionPanel(FieldItem item) {
            if (item == null) return;

            currentTarget = item;

            // UI 업데이트
            itemNameText.text = item.itemData.itemName;
            itemNameText.color = item.itemData.GetRarityColor();

            // 자동 획득 여부에 따라 텍스트 설정
            if (item.itemData.isAutoPickup) {
                promptText.text = "접근 시 자동 획득됩니다";
            } else {
                promptText.text = "[E] 획득";
            }

            // 패널 표시
            interactionPanel.gameObject.SetActive(true);

            // 패널 위치 업데이트
            UpdatePanelPosition();
        }

        // 상호작용 패널 숨기기
        public void HideInteractionPanel() {
            currentTarget = null;
            interactionPanel.gameObject.SetActive(false);
        }

        // 패널 위치 업데이트 (매 프레임 호출)
        private void Update() {
            if (currentTarget != null && interactionPanel.gameObject.activeSelf) {
                UpdatePanelPosition();
            }
        }

        // 패널 위치를 아이템 위치로 설정
        private void UpdatePanelPosition() {
            if (currentTarget == null) return;

            // 필드 아이템의 월드 위치를 스크린 위치로 변환
            Vector3 screenPos = Camera.main.WorldToScreenPoint(currentTarget.transform.position + Vector3.up * 1.5f);

            // 화면 밖으로 벗어난 경우 숨기기
            if (screenPos.z < 0 || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 ||
                screenPos.y > Screen.height) {
                interactionPanel.gameObject.SetActive(false);
                return;
            }

            interactionPanel.gameObject.SetActive(true);
            interactionPanel.position = screenPos;
        }

        // 현재 대상 아이템 반환
        public FieldItem GetCurrentTarget() {
            return currentTarget;
        }
    }
}