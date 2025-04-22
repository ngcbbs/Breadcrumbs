using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    [Serializable]
    public class SkillTree {
        public string treeName;
        public ClassType classType; // 특정 직업 전용 트리
        public Dictionary<string, SkillTreeNode> nodes = new Dictionary<string, SkillTreeNode>();

        // 에디터 지원용 노드 리스트
        public List<SkillTreeNode> nodeList = new List<SkillTreeNode>();

        // 시작점 노드들
        public List<string> rootNodeIds = new List<string>();

        // 초기화
        public void Initialize() {
            // 노드 리스트를 딕셔너리로 변환
            nodes.Clear();
            foreach (var node in nodeList) {
                nodes[node.nodeId] = node;
            }

            Debug.Log($"Initialized skill tree: {treeName} with {nodes.Count} nodes");
        }

        // 활성화 가능한 모든 노드 가져오기
        public List<SkillTreeNode> GetAvailableNodes(int availablePoints) {
            List<SkillTreeNode> availableNodes = new List<SkillTreeNode>();

            foreach (var node in nodes.Values) {
                if (node.CanActivate(nodes, availablePoints)) {
                    availableNodes.Add(node);
                }
            }

            return availableNodes;
        }

        // 레벨업 가능한 모든 노드 가져오기
        public List<SkillTreeNode> GetUpgradableNodes(int availablePoints) {
            List<SkillTreeNode> upgradableNodes = new List<SkillTreeNode>();

            foreach (var node in nodes.Values) {
                if (node.CanLevelUp(availablePoints)) {
                    upgradableNodes.Add(node);
                }
            }

            return upgradableNodes;
        }

        // 노드 활성화
        public bool ActivateNode(string nodeId, PlayerCharacter character, int availablePoints) {
            if (nodes.TryGetValue(nodeId, out SkillTreeNode node)) {
                if (node.CanActivate(nodes, availablePoints)) {
                    node.Activate(character);
                    return true;
                }
            }

            return false;
        }

        // 노드 레벨업
        public bool LevelUpNode(string nodeId, PlayerCharacter character, int availablePoints) {
            if (nodes.TryGetValue(nodeId, out SkillTreeNode node)) {
                if (node.CanLevelUp(availablePoints)) {
                    return node.LevelUp(character);
                }
            }

            return false;
        }

        // 활성화된 노드 수 가져오기
        public int GetUnlockedNodeCount() {
            int count = 0;
            foreach (var node in nodes.Values) {
                if (node.isUnlocked) {
                    count++;
                }
            }

            return count;
        }

        // 특정 경로의 진행도 계산 (예: "화염 마법" 경로)
        public float GetPathProgress(string pathName) {
            // 실제 구현에서는 경로별 노드를 미리 정의하고 진행도 계산
            // 임시 구현
            return 0.5f;
        }
    }
}