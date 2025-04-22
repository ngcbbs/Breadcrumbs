using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;
using Breadcrumbs.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Breadcrumbs.ItemSystem {
    public class DropItemDialog : MonoBehaviour {
        [SerializeField]
        private Slider quantitySlider;
        [SerializeField]
        private TextMeshProUGUI quantityText;
        [SerializeField]
        private TextMeshProUGUI itemNameText;
        [SerializeField]
        private Image itemIconImage;
        [SerializeField]
        private Button confirmButton;
        [SerializeField]
        private Button cancelButton;

        private InventorySlotUI targetSlot;
        private InventoryUIManager inventoryManager;
        private PlayerInventory playerInventory;

        // 초기화
        public void Initialize(InventoryUIManager manager, PlayerInventory inventory) {
            inventoryManager = manager;
            playerInventory = inventory;

            // 버튼 이벤트 초기화
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);

            // 슬라이더 이벤트 초기화
            quantitySlider.onValueChanged.AddListener(OnSliderValueChanged);

            // 기본 상태는 비활성화
            gameObject.SetActive(false);
        }

        // 다이얼로그 표시
        public void Show(InventorySlotUI slot) {
            targetSlot = slot;
            ItemData item = inventoryManager.GetItemAtSlot(slot.slotType, slot.slotIndex);
            int quantity = inventoryManager.GetQuantityAtSlot(slot.slotType, slot.slotIndex);

            // UI 업데이트
            itemNameText.text = item.itemName;
            itemIconImage.sprite = item.icon;

            // 슬라이더 설정
            quantitySlider.minValue = 1;
            quantitySlider.maxValue = quantity;
            quantitySlider.value = 1;
            OnSliderValueChanged(1);

            // 다이얼로그 표시
            gameObject.SetActive(true);
        }

        // 슬라이더 값 변경 이벤트
        private void OnSliderValueChanged(float value) {
            int intValue = Mathf.RoundToInt(value);
            quantityText.text = $"{intValue} / {quantitySlider.maxValue}";
        }

        // 확인 버튼 클릭
        private void OnConfirmButtonClicked() {
            int dropQuantity = Mathf.RoundToInt(quantitySlider.value);

            // 아이템 버리기 처리
            if (targetSlot.slotType == SlotType.Inventory) {
                // todo: fixme
                /*
                playerInventory.DropItem(targetSlot.slotIndex, dropQuantity);
                // */
            }

            Close();
        }

        // 취소 버튼 클릭
        private void OnCancelButtonClicked() {
            Close();
        }

        // 다이얼로그 닫기
        public void Close() {
            gameObject.SetActive(false);
        }
    }
}