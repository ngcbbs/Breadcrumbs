using Breadcrumbs.InventorySystem;
using Breadcrumbs.LootingSystem;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    public class PlayerInventoryController : MonoBehaviour {
        [Header("UI References")]
        [SerializeField]
        private InventoryUIManager inventoryUIManager;
        [SerializeField]
        private HotbarManager hotbarManager;
        [SerializeField]
        private ItemPickupNotification pickupNotification;
        [SerializeField]
        private FieldItemInteractionUI fieldItemInteractionUI;

        [Header("Interaction Settings")]
        [SerializeField]
        private float pickupRange = 2.5f;
        [SerializeField]
        private LayerMask fieldItemLayer;
        [SerializeField]
        private KeyCode inventoryToggleKey = KeyCode.I;
        [SerializeField]
        private KeyCode pickupKey = KeyCode.E;

        // 참조
        private PlayerInventory playerInventory;
        private FieldItem currentTargetItem;

        private void Awake() {
            // 컴포넌트 참조 찾기
            playerInventory = GetComponent<PlayerInventory>();

            if (playerInventory == null) {
                Debug.LogError("PlayerInventory 컴포넌트를 찾을 수 없습니다!");
                enabled = false;
                return;
            }

            // UI 초기화
            if (inventoryUIManager != null)
                inventoryUIManager.Initialize(playerInventory);

            if (hotbarManager != null)
                hotbarManager.Initialize(playerInventory);

            // 아이템 획득 이벤트 구독
            playerInventory.OnItemPickedUp += OnItemPickedUp;
        }

        private void Update() {
            // 인벤토리 UI 토글
            if (Input.GetKeyDown(inventoryToggleKey)) {
                inventoryUIManager.ToggleInventory();
            }

            // 핫바 키 입력 처리
            if (hotbarManager != null) {
                hotbarManager.HandleInput();
            }

            // 필드 아이템 탐색 및 상호작용 처리
            CheckForFieldItems();

            // 아이템 획득 키 처리
            if (Input.GetKeyDown(pickupKey) && currentTargetItem != null) {
                TryPickupItem();
            }
        }

        // 필드 아이템 탐색
        private void CheckForFieldItems() {
            // 플레이어 주변 필드 아이템 탐색
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRange, fieldItemLayer);

            if (hitColliders.Length > 0) {
                // 가장 가까운 아이템 찾기
                float closestDistance = float.MaxValue;
                FieldItem closestItem = null;

                foreach (var collider in hitColliders) {
                    FieldItem item = collider.GetComponent<FieldItem>();
                    if (item != null) {
                        float distance = Vector3.Distance(transform.position, item.transform.position);
                        if (distance < closestDistance) {
                            closestDistance = distance;
                            closestItem = item;
                        }
                    }
                }

                // 가장 가까운 아이템이 변경됐을 때만 UI 업데이트
                if (closestItem != currentTargetItem) {
                    currentTargetItem = closestItem;

                    if (fieldItemInteractionUI != null && currentTargetItem != null) {
                        fieldItemInteractionUI.ShowInteractionPanel(currentTargetItem);
                    }
                }
            } else {
                // 범위 내 아이템이 없는 경우
                if (currentTargetItem != null) {
                    currentTargetItem = null;

                    if (fieldItemInteractionUI != null) {
                        fieldItemInteractionUI.HideInteractionPanel();
                    }
                }
            }
        }

        // 아이템 획득 시도
        private void TryPickupItem() {
            if (currentTargetItem != null) {
                currentTargetItem.TryPickup();
                // 아이템 획득 후 UI 정리
                currentTargetItem = null;
                if (fieldItemInteractionUI != null) {
                    fieldItemInteractionUI.HideInteractionPanel();
                }
            }
        }

        // 아이템 획득 이벤트 핸들러
        private void OnItemPickedUp(ItemData item) {
            if (pickupNotification != null) {
                // 아이템 획득 알림 표시
                pickupNotification.ShowItemPickupNotification(item, 1);
            }
        }

        // 범위 시각화 (디버그용)
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
        }
    }
}