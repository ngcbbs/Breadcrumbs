using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    public class SkillTreeManager : MonoBehaviour {
        [SerializeField]
        private PlayerCharacter character;
        [SerializeField]
        private List<SkillTree> skillTrees = new List<SkillTree>();

        private Dictionary<string, SkillTree> treesByName = new Dictionary<string, SkillTree>();
        private int availableSkillPoints = 0;

        // 스킬 포인트 획득 이벤트
        public delegate void SkillPointsChangedHandler(int points);
        public event SkillPointsChangedHandler OnSkillPointsChanged;

        // 노드 활성화 이벤트
        public delegate void NodeActivatedHandler(SkillTreeNode node);
        public event NodeActivatedHandler OnNodeActivated;

        private void Start() {
            InitializeSkillTrees();
        }

        // 스킬 트리 초기화
        private void InitializeSkillTrees() {
            treesByName.Clear();

            foreach (var tree in skillTrees) {
                tree.Initialize();
                treesByName[tree.treeName] = tree;
            }

            Debug.Log($"Initialized {skillTrees.Count} skill trees");
        }

        // 스킬 포인트 추가
        public void AddSkillPoints(int points) {
            availableSkillPoints += points;
            OnSkillPointsChanged?.Invoke(availableSkillPoints);

            Debug.Log($"Added {points} skill points. Total: {availableSkillPoints}");
        }

        // 현재 사용 가능한 스킬 포인트
        public int GetAvailableSkillPoints() {
            return availableSkillPoints;
        }

        // 노드 활성화
        public bool ActivateNode(string treeName, string nodeId) {
            if (treesByName.TryGetValue(treeName, out SkillTree tree)) {
                if (tree.ActivateNode(nodeId, character, availableSkillPoints)) {
                    // 스킬 포인트 차감
                    if (tree.nodes.TryGetValue(nodeId, out SkillTreeNode node)) {
                        availableSkillPoints -= node.requiredPoints;
                        OnSkillPointsChanged?.Invoke(availableSkillPoints);
                        OnNodeActivated?.Invoke(node);
                        return true;
                    }
                }
            }

            return false;
        }

        // 노드 레벨업
        public bool LevelUpNode(string treeName, string nodeId) {
            if (treesByName.TryGetValue(treeName, out SkillTree tree)) {
                if (tree.nodes.TryGetValue(nodeId, out SkillTreeNode node)) {
                    if (tree.LevelUpNode(nodeId, character, availableSkillPoints)) {
                        // 스킬 포인트 차감
                        availableSkillPoints -= node.requiredPoints;
                        OnSkillPointsChanged?.Invoke(availableSkillPoints);
                        OnNodeActivated?.Invoke(node);
                        return true;
                    }
                }
            }

            return false;
        }

        // 활성화 가능한 모든 노드 가져오기
        public List<SkillTreeNode> GetAvailableNodes(string treeName) {
            if (treesByName.TryGetValue(treeName, out SkillTree tree)) {
                return tree.GetAvailableNodes(availableSkillPoints);
            }

            return new List<SkillTreeNode>();
        }

        // 모든 스킬 트리에서 활성화 가능한 노드 가져오기
        public Dictionary<string, List<SkillTreeNode>> GetAllAvailableNodes() {
            Dictionary<string, List<SkillTreeNode>> result = new Dictionary<string, List<SkillTreeNode>>();

            foreach (var tree in treesByName) {
                result[tree.Key] = tree.Value.GetAvailableNodes(availableSkillPoints);
            }

            return result;
        }

        // 스킬 트리 정보 출력 (디버그용)
        public void PrintSkillTreeInfo() {
            Debug.Log($"===== Skill Tree Information =====");
            Debug.Log($"Available Skill Points: {availableSkillPoints}");

            foreach (var tree in skillTrees) {
                int unlockedCount = tree.GetUnlockedNodeCount();
                int totalCount = tree.nodes.Count;

                Debug.Log($"Tree: {tree.treeName} - Progress: {unlockedCount}/{totalCount} nodes unlocked");

                // 활성화 가능한 노드 표시
                List<SkillTreeNode> availableNodes = tree.GetAvailableNodes(availableSkillPoints);
                if (availableNodes.Count > 0) {
                    Debug.Log("Available Nodes:");
                    foreach (var node in availableNodes) {
                        Debug.Log($"  - {node.nodeName} (Cost: {node.requiredPoints})");
                    }
                }
            }

            Debug.Log($"=================================");
        }
    }
}