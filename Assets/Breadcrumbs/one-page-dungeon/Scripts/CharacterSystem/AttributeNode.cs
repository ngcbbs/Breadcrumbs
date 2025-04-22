using System;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class AttributeNode : SkillTreeNode {
        public StatType attributeType;
        public float bonusPerLevel;

        protected override void ApplyAttributeBonus(PlayerCharacter character) {
            float bonus = bonusPerLevel * currentLevel;

            // 능력치에 보너스 적용
            StatModifier mod = new StatModifier(bonus, StatModifierType.Flat, this);
            character.Stats.AddModifier(attributeType, mod);

            Debug.Log($"Applied {bonus} to {attributeType} from attribute node {nodeName}");
        }
    }
}