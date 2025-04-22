using Breadcrumbs.CharacterSystem;
using UnityEngine;

namespace Breadcrumbs.ItemSystem {
    public class ItemSystemExample : MonoBehaviour {
        [SerializeField]
        private ItemDatabaseImpl itemDatabase;
        [SerializeField]
        private PlayerCharacter targetCharacter;

        [Header("테스트 설정")]
        [SerializeField]
        private int itemLevel = 10;
        [SerializeField]
        private ItemRarity minRarity = ItemRarity.Uncommon;

        private void Start() {
            // 아이템 서비스 초기화
            ItemService.Instance.Initialize(itemDatabase);

            // 예시: 무작위 아이템 생성
            CreateAndEquipRandomItems();
        }

        // 무작위 아이템 생성 및 장착 예시
        public void CreateAndEquipRandomItems() {
            // 무작위 무기 생성
            WeaponItem weapon = ItemService.Instance.CreateRandomWeapon(itemLevel, minRarity);
            Debug.Log($"Created weapon: {weapon.ItemName}, Damage: {weapon.BaseDamage}, Score: {weapon.CalculateItemScore()}");

            // 무작위 방어구 생성
            ArmorItem armor = ItemService.Instance.CreateRandomArmor(itemLevel, minRarity);
            Debug.Log($"Created armor: {armor.ItemName}, Defense: {armor.BaseDefense}, Score: {armor.CalculateItemScore()}");

            // 무작위 액세서리 생성
            AccessoryItem accessory = ItemService.Instance.CreateRandomAccessory(itemLevel, minRarity);
            Debug.Log($"Created accessory: {accessory.ItemName}, Score: {accessory.CalculateItemScore()}");

            // 아이템 장착 가능 여부 확인 및 장착
            if (targetCharacter != null) {
                if (weapon.CanEquip(targetCharacter)) {
                    targetCharacter.EquipItem(weapon);
                    Debug.Log($"Equipped weapon: {weapon.ItemName}");
                }

                if (armor.CanEquip(targetCharacter)) {
                    targetCharacter.EquipItem(armor);
                    Debug.Log($"Equipped armor: {armor.ItemName}");
                }

                if (accessory.CanEquip(targetCharacter)) {
                    targetCharacter.EquipItem(accessory);
                    Debug.Log($"Equipped accessory: {accessory.ItemName}");
                }

                // 세트 효과 적용
                ItemService.Instance.ApplySetBonuses(targetCharacter);
            }
        }

        // 아이템 복제 및 변형 예시
        public void CloneAndModifyItem() {
            // 기존 아이템 복제
            WeaponItem originalWeapon = ItemService.Instance.CreateRandomWeapon(itemLevel, minRarity);
            WeaponItem clonedWeapon = (WeaponItem)originalWeapon.Clone();

            // 복제된 아이템 수정
            clonedWeapon.ItemName = $"Modified {clonedWeapon.ItemName}";
            clonedWeapon.BaseDamage *= 1.2f; // 20% 데미지 증가

            // 효과 추가
            clonedWeapon.AddEffect(new ElementalDamageEffect(ElementType.Fire, 0.15f));

            Debug.Log($"Original weapon: {originalWeapon.ItemName}, Damage: {originalWeapon.BaseDamage}");
            Debug.Log($"Modified weapon: {clonedWeapon.ItemName}, Damage: {clonedWeapon.BaseDamage}");
        }
    }
}