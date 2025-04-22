using System;
using System.Collections.Generic;
using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;
using Breadcrumbs.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Breadcrumbs.ItemSystem {
    public class InventoryUIManager : MonoBehaviour {
        [Header("Inventory Panel")]
        [SerializeField]
        private GameObject inventoryPanel;
        [SerializeField]
        private Transform inventorySlotsContainer;
        [SerializeField]
        private Transform equipmentSlotsContainer;
        [SerializeField]
        private Button sortButton;
        [SerializeField]
        private Button closeButton;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject inventorySlotPrefab;
        [SerializeField]
        private GameObject equipmentSlotPrefab;

        [Header("UI Components")]
        [SerializeField]
        private InventoryContextMenu contextMenu;
        [SerializeField]
        private DropItemDialog dropItemDialog;
        [SerializeField]
        private SplitItemDialog splitItemDialog;
        [SerializeField]
        private ItemTooltip itemTooltip;

        [Header("Status Display")]
        [SerializeField]
        private TextMeshProUGUI goldText;
        [SerializeField]
        private TextMeshProUGUI weightText;
        [SerializeField]
        private TextMeshProUGUI capacityText;

        // 참조
        private PlayerInventory playerInventory;
        private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
        private Dictionary<EquipmentSlot, InventorySlotUI> equipmentSlots = new Dictionary<EquipmentSlot, InventorySlotUI>();

        // 현재 선택된 슬롯
        private InventorySlotUI selectedSlot;

        // 초기화
        public void Initialize(PlayerInventory inventory) {
            playerInventory = inventory;

            // 슬롯 생성
            CreateInventorySlots();
            CreateEquipmentSlots();

            // 버튼 이벤트 초기화
            sortButton.onClick.AddListener(OnSortButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);

            // UI 컴포넌트 초기화
            contextMenu.Initialize(this, playerInventory);
            dropItemDialog.Initialize(this, playerInventory);
            splitItemDialog.Initialize(this, playerInventory);

            // todo: fixme 이벤트 핸들링 수정 필요.
            /*
            // 플레이어 인벤토리 이벤트 등록
            playerInventory.OnSlotChanged += OnInventorySlotChanged;
            playerInventory.OnEquipChanged += OnEquipmentSlotChanged;
            // */

            // 초기 상태 설정
            UpdateAllSlots();
            UpdateStatusDisplay();

            // 기본 상태는 비활성화
            inventoryPanel.SetActive(false);
        }

        // 인벤토리 슬롯 생성
        private void CreateInventorySlots() {
            // 기존 슬롯 제거
            foreach (Transform child in inventorySlotsContainer) {
                Destroy(child.gameObject);
            }

            inventorySlots.Clear();

            // 인벤토리 크기만큼 슬롯 생성
            for (int i = 0; i < playerInventory.InventorySize; i++) {
                GameObject slotObj = Instantiate(inventorySlotPrefab, inventorySlotsContainer);
                InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();

                slot.Initialize(i, SlotType.Inventory, playerInventory, this);
                inventorySlots.Add(slot);

                // 이벤트 트리거 컴포넌트 추가 (툴팁 표시용)
                EventTrigger trigger = slotObj.AddComponent<EventTrigger>();

                // 마우스 진입 이벤트
                EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener((data) => OnPointerEnterSlot((PointerEventData)data, slot));
                trigger.triggers.Add(enterEntry);

                // 마우스 이탈 이벤트
                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((data) => OnPointerExitSlot((PointerEventData)data));
                trigger.triggers.Add(exitEntry);
            }
        }

        // 장비 슬롯 생성
        private void CreateEquipmentSlots() {
            // 기존 슬롯 제거
            foreach (Transform child in equipmentSlotsContainer) {
                Destroy(child.gameObject);
            }

            equipmentSlots.Clear();

            // 장비 슬롯 유형별로 생성
            foreach (EquipmentSlot equipSlot in Enum.GetValues(typeof(EquipmentSlot))) {
                // 특수 케이스: Ring1, Ring2는 Ring 슬롯으로 처리
                if (equipSlot == EquipmentSlot.Ring1 || equipSlot == EquipmentSlot.Ring2) {
                    if (equipSlot == EquipmentSlot.Ring1) {
                        GameObject slotObj = Instantiate(equipmentSlotPrefab, equipmentSlotsContainer);
                        InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();

                        slot.Initialize((int)equipSlot, SlotType.Equipment, playerInventory, this);
                        equipmentSlots[equipSlot] = slot;

                        // 이벤트 트리거 컴포넌트 추가 (툴팁 표시용)
                        EventTrigger trigger = slotObj.AddComponent<EventTrigger>();

                        // 마우스 진입 이벤트
                        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                        enterEntry.eventID = EventTriggerType.PointerEnter;
                        enterEntry.callback.AddListener((data) => OnPointerEnterSlot((PointerEventData)data, slot));
                        trigger.triggers.Add(enterEntry);

                        // 마우스 이탈 이벤트
                        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                        exitEntry.eventID = EventTriggerType.PointerExit;
                        exitEntry.callback.AddListener((data) => OnPointerExitSlot((PointerEventData)data));
                        trigger.triggers.Add(exitEntry);
                    }

                    // Ring2 슬롯 생성
                    if (equipSlot == EquipmentSlot.Ring2) {
                        GameObject slotObj = Instantiate(equipmentSlotPrefab, equipmentSlotsContainer);
                        InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();

                        slot.Initialize((int)equipSlot, SlotType.Equipment, playerInventory, this);
                        equipmentSlots[equipSlot] = slot;

                        // 이벤트 트리거 컴포넌트 추가 (툴팁 표시용)
                        EventTrigger trigger = slotObj.AddComponent<EventTrigger>();

                        // 마우스 진입 이벤트
                        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                        enterEntry.eventID = EventTriggerType.PointerEnter;
                        enterEntry.callback.AddListener((data) => OnPointerEnterSlot((PointerEventData)data, slot));
                        trigger.triggers.Add(enterEntry);

                        // 마우스 이탈 이벤트
                        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                        exitEntry.eventID = EventTriggerType.PointerExit;
                        exitEntry.callback.AddListener((data) => OnPointerExitSlot((PointerEventData)data));
                        trigger.triggers.Add(exitEntry);
                    }
                } else if (equipSlot != EquipmentSlot.Ring1 && equipSlot != EquipmentSlot.Ring2) // Ring은 Ring1, Ring2로 대체 
                {
                    // todo: 조건문 진입 처리 오류 수정 필요함.
                    GameObject slotObj = Instantiate(equipmentSlotPrefab, equipmentSlotsContainer);
                    InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();

                    slot.Initialize((int)equipSlot, SlotType.Equipment, playerInventory, this);
                    equipmentSlots[equipSlot] = slot;

                    // 이벤트 트리거 컴포넌트 추가 (툴팁 표시용)
                    EventTrigger trigger = slotObj.AddComponent<EventTrigger>();

                    // 마우스 진입 이벤트
                    EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                    enterEntry.eventID = EventTriggerType.PointerEnter;
                    enterEntry.callback.AddListener((data) => OnPointerEnterSlot((PointerEventData)data, slot));
                    trigger.triggers.Add(enterEntry);

                    // 마우스 이탈 이벤트
                    EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                    exitEntry.eventID = EventTriggerType.PointerExit;
                    exitEntry.callback.AddListener((data) => OnPointerExitSlot((PointerEventData)data));
                    trigger.triggers.Add(exitEntry);
                }
            }
        }

        // 인벤토리 슬롯 상태 업데이트 이벤트 핸들러
        private void OnInventorySlotChanged(int slotIndex) {
            if (slotIndex >= 0 && slotIndex < inventorySlots.Count) {
                InventorySlot slot = playerInventory.GetInventorySlot(slotIndex);
                inventorySlots[slotIndex].UpdateVisuals(slot.item, slot.quantity);

                // 상태 표시 업데이트
                UpdateStatusDisplay();
            }
        }

        // 장비 슬롯 상태 업데이트 이벤트 핸들러
        private void OnEquipmentSlotChanged(EquipmentSlot equipSlot) {
            if (equipmentSlots.ContainsKey(equipSlot)) {
                InventorySlot slot = playerInventory.GetEquipmentSlot(equipSlot);
                equipmentSlots[equipSlot].UpdateVisuals(slot.item, slot.quantity);

                // 상태 표시 업데이트
                UpdateStatusDisplay();
            }
        }

        // 모든 슬롯 업데이트
        public void UpdateAllSlots() {
            // 인벤토리 슬롯 업데이트
            for (int i = 0; i < inventorySlots.Count; i++) {
                InventorySlot slot = playerInventory.GetInventorySlot(i);
                inventorySlots[i].UpdateVisuals(slot.item, slot.quantity);
            }

            // 장비 슬롯 업데이트
            foreach (EquipmentSlot equipSlot in equipmentSlots.Keys) {
                InventorySlot slot = playerInventory.GetEquipmentSlot(equipSlot);
                equipmentSlots[equipSlot].UpdateVisuals(slot.item, slot.quantity);
            }
        }

        // 상태 표시 업데이트
        private void UpdateStatusDisplay() {
            // 골드 표시 업데이트
            goldText.text = $"{playerInventory.Gold} G";

            // 무게 표시 업데이트
            float currentWeight = playerInventory.GetTotalWeight();
            float maxWeight = playerInventory.MaxWeight;
            weightText.text = $"{currentWeight:F1} / {maxWeight:F1}";

            // 용량 표시 업데이트
            int usedSlots = playerInventory.GetUsedSlotCount();
            int totalSlots = playerInventory.InventorySize;
            capacityText.text = $"{usedSlots} / {totalSlots}";
        }

        // 인벤토리 패널 토글
        public void ToggleInventory() {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);

            if (inventoryPanel.activeSelf) {
                // 인벤토리 열 때 모든 슬롯 업데이트
                UpdateAllSlots();
                UpdateStatusDisplay();
            } else {
                // 인벤토리 닫을 때 컨텍스트 메뉴와 툴팁 닫기
                contextMenu.Close();
                itemTooltip.Hide();
                dropItemDialog.Close();
                splitItemDialog.Close();
            }
        }

        // 정렬 버튼 클릭 이벤트
        private void OnSortButtonClicked() {
            playerInventory.SortInventory();
            UpdateAllSlots();
        }

        // 닫기 버튼 클릭 이벤트
        private void OnCloseButtonClicked() {
            ToggleInventory();
        }

        // 슬롯 선택
        public void SelectSlot(InventorySlotUI slot) {
            // 현재 선택된 슬롯 선택 해제
            DeselectCurrentSlot();

            // 새 슬롯 선택
            selectedSlot = slot;
            selectedSlot.SetSelected(true);
        }

        // 현재 선택된 슬롯 선택 해제
        public void DeselectCurrentSlot() {
            if (selectedSlot != null) {
                selectedSlot.SetSelected(false);
                selectedSlot = null;
            }
        }

        // 컨텍스트 메뉴 표시
        public void ShowContextMenu(InventorySlotUI slot, Vector2 position) {
            contextMenu.Show(slot, position);
        }

        // 드롭 아이템 다이얼로그 표시
        public void ShowDropItemDialog(InventorySlotUI slot) {
            dropItemDialog.Show(slot);
        }

        // 분할 다이얼로그 표시
        public void ShowSplitItemDialog(InventorySlotUI slot) {
            splitItemDialog.Show(slot);
        }

        // 마우스가 슬롯에 진입할 때 (툴팁 표시)
        private void OnPointerEnterSlot(PointerEventData eventData, InventorySlotUI slot) {
            ItemData item = GetItemAtSlot(slot.slotType, slot.slotIndex);
            if (item != null) {
                itemTooltip.Show(item, eventData.position);
            }
        }

        // 마우스가 슬롯에서 나갈 때 (툴팁 숨기기)
        private void OnPointerExitSlot(PointerEventData eventData) {
            itemTooltip.Hide();
        }

        // 슬롯에 있는 아이템 데이터 가져오기
        public ItemData GetItemAtSlot(SlotType slotType, int slotIndex) {
            if (slotType == SlotType.Inventory) {
                return playerInventory.GetInventorySlot(slotIndex).item;
            } else if (slotType == SlotType.Equipment) {
                return playerInventory.GetEquipmentSlot((EquipmentSlot)slotIndex).item;
            }

            return null;
        }

        // 슬롯에 있는 아이템 수량 가져오기
        public int GetQuantityAtSlot(SlotType slotType, int slotIndex) {
            if (slotType == SlotType.Inventory) {
                return playerInventory.GetInventorySlot(slotIndex).quantity;
            } else if (slotType == SlotType.Equipment) {
                return playerInventory.GetEquipmentSlot((EquipmentSlot)slotIndex).quantity;
            }

            return 0;
        }
    }
}