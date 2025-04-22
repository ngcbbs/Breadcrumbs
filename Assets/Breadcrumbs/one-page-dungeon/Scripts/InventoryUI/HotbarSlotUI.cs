using Breadcrumbs.CharacterSystem;
using Breadcrumbs.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Breadcrumbs.ItemSystem {
    public class HotbarSlotUI : MonoBehaviour, IPointerClickHandler {
        [SerializeField]
        private Image backgroundImage;
        [SerializeField]
        private Image itemIconImage;
        [SerializeField]
        private TextMeshProUGUI keyBindText;
        [SerializeField]
        private TextMeshProUGUI quantityText;
        [SerializeField]
        private GameObject selectedHighlight;

        private int slotIndex;
        private int linkedInventorySlot = -1;
        private PlayerInventory playerInventory;
        private HotbarManager hotbarManager;

        // 초기화
        public void Initialize(int index, PlayerInventory inventory, HotbarManager manager) {
            slotIndex = index;
            playerInventory = inventory;
            hotbarManager = manager;

            // 키 바인딩 텍스트 설정 (1~0)
            keyBindText.text = (index + 1) % 10 == 0 ? "0" : ((index + 1) % 10).ToString();

            // 초기화
            UpdateVisuals();
            SetSelected(false);
        }

        // 연결된 인벤토리 슬롯 설정
        public void SetLinkedInventorySlot(int inventorySlotIndex) {
            linkedInventorySlot = inventorySlotIndex;
            UpdateVisuals();
        }

        // 시각적 업데이트
        public void UpdateVisuals() {
            bool isEmpty = (linkedInventorySlot == -1);

            if (isEmpty) {
                // 빈 상태 표시
                itemIconImage.gameObject.SetActive(false);
                quantityText.gameObject.SetActive(false);
            } else {
                // 연결된 인벤토리 슬롯 아이템 표시
                InventorySlot slot = playerInventory.GetInventorySlot(linkedInventorySlot);

                if (slot.IsEmpty()) {
                    // 연결된 슬롯이 비어있으면 연결 해제
                    linkedInventorySlot = -1;
                    itemIconImage.gameObject.SetActive(false);
                    quantityText.gameObject.SetActive(false);
                } else {
                    // 아이템 표시
                    itemIconImage.gameObject.SetActive(true);
                    itemIconImage.sprite = slot.item.icon;

                    // 수량 표시 (1개 이상인 경우만)
                    quantityText.gameObject.SetActive(slot.quantity > 1);
                    quantityText.text = slot.quantity.ToString();
                }
            }
        }

        // 선택 상태 설정
        public void SetSelected(bool selected) {
            selectedHighlight.SetActive(selected);
        }

        // 클릭 이벤트 처리
        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                // 좌클릭: 슬롯 활성화
                hotbarManager.ActivateSlot(slotIndex);
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                // 우클릭: 연결 해제
                linkedInventorySlot = -1;
                UpdateVisuals();
                hotbarManager.SaveHotbarSettings();
            }
        }

        // 핫바 슬롯 정보 가져오기
        public int GetLinkedInventorySlot() {
            return linkedInventorySlot;
        }

        // 슬롯 인덱스 가져오기
        public int GetSlotIndex() {
            return slotIndex;
        }
    }
}