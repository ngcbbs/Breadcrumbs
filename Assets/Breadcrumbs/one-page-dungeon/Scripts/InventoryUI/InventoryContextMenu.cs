using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;
using Breadcrumbs.InventorySystem;
using UnityEngine;
using UnityEngine.UI;

namespace Breadcrumbs.ItemSystem {
    public class InventoryContextMenu : MonoBehaviour {
        [SerializeField]
        private Button useButton;
        [SerializeField]
        private Button equipButton;
        [SerializeField]
        private Button unequipButton;
        [SerializeField]
        private Button dropButton;
        [SerializeField]
        private Button splitButton;
        [SerializeField]
        private Button closeButton;

        private InventorySlotUI targetSlot;
        private InventoryUIManager inventoryManager;
        private PlayerInventory playerInventory;

        // 초기화
        public void Initialize(InventoryUIManager manager, PlayerInventory inventory) {
            inventoryManager = manager;
            playerInventory = inventory;

            // 버튼 이벤트 초기화
            useButton.onClick.AddListener(OnUseButtonClicked);
            equipButton.onClick.AddListener(OnEquipButtonClicked);
            unequipButton.onClick.AddListener(OnUnequipButtonClicked);
            dropButton.onClick.AddListener(OnDropButtonClicked);
            splitButton.onClick.AddListener(OnSplitButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);

            // 기본 상태는 비활성화
            gameObject.SetActive(false);
        }

        // 컨텍스트 메뉴 표시
        public void Show(InventorySlotUI slot, Vector2 position) {
            targetSlot = slot;
            ItemData item = inventoryManager.GetItemAtSlot(slot.slotType, slot.slotIndex);

            // 위치 설정
            RectTransform rt = GetComponent<RectTransform>();
            rt.position = position;

            // 버튼 표시 여부 설정
            useButton.gameObject.SetActive(item != null && item.IsConsumable() && slot.slotType == SlotType.Inventory);
            equipButton.gameObject.SetActive(item != null && item.IsEquipment() && slot.slotType == SlotType.Inventory);
            unequipButton.gameObject.SetActive(slot.slotType == SlotType.Equipment);
            dropButton.gameObject.SetActive(true);
            splitButton.gameObject.SetActive(
                item != null && inventoryManager.GetQuantityAtSlot(slot.slotType, slot.slotIndex) > 1);

            // 메뉴 표시
            gameObject.SetActive(true);
        }

        // 사용 버튼 클릭
        private void OnUseButtonClicked() {
            if (targetSlot.slotType == SlotType.Inventory) {
                playerInventory.UseItem(targetSlot.slotIndex);
            }

            Close();
        }

        // 장착 버튼 클릭
        private void OnEquipButtonClicked() {
            if (targetSlot.slotType == SlotType.Inventory) {
                playerInventory.EquipItem(targetSlot.slotIndex);
            }

            Close();
        }

        // 장착 해제 버튼 클릭
        private void OnUnequipButtonClicked() {
            if (targetSlot.slotType == SlotType.Equipment) {
                playerInventory.UnequipItem(targetSlot.equipSlot);
            }

            Close();
        }

        // 버리기 버튼 클릭
        private void OnDropButtonClicked() {
            if (targetSlot.slotType == SlotType.Inventory) {
                inventoryManager.ShowDropItemDialog(targetSlot);
            } else if (targetSlot.slotType == SlotType.Equipment) {
                // 장비 아이템 직접 버리기
                ItemData item = inventoryManager.GetItemAtSlot(targetSlot.slotType, targetSlot.slotIndex);
                if (item != null) {
                    playerInventory.UnequipAndDropItem(targetSlot.equipSlot);
                }
            }

            Close();
        }

        // 분할 버튼 클릭
        private void OnSplitButtonClicked() {
            inventoryManager.ShowSplitItemDialog(targetSlot);
            Close();
        }

        // 닫기 버튼 클릭
        private void OnCloseButtonClicked() {
            Close();
        }

        // 컨텍스트 메뉴 닫기
        public void Close() {
            gameObject.SetActive(false);
        }
    }
}