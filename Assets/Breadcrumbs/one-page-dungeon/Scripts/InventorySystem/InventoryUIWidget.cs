using System;
using Breadcrumbs.CharacterSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Breadcrumbs.Core;

namespace Breadcrumbs.UI {
    /// <summary>
    /// 인벤토리 UI 위젯 - 이벤트 기반 구조로 개선된 인벤토리 UI 컴포넌트
    /// </summary>
    public class InventoryUIWidget : MonoBehaviour {
        [Header("UI 요소")]
        [SerializeField]
        private TextMeshProUGUI goldText;
        [SerializeField]
        private TextMeshProUGUI weightText;
        [SerializeField]
        private Slider weightSlider;
        [SerializeField]
        private TextMeshProUGUI capacityText;
        [SerializeField]
        private Button sortButton;

        [Header("참조")]
        [SerializeField]
        private Transform inventorySlotsContainer;

        // 캐싱된 서비스 참조
        private PlayerInventory inventory;
        private PlayerCharacter character;

        private void Awake() {
            // 버튼 이벤트 연결
            if (sortButton != null) {
                sortButton.onClick.AddListener(OnSortButtonClicked);
            }
        }

        private void Start() {
            // 서비스 로케이터에서 필요한 서비스 가져오기
            inventory = ServiceLocator.GetService<IInventory>() as PlayerInventory;
            character = ServiceLocator.GetService<PlayerCharacter>();

            // 이벤트 리스너 등록
            RegisterEvents();

            // 초기 UI 상태 설정
            if (inventory != null) {
                UpdateInventoryUI();
            }
        }

        private void OnDestroy() {
            // 이벤트 리스너 해제
            UnregisterEvents();

            // 버튼 이벤트 해제
            if (sortButton != null) {
                sortButton.onClick.RemoveListener(OnSortButtonClicked);
            }
        }

        /// <summary>
        /// 이벤트 리스너 등록
        /// </summary>
        private void RegisterEvents() {
            // 인벤토리 슬롯 변경 이벤트
            EventManager.Subscribe("Inventory.SlotChanged", OnInventoryChanged);

            // 장비 변경 이벤트
            EventManager.Subscribe("Equipment.Changed", OnInventoryChanged);

            // 골드 변경 이벤트
            EventManager.Subscribe("Gold.Changed", OnGoldChanged);

            // 인벤토리 정렬 이벤트
            EventManager.Subscribe("Inventory.Sorted", OnInventoryChanged);
        }

        /// <summary>
        /// 이벤트 리스너 해제
        /// </summary>
        private void UnregisterEvents() {
            EventManager.Unsubscribe("Inventory.SlotChanged", OnInventoryChanged);
            EventManager.Unsubscribe("Equipment.Changed", OnInventoryChanged);
            EventManager.Unsubscribe("Gold.Changed", OnGoldChanged);
            EventManager.Unsubscribe("Inventory.Sorted", OnInventoryChanged);
        }

        /// <summary>
        /// 인벤토리 변경 이벤트 처리
        /// </summary>
        private void OnInventoryChanged(object data) {
            UpdateInventoryUI();
        }

        /// <summary>
        /// 골드 변경 이벤트 처리
        /// </summary>
        private void OnGoldChanged(object data) {
            if (data is GoldChangedEventData eventData) {
                UpdateGoldDisplay(eventData.CurrentGold);
            }
        }

        /// <summary>
        /// 인벤토리 UI 업데이트
        /// </summary>
        private void UpdateInventoryUI() {
            if (inventory == null) return;

            // 골드 표시 업데이트
            UpdateGoldDisplay(inventory.Gold);

            // 무게 표시 업데이트
            float currentWeight = inventory.GetTotalWeight();
            float maxWeight = inventory.MaxWeight;

            if (weightText != null) {
                weightText.text = $"{currentWeight:F1} / {maxWeight:F1}";
            }

            // 무게 슬라이더 업데이트
            if (weightSlider != null) {
                weightSlider.minValue = 0f;
                weightSlider.maxValue = maxWeight;
                weightSlider.value = currentWeight;
            }

            // 용량 표시 업데이트
            int usedSlots = inventory.GetUsedSlotCount();
            int totalSlots = inventory.InventorySize;

            if (capacityText != null) {
                capacityText.text = $"{usedSlots} / {totalSlots}";
            }
        }

        /// <summary>
        /// 골드 표시 업데이트
        /// </summary>
        private void UpdateGoldDisplay(int gold) {
            if (goldText != null) {
                goldText.text = $"{gold:N0} G";
            }
        }

        /// <summary>
        /// 정렬 버튼 클릭 이벤트
        /// </summary>
        private void OnSortButtonClicked() {
            if (inventory != null) {
                inventory.SortInventory();
            }
        }

        /// <summary>
        /// 인벤토리 토글 (외부에서 호출)
        /// </summary>
        public void ToggleInventory(bool show) {
            gameObject.SetActive(show);

            if (show) {
                UpdateInventoryUI();
            }
        }
    }
}