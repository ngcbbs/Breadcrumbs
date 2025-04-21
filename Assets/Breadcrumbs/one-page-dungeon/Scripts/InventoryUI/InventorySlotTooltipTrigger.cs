using Breadcrumbs.InventorySystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Breadcrumbs.ItemSystem {
    [RequireComponent(typeof(InventorySlotUI))]
    public class InventorySlotTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        private InventorySlotUI slotUI;
        private ItemTooltip tooltip;
        private float tooltipDelay = 0.5f;
        private float tooltipTimer = 0f;
        private bool isHovering = false;

        private void Awake() {
            slotUI = GetComponent<InventorySlotUI>();
        }

        public void Initialize(ItemTooltip tooltipReference) {
            tooltip = tooltipReference;
        }

        private void Update() {
            if (isHovering) {
                tooltipTimer += Time.deltaTime;

                if (tooltipTimer >= tooltipDelay) {
                    // 아이템 정보 가져오기
                    ItemData item = null;

                    // todo: GetItemData 누락 수정 필요.
                    /*
                    if (slotUI.slotType == PlayerInventory.SlotType.Inventory) {
                        item = slotUI.GetItemData();
                    } else if (slotUI.slotType == PlayerInventory.SlotType.Equipment) {
                        item = slotUI.GetItemData();
                    }
                    // */

                    if (item != null) {
                        // 툴팁 표시
                        tooltip.Show(item, Input.mousePosition);
                    }

                    isHovering = false;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            isHovering = true;
            tooltipTimer = 0f;
        }

        public void OnPointerExit(PointerEventData eventData) {
            isHovering = false;
            tooltipTimer = 0f;
            tooltip.Hide();
        }
    }
}