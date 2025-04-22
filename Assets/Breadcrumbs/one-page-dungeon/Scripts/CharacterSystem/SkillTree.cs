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

        // SkillTree 클래스에 추가할 필드
        [Serializable]
        public class SkillPath {
            public string pathName;
            public string pathDescription;
            public List<string> pathNodeIds = new List<string>();
        }

        public List<SkillPath> skillPaths = new List<SkillPath>();

        // SkillTree 클래스의 GetPathProgress 메서드 구현
        public float GetPathProgress(string pathName) {
            // 해당 경로 찾기
            SkillPath path = skillPaths.Find(p => p.pathName == pathName);
            if (path == null || path.pathNodeIds.Count == 0) {
                Debug.LogWarning($"Path not found: {pathName}");
                return 0f;
            }

            // 해당 경로의 활성화된 노드 수 계산
            int activatedNodes = 0;
            foreach (string nodeId in path.pathNodeIds) {
                if (nodes.TryGetValue(nodeId, out SkillTreeNode node) && node.isUnlocked) {
                    activatedNodes++;
                }
            }

            // 진행도 계산 (활성화된 노드 수 / 전체 노드 수)
            return (float)activatedNodes / path.pathNodeIds.Count;
        }

        // 경로 노드의 평균 레벨 계산
        public float GetPathAverageLevel(string pathName) {
            SkillPath path = skillPaths.Find(p => p.pathName == pathName);
            if (path == null || path.pathNodeIds.Count == 0) {
                return 0f;
            }

            int totalLevels = 0;
            int unlockedNodes = 0;

            foreach (string nodeId in path.pathNodeIds) {
                if (nodes.TryGetValue(nodeId, out SkillTreeNode node) && node.isUnlocked) {
                    totalLevels += node.currentLevel;
                    unlockedNodes++;
                }
            }

            if (unlockedNodes == 0) return 0f;

            return (float)totalLevels / unlockedNodes;
        }

        // 특정 경로에서 다음 활성화 가능한 노드 찾기
        public List<SkillTreeNode> GetNextAvailableNodesInPath(string pathName, int availablePoints) {
            SkillPath path = skillPaths.Find(p => p.pathName == pathName);
            if (path == null) return new List<SkillTreeNode>();

            List<SkillTreeNode> availableNodes = new List<SkillTreeNode>();

            foreach (string nodeId in path.pathNodeIds) {
                if (nodes.TryGetValue(nodeId, out SkillTreeNode node) && !node.isUnlocked) {
                    if (node.CanActivate(nodes, availablePoints)) {
                        availableNodes.Add(node);
                    }
                }
            }

            return availableNodes;
        }

        // 경로의 핵심 노드 여부 확인
        public bool HasUnlockedKeyNodesInPath(string pathName, List<string> keyNodeIds) {
            SkillPath path = skillPaths.Find(p => p.pathName == pathName);
            if (path == null) return false;

            foreach (string nodeId in keyNodeIds) {
                if (nodes.TryGetValue(nodeId, out SkillTreeNode node) && node.isUnlocked) {
                    return true;
                }
            }

            return false;
        }

        // 특정 경로 완료 여부 확인
        public bool IsPathCompleted(string pathName) {
            return GetPathProgress(pathName) >= 1.0f;
        }

        // 경로 마스터리 레벨 계산 (모든 노드가 최대 레벨인지)
        public float GetPathMasteryLevel(string pathName) {
            SkillPath path = skillPaths.Find(p => p.pathName == pathName);
            if (path == null || path.pathNodeIds.Count == 0) {
                return 0f;
            }

            int totalLevels = 0;
            int totalMaxLevels = 0;

            foreach (string nodeId in path.pathNodeIds) {
                if (nodes.TryGetValue(nodeId, out SkillTreeNode node) && node.isUnlocked) {
                    totalLevels += node.currentLevel;
                    totalMaxLevels += node.maxLevel;
                }
            }

            if (totalMaxLevels == 0) return 0f;

            return (float)totalLevels / totalMaxLevels;
        }
    }
}