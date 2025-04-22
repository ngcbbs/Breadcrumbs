using System;
using System.Collections.Generic;
using UnityEngine;
using Breadcrumbs.Core;
using Breadcrumbs.InventorySystem;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.CharacterSystem {
    /// <summary>
    /// 플레이어 인벤토리 시스템 - 아이템 소지 및 관리
    /// </summary>
    public partial class PlayerInventory : MonoBehaviour, IItemOwner, IInventory, INetworkSyncableInventory {
        #region 필드 및 속성

        [Header("인벤토리 설정")]
        [SerializeField]
        private int inventorySize = 28; // 인벤토리 크기
        [SerializeField]
        private Transform dropPosition; // 아이템 드롭 위치

        [Header("추가 설정")]
        [SerializeField]
        private float maxWeight = 100f; // 최대 소지 무게
        [SerializeField]
        private int gold = 0; // 소지 골드

        // 인벤토리 슬롯 배열
        private InventorySlot[] inventorySlots;

        // 장비 슬롯 딕셔너리 (키: 장비 슬롯, 값: 장착된 아이템 데이터)
        private Dictionary<EquipmentSlot, InventorySlot> equipmentSlots = new Dictionary<EquipmentSlot, InventorySlot>();

        // 플레이어 캐릭터 참조
        private PlayerCharacter playerCharacter;

        // 속성 접근자
        public int InventorySize => inventorySlots.Length;
        public float MaxWeight => maxWeight;
        public int Gold => gold;

        #endregion

        #region Unity 라이프사이클 메서드

        private void Awake() {
            // 인벤토리 슬롯 초기화
            inventorySlots = new InventorySlot[inventorySize];
            for (int i = 0; i < inventorySize; i++) {
                inventorySlots[i] = new InventorySlot();
            }

            // 장비 슬롯 초기화
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot))) {
                equipmentSlots[slot] = new InventorySlot();
            }

            // 플레이어 캐릭터 참조 찾기
            playerCharacter = GetComponent<PlayerCharacter>();

            // 서비스 로케이터에 등록
            ServiceLocator.RegisterService<IInventory>(this);
        }

        private void OnDestroy() {
            // 필요한 정리 작업 수행
        }

        #endregion

        #region 유틸리티 메서드

        // 인벤토리 슬롯 직접 액세스 (UI용)
        public InventorySlot GetInventorySlot(int index) {
            if (index < 0 || index >= inventorySlots.Length)
                return new InventorySlot();

            return inventorySlots[index];
        }

        // 장비 슬롯 직접 액세스 (UI용)
        public InventorySlot GetEquipmentSlot(EquipmentSlot slot) {
            if (equipmentSlots.TryGetValue(slot, out InventorySlot equipSlot))
                return equipSlot;

            return new InventorySlot();
        }

        // 골드 추가
        public void AddGold(int amount) {
            if (amount <= 0) return;

            gold += amount;

            // 이벤트 발생
            EventManager.Trigger("Gold.Changed", new GoldChangedEventData(amount, gold, playerCharacter));

            Debug.Log($"{amount} 골드를 획득했습니다. 현재 골드: {gold}");
        }

        // 골드 차감
        public bool SpendGold(int amount) {
            if (amount <= 0) return false;

            if (gold >= amount) {
                gold -= amount;

                // 이벤트 발생
                EventManager.Trigger("Gold.Changed", new GoldChangedEventData(-amount, gold, playerCharacter));

                Debug.Log($"{amount} 골드를 사용했습니다. 남은 골드: {gold}");
                return true;
            } else {
                Debug.Log($"골드가 부족합니다. 필요: {amount}, 현재: {gold}");
                return false;
            }
        }

        // 전체 무게 계산
        public float GetTotalWeight() {
            float totalWeight = 0f;

            // 인벤토리 아이템 무게 계산
            foreach (var slot in inventorySlots) {
                if (!slot.IsEmpty()) {
                    // 아이템 데이터에 무게 정보 추가 필요
                    // totalWeight += slot.item.weight * slot.quantity;
                    totalWeight += 0.5f * slot.quantity; // 임시 값
                }
            }

            // 장비 아이템 무게 계산
            foreach (var pair in equipmentSlots) {
                if (!pair.Value.IsEmpty()) {
                    // 아이템 데이터에 무게 정보 추가 필요
                    // totalWeight += pair.Value.item.weight;
                    totalWeight += 1.0f; // 임시 값
                }
            }

            return totalWeight;
        }

        // 사용된 슬롯 수 계산
        public int GetUsedSlotCount() {
            int count = 0;

            foreach (var slot in inventorySlots) {
                if (!slot.IsEmpty()) {
                    count++;
                }
            }

            return count;
        }

        #endregion
    }

    #region 이벤트 데이터 클래스

    

    #endregion
}