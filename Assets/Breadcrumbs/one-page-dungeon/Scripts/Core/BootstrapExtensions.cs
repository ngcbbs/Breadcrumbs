using UnityEngine;
using Breadcrumbs.CharacterSystem;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.Core {
    /// <summary>
    /// 부트스트랩 확장 메서드 - 이벤트 리스너 등록 및 서비스 등록을 간소화
    /// </summary>
    public static class BootstrapExtensions {
        /// <summary>
        /// 인벤토리 UI 시스템에 이벤트 리스너 등록
        /// </summary>
        public static void RegisterInventoryUIEvents(this InventoryUIManager uiManager) {
            // 슬롯 변경 이벤트 처리
            EventManager.Subscribe("Inventory.SlotChanged", (data) => {
                if (data is InventorySlotChangedEventData eventData) {
                    // todo: fixme
                    /*
                    uiManager.OnInventorySlotChanged(eventData.SlotIndex);
                    // */
                }
            });

            // 장비 변경 이벤트 처리
            EventManager.Subscribe("Equipment.Changed", (data) => {
                if (data is EquipmentChangedEventData eventData) {
                    // todo: fixme
                    /*
                    uiManager.OnEquipmentSlotChanged(eventData.Slot);
                    // */
                }
            });

            // 인벤토리 정렬 이벤트 처리
            EventManager.Subscribe("Inventory.Sorted", (data) => { uiManager.UpdateAllSlots(); });

            Debug.Log("인벤토리 UI 이벤트 리스너 등록 완료");
        }

        /// <summary>
        /// 아이템 획득 알림 시스템에 이벤트 리스너 등록
        /// </summary>
        public static void RegisterItemPickupEvents(this ItemPickupNotification notification) {
            // 아이템 획득 이벤트 처리
            EventManager.Subscribe("Item.Pickup", (data) => {
                if (data is ItemPickupEventData eventData) {
                    notification.ShowItemPickupNotification(eventData.Item, eventData.Quantity);
                }
            });

            Debug.Log("아이템 획득 알림 이벤트 리스너 등록 완료");
        }

        /// <summary>
        /// 레벨업 이벤트 리스너 등록
        /// </summary>
        public static void RegisterLevelUpEvents(this MonoBehaviour target, System.Action<LevelUpEventData> callback) {
            EventManager.Subscribe("Character.LevelUp", (data) => {
                if (data is LevelUpEventData eventData) {
                    callback(eventData);
                }
            });

            Debug.Log("레벨업 이벤트 리스너 등록 완료");
        }

        /// <summary>
        /// 버프 관련 이벤트 리스너 등록
        /// </summary>
        public static void RegisterBuffEvents(this MonoBehaviour target,
            System.Action<BuffAppliedEventData> onApplied = null,
            System.Action<BuffRemovedEventData> onRemoved = null,
            System.Action<BuffAppliedEventData> onUpdated = null) {
            if (onApplied != null) {
                EventManager.Subscribe("Buff.Applied", (data) => {
                    if (data is BuffAppliedEventData eventData) {
                        onApplied(eventData);
                    }
                });
            }

            if (onRemoved != null) {
                EventManager.Subscribe("Buff.Removed", (data) => {
                    if (data is BuffRemovedEventData eventData) {
                        onRemoved(eventData);
                    }
                });
            }

            if (onUpdated != null) {
                EventManager.Subscribe("Buff.Updated", (data) => {
                    if (data is BuffAppliedEventData eventData) {
                        onUpdated(eventData);
                    }
                });
            }

            Debug.Log("버프 이벤트 리스너 등록 완료");
        }

        /// <summary>
        /// 이벤트 리스너 해제
        /// </summary>
        public static void UnregisterEvents(this MonoBehaviour target, string eventName, System.Action<object> callback) {
            EventManager.Unsubscribe(eventName, callback);
        }
    }
}