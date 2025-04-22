using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;
using Breadcrumbs.InventorySystem;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    [RequireComponent(typeof(CanvasGroup))]
    public class InventorySlotUIEnhanced : InventorySlotUI {
        // 추가 참조
        private CanvasGroup canvasGroup;

        // 오버라이드된 초기화 메서드
        public new void Initialize(int index, SlotType type, PlayerInventory inventory, InventoryUIManager manager) {
            base.Initialize(index, type, inventory, manager);

            canvasGroup = GetComponent<CanvasGroup>();
        }

        // 드래그 시작 시 투명도 조정
        public void SetDragState(bool isDragging) {
            if (isDragging) {
                canvasGroup.alpha = 0.6f;
            } else {
                canvasGroup.alpha = 1.0f;
            }
        }

        // 아이템 하이라이트 효과
        public void HighlightForDrop(bool canDrop) {
            // 드롭 가능 여부에 따라 하이라이트 효과 적용
            if (canDrop) {
                // 드롭 가능 하이라이트 (초록색)
                // 구현 필요
            } else {
                // 드롭 불가 하이라이트 (빨간색)
                // 구현 필요
            }
        }

        // 하이라이트 효과 제거
        public void ClearHighlight() {
            // 하이라이트 효과 제거
            // 구현 필요
        }
    }
}