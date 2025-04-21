using System;
using System.Collections;
using System.Collections.Generic;
using Breadcrumbs.InventorySystem;
using UnityEngine;
using Breadcrumbs.ItemSystem;

namespace Breadcrumbs.LootingSystem {

    // 아이템 드롭 관리자
    public class ItemDropManager : MonoBehaviour, INetworkSyncableLooting {
        // 싱글톤 인스턴스
        public static ItemDropManager Instance { get; private set; }

        // 드롭된 아이템 프리팹
        [SerializeField]
        private GameObject fieldItemPrefab;

        // 드롭된 아이템 추적 (인스턴스 ID -> 필드 아이템)
        private Dictionary<int, FieldItem> activeFieldItems = new Dictionary<int, FieldItem>();

        // 인스턴스 ID 카운터
        private int nextInstanceId = 0;

        // 던전 난이도 설정
        [SerializeField]
        private DungeonDifficulty currentDifficulty = DungeonDifficulty.Normal;

        // 트랩 연동 시스템 관련 변수 (선택 사항)
        [SerializeField]
        private float trapDropRateMultiplier = 1.5f;
        private bool isTrapDropRateActive = false;
        private float trapDropRateTimer = 0f; // 트랩 효과 지속 시간

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
                return;
            }
        }

        private void Update() {
            // 트랩 드롭률 증가 효과 타이머 처리 (선택 사항)
            if (isTrapDropRateActive) {
                trapDropRateTimer -= Time.deltaTime;
                if (trapDropRateTimer <= 0) {
                    isTrapDropRateActive = false;
                }
            }
        }

        // 아이템 드롭 (몬스터 사망 시 호출)
        public void DropItemsFromMonster(Vector3 position, DropTable dropTable) {
            if (dropTable == null) return;

            // 현재 난이도로 드롭 아이템 결정
            float dropMultiplier = isTrapDropRateActive ? trapDropRateMultiplier : 1f;
            List<(ItemData item, int quantity)> droppedItems;

            if (isTrapDropRateActive) {
                droppedItems = dropTable.RollDropsWithTrapBonus(currentDifficulty, dropMultiplier);
            } else {
                droppedItems = dropTable.RollDrops(currentDifficulty);
            }

            // 드롭된 아이템마다 필드에 생성
            foreach (var drop in droppedItems) {
                DropItem(position, drop.item, drop.quantity);
            }
        }

        // 개별 아이템 드롭
        public FieldItem DropItem(Vector3 position, ItemData item, int quantity) {
            if (item == null || quantity <= 0 || fieldItemPrefab == null) return null;

            // 드롭 위치 약간 랜덤화 (아이템이 겹치지 않도록)
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                0,
                UnityEngine.Random.Range(-0.5f, 0.5f)
            );

            // 필드 아이템 생성
            GameObject itemObj = Instantiate(fieldItemPrefab, position + randomOffset, Quaternion.identity);
            FieldItem fieldItem = itemObj.GetComponent<FieldItem>();

            if (fieldItem != null) {
                // 아이템 정보 설정
                fieldItem.itemData = item;
                fieldItem.quantity = quantity;
                fieldItem.instanceId = GenerateNewInstanceId();

                // 활성 아이템 딕셔너리에 추가
                activeFieldItems[fieldItem.instanceId] = fieldItem;
            }

            return fieldItem;
        }

        // 새 인스턴스 ID 생성
        private int GenerateNewInstanceId() {
            return nextInstanceId++;
        }

        // 트랩 드롭률 증가 효과 활성화 (선택 사항)
        public void ActivateTrapDropRateBonus(float duration) {
            isTrapDropRateActive = true;
            trapDropRateTimer = duration;

            Debug.Log($"트랩 효과로 인한 드롭률 증가 활성화! 지속시간: {duration}초");
        }

        // 난이도 설정
        public void SetDungeonDifficulty(DungeonDifficulty difficulty) {
            currentDifficulty = difficulty;
            Debug.Log($"던전 난이도가 {difficulty}로 설정되었습니다.");
        }

        #region INetworkSyncableLooting 인터페이스 구현

        // 아이템 드롭 동기화
        public void SyncItemDrop(Vector3 position, ItemData item, int quantity) {
            // 네트워크를 통해 다른 클라이언트에게 아이템 드롭 정보 전달
            DropItem(position, item, quantity);
        }

        // 아이템 획득 동기화
        public void SyncItemPickup(int itemInstanceId, IItemOwner owner) {
            // 네트워크를 통해 다른 클라이언트에게 아이템 획득 정보 전달
            if (activeFieldItems.TryGetValue(itemInstanceId, out FieldItem fieldItem)) {
                if (owner is PlayerInventory inventory) {
                    // 아이템 정보 추출
                    ItemData item = fieldItem.itemData;
                    int quantity = fieldItem.quantity;

                    // 인벤토리에 아이템 추가
                    if (inventory.AddItem(item, quantity)) {
                        // 필드에서 아이템 제거
                        activeFieldItems.Remove(itemInstanceId);
                        Destroy(fieldItem.gameObject);
                    }
                }
            }
        }

        #endregion
    }
}