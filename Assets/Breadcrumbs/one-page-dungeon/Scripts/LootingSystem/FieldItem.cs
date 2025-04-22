using Breadcrumbs.CharacterSystem;
using Breadcrumbs.ItemSystem;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.LootingSystem {
    public class FieldItem : MonoBehaviour, INetworkSyncableItem {
        // 고유 ID (네트워크 동기화용)
        public int instanceId;

        // 아이템 정보
        public ItemData itemData;
        public int quantity = 1;

        // 시각 효과
        [SerializeField]
        private SpriteRenderer spriteRenderer;
        [SerializeField]
        private GameObject glowEffect;
        [SerializeField]
        private GameObject pickupPrompt;
        [SerializeField]
        private float floatSpeed = 1f;
        [SerializeField]
        private float floatAmplitude = 0.1f;
        [SerializeField]
        private float rotateSpeed = 50f;

        // 자동 획득 설정
        [SerializeField]
        private bool isAutoPickup = false;
        [SerializeField]
        private float autoPickupRange = 2f;

        // 아이템 생존 시간 (필드에 남아있는 시간)
        [SerializeField]
        private float lifetime = 300f; // 기본 5분
        private float lifetimeCounter;

        // 획득 가능 여부
        private bool canPickup = false;

        // 컴포넌트 참조
        private Collider itemCollider;
        private Scripts.Player player; // 플레이어 클래스 참조 (실제 구현 시 필요)

        private void Awake() {
            itemCollider = GetComponent<Collider>();
            pickupPrompt.SetActive(false);
        }

        private void Start() {
            // 아이템 기본 설정
            lifetimeCounter = lifetime;

            // 아이템 타입에 따라 자동 획득 설정
            if (itemData != null) {
                isAutoPickup = itemData.isAutoPickup;

                // 아이콘 설정
                if (spriteRenderer != null && itemData.icon != null) {
                    spriteRenderer.sprite = itemData.icon;
                }

                // 희귀도에 따른 외곽선 색상 설정
                SetRarityVisuals();
            }
        }

        private void Update() {
            // 드롭된 아이템 시각 효과 (부드럽게 떠 있는 효과)
            transform.position += new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * floatAmplitude * Time.deltaTime, 0);
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

            // 수명 카운트다운
            lifetimeCounter -= Time.deltaTime;
            if (lifetimeCounter <= 0) {
                Destroy(gameObject);
            }

            // 자동 획득 체크
            if (isAutoPickup && player != null) {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= autoPickupRange) {
                    TryPickup();
                }
            }
        }

        // 희귀도에 따른 시각적 효과 설정
        private void SetRarityVisuals() {
            if (itemData == null || glowEffect == null) return;

            // 희귀도별 색상 설정
            Color rarityColor = itemData.GetRarityColor();

            // Material 속성 변경
            Material glowMaterial = glowEffect.GetComponent<Renderer>()?.material;
            if (glowMaterial != null) {
                glowMaterial.SetColor("_EmissionColor", rarityColor);
                glowMaterial.color = rarityColor;
            }

            // 희귀도에 따라 외곽선 강도 조절
            float glowIntensity = 0f;
            switch (itemData.rarity) {
                case ItemRarity.Common:
                    glowIntensity = 0.5f;
                    break;
                case ItemRarity.Uncommon:
                    glowIntensity = 1f;
                    break;
                case ItemRarity.Rare:
                    glowIntensity = 1.5f;
                    break;
                case ItemRarity.Epic:
                    glowIntensity = 2f;
                    break;
                case ItemRarity.Legendary:
                    glowIntensity = 3f;
                    // 전설 아이템은 추가 파티클 효과 활성화 가능
                    break;
            }

            // 발광 효과 강도 조절
            if (glowMaterial != null) {
                glowMaterial.SetFloat("_Intensity", glowIntensity);
            }
        }

        // 접근 가능 상태 진입
        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player")) {
                player = other.GetComponent<Scripts.Player>();

                if (player != null) {
                    canPickup = true;
                    pickupPrompt.SetActive(!isAutoPickup); // 자동 획득이 아닌 경우에만 프롬프트 표시
                }
            }
        }

        // 접근 가능 상태 종료
        private void OnTriggerExit(Collider other) {
            if (other.CompareTag("Player") && other.GetComponent<Scripts.Player>() == player) {
                canPickup = false;
                pickupPrompt.SetActive(false);
                player = null;
            }
        }

        // 아이템 획득 시도
        public void TryPickup() {
            if (!canPickup || player == null) return;

            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null) {
                if (inventory.CanOwnItem(itemData, quantity)) {
                    if (inventory.AddItem(itemData, quantity)) {
                        // 아이템 획득 효과 및 알림
                        ShowPickupEffect();

                        // 아이템 객체 제거
                        Destroy(gameObject);
                    } else {
                        Debug.Log("아이템을 획득할 수 없습니다. 인벤토리가 가득 찼습니다.");
                    }
                } else {
                    Debug.Log("인벤토리 공간이 부족합니다.");
                }
            }
        }

        // 아이템 획득 효과 표시
        private void ShowPickupEffect() {
            // 아이템 획득 사운드 재생
            // AudioManager.Instance.PlaySFX("ItemPickup");

            // 획득 텍스트 표시
            // UI_FloatingTextManager.Instance.ShowFloatingText($"+{itemData.itemName} x{quantity}", transform.position, itemData.GetRarityColor());

            // 희귀도에 따른 추가 효과 (레어 이상)
            if (itemData.rarity >= ItemRarity.Rare) {
                // 특별 획득 효과 표시
                // EffectManager.Instance.PlayEffect("RareItemPickup", transform.position);
            }
        }

        // 네트워크 동기화 상태 가져오기
        public object GetSyncState() {
            return new FieldItemSyncData {
                instanceId = this.instanceId,
                itemId = itemData.itemId,
                quantity = this.quantity,
                position = transform.position,
                rotation = transform.rotation.eulerAngles,
                remainingLifetime = this.lifetimeCounter
            };
        }

        // 네트워크 동기화 상태 적용
        public void ApplySyncState(object state) {
            if (state is FieldItemSyncData data) {
                this.instanceId = data.instanceId;

                // 아이템 데이터 설정 (실제 구현 시 ItemDatabase 사용)
                // this.itemData = ItemDatabase.Instance.GetItemById(data.itemId);

                this.quantity = data.quantity;
                transform.position = data.position;
                transform.rotation = Quaternion.Euler(data.rotation);
                this.lifetimeCounter = data.remainingLifetime;

                // 시각적 효과 업데이트
                if (this.itemData != null) {
                    SetRarityVisuals();
                }
            }
        }

        // 필드 아이템 동기화 데이터
        [System.Serializable]
        public class FieldItemSyncData {
            public int instanceId;
            public string itemId;
            public int quantity;
            public Vector3 position;
            public Vector3 rotation;
            public float remainingLifetime;
        }
    }
}