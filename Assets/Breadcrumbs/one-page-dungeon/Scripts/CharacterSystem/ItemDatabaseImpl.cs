using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "RPG/Item Database")]
    public class ItemDatabaseImpl : ScriptableObject, ItemDatabase {
        [SerializeField] private List<WeaponItemData> weapons = new List<WeaponItemData>();
        [SerializeField] private List<ArmorItemData> armors = new List<ArmorItemData>();
        [SerializeField] private List<AccessoryItemData> accessories = new List<AccessoryItemData>();
        
        // 캐시 딕셔너리
        private Dictionary<string, EquipmentItem> itemsById = null;
        
        private void OnEnable() {
            InitializeDatabase();
        }
        
        private void InitializeDatabase() {
            itemsById = new Dictionary<string, EquipmentItem>();
            
            // 무기 등록
            foreach (var weapon in weapons) {
                if (!string.IsNullOrEmpty(weapon.itemId)) {
                    itemsById[weapon.itemId] = weapon.CreateWeaponItem();
                }
            }
            
            // 방어구 등록
            foreach (var armor in armors) {
                if (!string.IsNullOrEmpty(armor.itemId)) {
                    itemsById[armor.itemId] = armor.CreateArmorItem();
                }
            }
            
            // 액세서리 등록
            foreach (var accessory in accessories) {
                if (!string.IsNullOrEmpty(accessory.itemId)) {
                    itemsById[accessory.itemId] = accessory.CreateAccessoryItem();
                }
            }
            
            Debug.Log($"Item database initialized with {itemsById.Count} items");
        }
        
        public EquipmentItem GetItemById(string itemId) {
            if (itemsById == null) {
                InitializeDatabase();
            }
            
            if (itemsById.TryGetValue(itemId, out EquipmentItem item)) {
                return item;
            }
            
            Debug.LogWarning($"Item not found with ID: {itemId}");
            return null;
        }
        
        // 아이템 유형별 검색
        public List<EquipmentItem> GetItemsByClass(ClassType classType) {
            List<EquipmentItem> results = new List<EquipmentItem>();
            
            foreach (var item in itemsById.Values) {
                if (item.classType == classType || item.classType == ClassType.None) {
                    results.Add(item);
                }
            }
            
            return results;
        }
        
        public List<EquipmentItem> GetItemsByLevel(int maxLevel) {
            List<EquipmentItem> results = new List<EquipmentItem>();
            
            foreach (var item in itemsById.Values) {
                if (item.requiredLevel <= maxLevel) {
                    results.Add(item);
                }
            }
            
            return results;
        }
        
        public List<EquipmentItem> GetItemsByRarity(ItemRarity rarity) {
            List<EquipmentItem> results = new List<EquipmentItem>();
            
            foreach (var item in itemsById.Values) {
                if (item.rarity == rarity) {
                    results.Add(item);
                }
            }
            
            return results;
        }
    }
}