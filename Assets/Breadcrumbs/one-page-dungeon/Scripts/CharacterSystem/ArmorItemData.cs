using System.Collections.Generic;
using Breadcrumbs.Core;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [System.Serializable]
    public class ArmorItemData {
        public string itemId;
        public string itemName;
        public string description;
        public Sprite icon;
        public EquipmentSlot equipSlot;
        public int requiredLevel;
        public ClassType classType;
        public ItemRarity rarity;
        public int itemLevel;
        public ArmorType armorType;
        public float baseDefense;
        public float magicDefense;
        public float movementPenalty;
        public bool hasSetBonus;
        public GameObject defendEffect;
        public AudioClip defendSound;
        public GameObject itemModel;
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.white;
        public string setName;
        public List<StatData> stats = new List<StatData>();
        public List<SpecialEffectData> specialEffects = new List<SpecialEffectData>();
        
        public ArmorItem CreateArmorItem() {
            ArmorItem armor = new ArmorItem {
                ItemId = itemId,
                ItemName = itemName,
                Description = description,
                Icon = icon,
                EquipSlot = equipSlot,
                RequiredLevel = requiredLevel,
                ClassType = classType,
                ItemLevel = itemLevel,
                Rarity = rarity,
                ArmorType = armorType,
                BaseDefense = baseDefense,
                MagicDefense = magicDefense,
                MovementPenalty = movementPenalty,
                HasSetBonus = hasSetBonus,
                DefendEffect = defendEffect,
                DefendSound = defendSound,
                ItemModel = itemModel,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                SetName = setName
            };
            
            // 스탯 추가
            /*
            foreach (var statData in stats) {
                armor.stats.Add(new EquipmentItem.ItemStat {
                    statType = statData.statType,
                    value = statData.value,
                    type = statData.type
                });
            }
            // */
            
            // 특수 효과 추가
            /*
            foreach (var effectData in specialEffects) {
                armor.specialEffects.Add(new EquipmentItem.SpecialEffect {
                    effectName = effectData.effectName,
                    effectDescription = effectData.effectDescription,
                    activationChance = effectData.activationChance,
                    visualEffect = effectData.visualEffect,
                    soundEffect = effectData.soundEffect
                });
            }
            // */
            Debug.Log("fixme!");
            
            return armor;
        }
    }
}