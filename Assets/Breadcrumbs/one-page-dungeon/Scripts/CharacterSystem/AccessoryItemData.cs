using System.Collections.Generic;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [System.Serializable]
    public class AccessoryItemData {
        public string itemId;
        public string itemName;
        public string description;
        public Sprite icon;
        public EquipmentSlot equipSlot;
        public int requiredLevel;
        public ClassType classType;
        public ItemRarity rarity;
        public int itemLevel;
        public bool isUnique;
        public bool hasProc;
        public GameObject itemModel;
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.white;
        public string setName;
        public List<StatData> stats = new List<StatData>();
        public List<PassiveAbilityData> passiveAbilities = new List<PassiveAbilityData>();
        public List<SpecialEffectData> specialEffects = new List<SpecialEffectData>();
        
        public AccessoryItem CreateAccessoryItem() {
            AccessoryItem accessory = new AccessoryItem {
                itemId = this.itemId,
                itemName = this.itemName,
                description = this.description,
                icon = this.icon,
                equipSlot = this.equipSlot,
                requiredLevel = this.requiredLevel,
                classType = this.classType,
                itemLevel = this.itemLevel,
                rarity = this.rarity,
                isUnique = this.isUnique,
                hasProc = this.hasProc,
                itemModel = this.itemModel,
                primaryColor = this.primaryColor,
                secondaryColor = this.secondaryColor,
                setName = this.setName
            };
            
            // 스탯 추가
            foreach (var statData in stats) {
                accessory.stats.Add(new EquipmentItem.ItemStat {
                    statType = statData.statType,
                    value = statData.value,
                    type = statData.type
                });
            }
            
            // 패시브 능력 추가
            foreach (var abilityData in passiveAbilities) {
                accessory.passiveAbilities.Add(new AccessoryItem.PassiveAbility {
                    abilityName = abilityData.abilityName,
                    description = abilityData.description,
                    value = abilityData.value
                });
            }
            
            // 특수 효과 추가
            foreach (var effectData in specialEffects) {
                accessory.specialEffects.Add(new EquipmentItem.SpecialEffect {
                    effectName = effectData.effectName,
                    effectDescription = effectData.effectDescription,
                    activationChance = effectData.activationChance,
                    visualEffect = effectData.visualEffect,
                    soundEffect = effectData.soundEffect
                });
            }
            
            return accessory;
        }
    }
}