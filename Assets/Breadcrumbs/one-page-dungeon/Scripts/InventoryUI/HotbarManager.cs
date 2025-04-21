using System.Collections.Generic;
using Breadcrumbs.InventorySystem;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    public class HotbarManager : MonoBehaviour {
        [SerializeField]
        private Transform hotbarSlotsContainer;
        [SerializeField]
        private GameObject hotbarSlotPrefab;
        [SerializeField]
        private int hotbarSize = 10;

        private List<HotbarSlotUI> hotbarSlots = new List<HotbarSlotUI>();
        private PlayerInventory playerInventory;
        private int currentSelectedSlot = -1;

        // 초기화
        public void Initialize(PlayerInventory inventory) {
            playerInventory = inventory;

            // 핫바 슬롯 생성
            CreateHotbarSlots();

            // 인벤토리 이벤트 구독
            playerInventory.OnSlotChanged += OnInventorySlotChanged;

            // 저장된 핫바 설정 로드
            LoadHotbarSettings();
        }

        // 핫바 슬롯 생성
        private void CreateHotbarSlots() {
            // 기존 슬롯 제거
            foreach (Transform child in hotbarSlotsContainer) {
                Destroy(child.gameObject);
            }

            hotbarSlots.Clear();

            // 새 슬롯 생성
            for (int i = 0; i < hotbarSize; i++) {
                GameObject slotObj = Instantiate(hotbarSlotPrefab, hotbarSlotsContainer);
                HotbarSlotUI slot = slotObj.GetComponent<HotbarSlotUI>();

                slot.Initialize(i, playerInventory, this);
                hotbarSlots.Add(slot);
            }
        }

        // 인벤토리 슬롯 변경 이벤트 핸들러
        private void OnInventorySlotChanged(int slotIndex) {
            // 영향을 받는 핫바 슬롯 업데이트
            foreach (var hotbarSlot in hotbarSlots) {
                if (hotbarSlot.GetLinkedInventorySlot() == slotIndex) {
                    hotbarSlot.UpdateVisuals();
                }
            }
        }

        // 키 입력 처리 (Update 메서드에서 호출)
        public void HandleInput() {
            // 숫자 키 1-0 입력 처리
            for (int i = 0; i < hotbarSize; i++) {
                int keyNumber = (i + 1) % 10; // 0은 10번째 키

                if (Input.GetKeyDown(keyNumber.ToString())) {
                    ActivateSlot(i);
                }
            }
        }

        // 핫바 슬롯 활성화
        public void ActivateSlot(int slotIndex) {
            if (slotIndex < 0 || slotIndex >= hotbarSlots.Count)
                return;

            // 이전 선택 슬롯 선택 해제
            if (currentSelectedSlot >= 0 && currentSelectedSlot < hotbarSlots.Count) {
                hotbarSlots[currentSelectedSlot].SetSelected(false);
            }

            // 새 슬롯 선택
            currentSelectedSlot = slotIndex;
            hotbarSlots[currentSelectedSlot].SetSelected(true);

            // 연결된 인벤토리 슬롯의 아이템 사용
            int inventorySlot = hotbarSlots[slotIndex].GetLinkedInventorySlot();
            if (inventorySlot >= 0) {
                playerInventory.UseItem(inventorySlot);
            }
        }

        // 인벤토리 슬롯을 핫바에 등록
        public void AssignToHotbar(int inventorySlotIndex, int hotbarSlotIndex) {
            if (hotbarSlotIndex < 0 || hotbarSlotIndex >= hotbarSlots.Count)
                return;

            // 핫바 슬롯에 인벤토리 슬롯 연결
            hotbarSlots[hotbarSlotIndex].SetLinkedInventorySlot(inventorySlotIndex);

            // 설정 저장
            SaveHotbarSettings();
        }

        // 핫바 설정 저장
        public void SaveHotbarSettings() {
            // 핫바 설정을 PlayerPrefs에 저장
            for (int i = 0; i < hotbarSlots.Count; i++) {
                int linkedSlot = hotbarSlots[i].GetLinkedInventorySlot();
                PlayerPrefs.SetInt($"Hotbar_Slot_{i}", linkedSlot);
            }

            PlayerPrefs.Save();
        }

        // 핫바 설정 로드
        private void LoadHotbarSettings() {
            // PlayerPrefs에서 핫바 설정 로드
            for (int i = 0; i < hotbarSlots.Count; i++) {
                int linkedSlot = PlayerPrefs.GetInt($"Hotbar_Slot_{i}", -1);
                hotbarSlots[i].SetLinkedInventorySlot(linkedSlot);
            }

            // 첫 번째 슬롯 선택 (기본)
            if (hotbarSlots.Count > 0) {
                currentSelectedSlot = 0;
                hotbarSlots[0].SetSelected(true);
            }
        }
    }
}