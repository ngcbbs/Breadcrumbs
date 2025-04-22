using System.Collections.Generic;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [System.Serializable]
    public class WeaponItemData {
        public string itemId;
        public string itemName;
        public string description;
        public Sprite icon;
        public int requiredLevel;
        public ClassType classType;
        public ItemRarity rarity;
        public int itemLevel;
        public WeaponType weaponType;
        public float baseDamage;
        public float attackSpeed;
        public float range;
        public bool isTwoHanded;
        public ElementType elementType = ElementType.None;
        public float elementalDamage = 0f;
        public GameObject attackEffect;
        public AudioClip attackSound;
        public GameObject itemModel;
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.white;
        public string setName;
        public List<StatData> stats = new List<StatData>();
        public List<SpecialEffectData> specialEffects = new List<SpecialEffectData>();
        
        public WeaponItem CreateWeaponItem() {
            WeaponItem weapon = new WeaponItem {
                itemId = this.itemId,
                itemName = this.itemName,
                description = this.description,
                icon = this.icon,
                equipSlot = isTwoHanded ? EquipmentSlot.TwoHand : EquipmentSlot.MainHand,
                requiredLevel = this.requiredLevel,
                classType = this.classType,
                itemLevel = this.itemLevel,
                rarity = this.rarity,
                weaponType = this.weaponType,
                baseDamage = this.baseDamage,
                attackSpeed = this.attackSpeed,
                range = this.range,
                isTwoHanded = this.isTwoHanded,
                elementType = this.elementType,
                elementalDamage = this.elementalDamage,
                attackEffect = this.attackEffect,
                attackSound = this.attackSound,
                itemModel = this.itemModel,
                primaryColor = this.primaryColor,
                secondaryColor = this.secondaryColor,
                setName = this.setName
            };
            
            // 스탯 추가
            foreach (var statData in stats) {
                weapon.stats.Add(new EquipmentItem.ItemStat {
                    statType = statData.statType,
                    value = statData.value,
                    type = statData.type
                });
            }
            
            // 특수 효과 추가
            foreach (var effectData in specialEffects) {
                weapon.specialEffects.Add(new EquipmentItem.SpecialEffect {
                    effectName = effectData.effectName,
                    effectDescription = effectData.effectDescription,
                    activationChance = effectData.activationChance,
                    visualEffect = effectData.visualEffect,
                    soundEffect = effectData.soundEffect
                });
            }
            
            return weapon;
        }
    }
}