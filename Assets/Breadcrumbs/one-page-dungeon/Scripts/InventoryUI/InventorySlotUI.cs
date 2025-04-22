using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;
using Breadcrumbs.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Breadcrumbs.ItemSystem {
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IDropHandler {
        [Header("UI Components")]
        [SerializeField]
        private Image backgroundImage;
        [SerializeField]
        private Image itemIconImage;
        [SerializeField]
        private Image rarityBorderImage;
        [SerializeField]
        private TextMeshProUGUI quantityText;
        [SerializeField]
        private GameObject selectedHighlight;

        [Header("Colors")]
        [SerializeField]
        private Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField]
        private Color filledSlotColor = Color.white;

        // 슬롯 속성
        public int slotIndex { get; private set; }
        public SlotType slotType { get; private set; }
        public EquipmentSlot equipSlot { get; private set; }

        // 슬롯 상태
        private bool isEmpty = true;
        private bool isSelected = false;

        // 참조
        private PlayerInventory playerInventory;
        private InventoryUIManager inventoryManager;

        // 드래그 관련
        private static GameObject draggedItem;
        private static InventorySlotUI draggingSlot;

        // 초기화 메서드
        public void Initialize(int index, SlotType type, PlayerInventory inventory, InventoryUIManager manager) {
            slotIndex = index;
            slotType = type;
            playerInventory = inventory;
            inventoryManager = manager;

            // 장비 슬롯인 경우 장비 슬롯 타입 설정
            if (type == SlotType.Equipment) {
                equipSlot = (EquipmentSlot)index;
            }

            // 디폴트 상태 설정
            UpdateVisuals(null, 0);
            SetSelected(false);
        }

        // 슬롯 업데이트
        public void UpdateVisuals(ItemData item, int quantity) {
            isEmpty = (item == null || quantity <= 0);

            // 슬롯 백그라운드 색상 설정
            backgroundImage.color = isEmpty ? emptySlotColor : filledSlotColor;

            if (isEmpty) {
                // 아이템 아이콘 숨기기
                itemIconImage.gameObject.SetActive(false);
                rarityBorderImage.gameObject.SetActive(false);
                quantityText.gameObject.SetActive(false);
            } else {
                // 아이템 아이콘 표시
                itemIconImage.gameObject.SetActive(true);
                itemIconImage.sprite = item.icon;

                // 희귀도 테두리 설정
                rarityBorderImage.gameObject.SetActive(true);
                rarityBorderImage.color = item.GetRarityColor();

                // 수량 텍스트 설정 (1개 이상일 때만 표시)
                quantityText.gameObject.SetActive(quantity > 1);
                quantityText.text = quantity.ToString();
            }
        }

        // 선택 상태 설정
        public void SetSelected(bool selected) {
            isSelected = selected;
            selectedHighlight.SetActive(selected);
        }

        // 클릭 이벤트 처리
        public void OnPointerClick(PointerEventData eventData) {
            // 좌클릭: 슬롯 선택
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (!isEmpty) {
                    // 슬롯 선택 처리
                    inventoryManager.SelectSlot(this);

                    // 더블 클릭: 아이템 사용/장착
                    if (eventData.clickCount == 2) {
                        if (slotType == SlotType.Inventory) {
                            playerInventory.UseItem(slotIndex);
                        } else if (slotType == SlotType.Equipment) {
                            playerInventory.UnequipItem(equipSlot);
                        }
                    }
                } else {
                    // 빈 슬롯 선택 취소
                    inventoryManager.DeselectCurrentSlot();
                }
            }
            // 우클릭: 컨텍스트 메뉴
            else if (eventData.button == PointerEventData.InputButton.Right && !isEmpty) {
                // 슬롯 선택
                inventoryManager.SelectSlot(this);

                // 컨텍스트 메뉴 표시
                inventoryManager.ShowContextMenu(this, eventData.position);
            }
        }

        // 드래그 시작
        public void OnBeginDrag(PointerEventData eventData) {
            if (isEmpty) return;

            // 드래그 아이템 생성
            draggedItem = new GameObject("DraggedItem");
            draggedItem.transform.SetParent(inventoryManager.transform);
            draggedItem.transform.SetAsLastSibling(); // 최상위 레이어에 표시

            // 아이템 이미지 설정
            Image dragImage = draggedItem.AddComponent<Image>();
            dragImage.sprite = itemIconImage.sprite;
            dragImage.raycastTarget = false; // 레이캐스트 통과

            RectTransform rt = draggedItem.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 50); // 드래그 아이템 크기

            // 드래그 중인 슬롯 설정
            draggingSlot = this;
        }

        // 드래그 중
        public void OnDrag(PointerEventData eventData) {
            if (draggedItem == null) return;

            // 드래그 아이템 위치 업데이트
            draggedItem.transform.position = eventData.position;
        }

        // 드래그 종료
        public void OnEndDrag(PointerEventData eventData) {
            // 드래그 아이템 제거
            if (draggedItem != null) {
                Destroy(draggedItem);
                draggedItem = null;
            }
        }

        // 드롭 처리
        public void OnDrop(PointerEventData eventData) {
            // 슬롯 간 아이템 이동 처리
            if (draggingSlot != null && draggingSlot != this) {
                // 인벤토리 슬롯 간 이동
                if (draggingSlot.slotType == SlotType.Inventory && slotType == SlotType.Inventory) {
                    playerInventory.MoveItem(draggingSlot.slotIndex, slotIndex);
                }
                // 인벤토리에서 장비 슬롯으로 이동
                else if (draggingSlot.slotType == SlotType.Inventory && slotType == SlotType.Equipment) {
                    // 장착 가능한 슬롯인지 확인
                    ItemData item = inventoryManager.GetItemAtSlot(draggingSlot.slotType, draggingSlot.slotIndex);
                    if (item != null && item.IsEquipment() &&
                        (item.equipSlot == equipSlot ||
                         (item.itemType == ItemType.Ring && equipSlot is EquipmentSlot.Ring1 or EquipmentSlot.Ring2))) {
                        playerInventory.EquipItem(draggingSlot.slotIndex);
                    }
                }
                // 장비 슬롯에서 인벤토리로 이동
                else if (draggingSlot.slotType == SlotType.Equipment && slotType == SlotType.Inventory) {
                    if (isEmpty) {
                        playerInventory.UnequipItem(draggingSlot.equipSlot);
                    }
                }
                // 장비 슬롯 간 이동 (반지만 가능)
                else if (draggingSlot.slotType == SlotType.Equipment && slotType == SlotType.Equipment) {
                    if ((draggingSlot.equipSlot == EquipmentSlot.Ring1 || draggingSlot.equipSlot == EquipmentSlot.Ring2) &&
                        (equipSlot == EquipmentSlot.Ring1 || equipSlot == EquipmentSlot.Ring2)) {
                        // 반지 슬롯 간 교체 로직
                        InventorySlot temp = new InventorySlot();
                        temp.CopyFrom(playerInventory.GetEquipmentSlot(draggingSlot.equipSlot));
                        playerInventory.UnequipItem(draggingSlot.equipSlot);
                        playerInventory.EquipItem(temp.item, equipSlot);
                    }
                }
            }

            draggingSlot = null;
        }
    }
}