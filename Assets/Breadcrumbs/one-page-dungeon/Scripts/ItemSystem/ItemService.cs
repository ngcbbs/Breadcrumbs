using System.Collections.Generic;
using Breadcrumbs.CharacterSystem;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    /// <summary>
    /// 아이템 서비스: 전체 아이템 시스템의 중앙 관리 지점
    /// </summary>
    internal class ItemService {
        private ItemDatabaseImpl itemDatabase;

        // 타입별 아이템 저장소
        private ItemRepository<WeaponItem> weaponRepository = new ItemRepository<WeaponItem>();
        private ItemRepository<ArmorItem> armorRepository = new ItemRepository<ArmorItem>();
        private ItemRepository<AccessoryItem> accessoryRepository = new ItemRepository<AccessoryItem>();

        // 팩토리 인스턴스
        private WeaponFactory weaponFactory;
        private ArmorFactory armorFactory;
        private AccessoryFactory accessoryFactory;

        // 싱글톤 인스턴스
        private static ItemService instance;
        public static ItemService Instance {
            get {
                if (instance == null) {
                    instance = new ItemService();
                }

                return instance;
            }
        }

        // 초기화
        public void Initialize(ItemDatabaseImpl database) {
            this.itemDatabase = database;

            // 팩토리 초기화
            weaponFactory = new WeaponFactory(itemDatabase);
            armorFactory = new ArmorFactory(itemDatabase);
            accessoryFactory = new AccessoryFactory(itemDatabase);

            // 기존 아이템 데이터베이스에서 아이템 로드
            LoadItemsFromDatabase();
        }

        private void LoadItemsFromDatabase() {
            if (itemDatabase == null) {
                Debug.LogError("Item database not initialized");
                return;
            }

            // 아이템 데이터베이스 내의 모든 아이템 조회 및 등록
            foreach (var item in itemDatabase.GetAllItems()) {
                RegisterItem(item);
            }
        }

        // 아이템 등록
        public void RegisterItem(EquipmentItem item) {
            if (item == null)
                return;

            if (item is WeaponItem weapon) {
                weaponRepository.RegisterItem(weapon);
            } else if (item is ArmorItem armor) {
                armorRepository.RegisterItem(armor);
            } else if (item is AccessoryItem accessory) {
                accessoryRepository.RegisterItem(accessory);
            }
        }

        // 아이템 ID로 검색
        public EquipmentItem GetItemById(string itemId) {
            EquipmentItem item = weaponRepository.GetById(itemId);
            if (item != null)
                return item;

            item = armorRepository.GetById(itemId);
            if (item != null)
                return item;

            return accessoryRepository.GetById(itemId);
        }

        // 랜덤 아이템 생성
        public EquipmentItem CreateRandomItem(int itemLevel, ItemRarity minRarity = ItemRarity.Common) {
            // 랜덤하게 아이템 타입 결정
            int itemType = UnityEngine.Random.Range(0, 3);

            // 선택된 타입의 아이템 생성
            switch (itemType) {
                case 0:
                    return weaponFactory.CreateRandomItem(itemLevel, minRarity);
                case 1:
                    return armorFactory.CreateRandomItem(itemLevel, minRarity);
                case 2:
                    return accessoryFactory.CreateRandomItem(itemLevel, minRarity);
                default:
                    return weaponFactory.CreateRandomItem(itemLevel, minRarity);
            }
        }

        // 특정 무기 생성
        public WeaponItem CreateWeapon(string itemId) {
            return weaponFactory.CreateItem(itemId);
        }

        public WeaponItem CreateRandomWeapon(int itemLevel, ItemRarity minRarity = ItemRarity.Common) {
            return weaponFactory.CreateRandomItem(itemLevel, minRarity);
        }

        // 특정 방어구 생성
        public ArmorItem CreateArmor(string itemId) {
            return armorFactory.CreateItem(itemId);
        }

        public ArmorItem CreateRandomArmor(int itemLevel, ItemRarity minRarity = ItemRarity.Common) {
            return armorFactory.CreateRandomItem(itemLevel, minRarity);
        }

        // 특정 액세서리 생성
        public AccessoryItem CreateAccessory(string itemId) {
            return accessoryFactory.CreateItem(itemId);
        }

        public AccessoryItem CreateRandomAccessory(int itemLevel, ItemRarity minRarity = ItemRarity.Common) {
            return accessoryFactory.CreateRandomItem(itemLevel, minRarity);
        }

        // 아이템 장착 검증
        public bool CanEquipItem(PlayerCharacter character, EquipmentItem item) {
            return item.CanEquip(character);
        }

        // 장비 세트 효과 적용
        public void ApplySetBonuses(PlayerCharacter character) {
            // 캐릭터가 현재 장착한 모든 장비에서 세트 이름 추출
            Dictionary<string, int> setCounts = new Dictionary<string, int>();

            // todo: 장비 세트 집계
            /*
            foreach (var item in character.GetEquippedItems()) {
                if (!string.IsNullOrEmpty(item.SetName)) {
                    if (!setCounts.ContainsKey(item.SetName)) {
                        setCounts[item.SetName] = 0;
                    }

                    setCounts[item.SetName]++;
                }
            }
            // */

            // 세트 효과 적용
            foreach (var setCount in setCounts) {
                // 세트 효과 정보는 별도 데이터 클래스나 DB에서 가져와야 함
                // ApplySetEffect(character, setCount.Key, setCount.Value);
            }
        }
    }
}