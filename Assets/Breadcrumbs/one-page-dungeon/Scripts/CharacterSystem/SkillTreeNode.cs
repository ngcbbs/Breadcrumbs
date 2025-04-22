using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class SkillTreeNode {
        public string nodeId;
        public string nodeName;
        public SkillData skill;
        public Vector2 position; // 트리에서의 위치

        // 필요 포인트 및 선행 노드
        public int requiredPoints;
        public List<string> requiredNodeIds = new List<string>();

        // 노드 상태
        public bool isUnlocked = false;
        public int currentLevel = 0;
        public int maxLevel = 1;

        // 노드 타입
        public enum NodeType {
            Active,    // 액티브 스킬
            Passive,   // 패시브 스킬
            Attribute, // 능력치 상승
            Mastery    // 특수 마스터리
        }

        public NodeType nodeType;

        // 노드 발동 효과 (스킬 습득, 특성 부여 등)
        public virtual void Activate(PlayerCharacter character) {
            if (isUnlocked) return;

            isUnlocked = true;
            currentLevel = 1;

            switch (nodeType) {
                case NodeType.Active:
                case NodeType.Passive:
                    if (skill != null) {
                        character.GetComponent<SkillManager>()?.AddSkill(skill);
                    }

                    break;

                case NodeType.Attribute:
                    // 능력치 보너스 적용
                    ApplyAttributeBonus(character);
                    break;

                case NodeType.Mastery:
                    // 특수 마스터리 적용
                    ApplyMasteryEffect(character);
                    break;
            }

            Debug.Log($"Activated skill tree node: {nodeName}");
        }

        // 노드 레벨업
        public virtual bool LevelUp(PlayerCharacter character) {
            if (!isUnlocked || currentLevel >= maxLevel)
                return false;

            currentLevel++;

            // 노드 타입에 따른 추가 효과
            switch (nodeType) {
                case NodeType.Active:
                case NodeType.Passive:
                    if (skill != null) {
                        character.GetComponent<SkillManager>()?.LevelUpSkill(skill.skillId);
                    }

                    break;

                case NodeType.Attribute:
                    // 추가 능력치 보너스 적용
                    ApplyAttributeBonus(character);
                    break;

                case NodeType.Mastery:
                    // 마스터리 효과 강화
                    ApplyMasteryEffect(character);
                    break;
            }

            Debug.Log($"Leveled up skill tree node: {nodeName} to level {currentLevel}");
            return true;
        }

        // 능력치 보너스 적용
        protected virtual void ApplyAttributeBonus(PlayerCharacter character) {
            // 자식 클래스에서 구현
            Debug.Log($"Applied attribute bonus from {nodeName}");
        }

        // 마스터리 효과 적용
        protected virtual void ApplyMasteryEffect(PlayerCharacter character) {
            // 자식 클래스에서 구현
            Debug.Log($"Applied mastery effect from {nodeName}");
        }

        // 노드 활성화 가능 여부 확인
        public bool CanActivate(Dictionary<string, SkillTreeNode> allNodes, int availablePoints) {
            // 이미 활성화된 경우
            if (isUnlocked)
                return false;

            // 포인트 부족
            if (availablePoints < requiredPoints)
                return false;

            // 선행 노드 확인
            foreach (string requiredId in requiredNodeIds) {
                if (allNodes.TryGetValue(requiredId, out SkillTreeNode node)) {
                    if (!node.isUnlocked)
                        return false;
                } else {
                    Debug.LogWarning($"Required node {requiredId} not found");
                    return false;
                }
            }

            return true;
        }

        // 레벨업 가능 여부 확인
        public bool CanLevelUp(int availablePoints) {
            return isUnlocked && currentLevel < maxLevel && availablePoints >= requiredPoints;
        }
    }
}