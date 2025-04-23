using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Character.Skills
{
    /// <summary>
    /// 스킬 트리를 정의하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill Tree", menuName = "Breadcrumbs/Skills/Skill Tree")]
    public class SkillTree : ScriptableObject
    {
        [SerializeField] private string treeId;
        [SerializeField] private string treeName;
        [SerializeField] private ClassType classRestriction;
        [SerializeField] private List<SkillTreeNode> nodes = new List<SkillTreeNode>();
        
        // 프로퍼티
        public string TreeId => treeId;
        public string TreeName => treeName;
        public ClassType ClassRestriction => classRestriction;
        public IReadOnlyList<SkillTreeNode> Nodes => nodes;
        
        // 런타임 필드
        private Dictionary<int, SkillTreeNode> nodeMap;
        private Dictionary<string, SkillTreeNode> skillMap;
        
        /// <summary>
        /// 스킬 트리를 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            nodeMap = new Dictionary<int, SkillTreeNode>();
            skillMap = new Dictionary<string, SkillTreeNode>();
            
            foreach (var node in nodes)
            {
                nodeMap[node.NodeId] = node;
                
                if (!string.IsNullOrEmpty(node.SkillId))
                {
                    skillMap[node.SkillId] = node;
                }
            }
        }
        
        /// <summary>
        /// ID로 노드를 가져옵니다.
        /// </summary>
        public SkillTreeNode GetNode(int nodeId)
        {
            if (nodeMap == null)
            {
                Initialize();
            }
            
            if (nodeMap.TryGetValue(nodeId, out SkillTreeNode node))
            {
                return node;
            }
            
            return null;
        }
        
        /// <summary>
        /// 스킬 ID로 노드를 가져옵니다.
        /// </summary>
        public SkillTreeNode GetNodeBySkill(string skillId)
        {
            if (skillMap == null)
            {
                Initialize();
            }
            
            if (skillMap.TryGetValue(skillId, out SkillTreeNode node))
            {
                return node;
            }
            
            return null;
        }
        
        /// <summary>
        /// 노드가 잠금 해제 가능한지 확인합니다.
        /// </summary>
        public bool CanUnlockNode(int nodeId, IReadOnlyDictionary<int, bool> unlockedNodes, int availablePoints)
        {
            SkillTreeNode node = GetNode(nodeId);
            if (node == null)
            {
                return false;
            }
            
            return node.CanUnlock(unlockedNodes, availablePoints);
        }
        
        /// <summary>
        /// 스킬 트리의 유효성을 검사합니다.
        /// </summary>
        private void OnValidate()
        {
            // treeId가 비어있으면 자동으로 생성
            if (string.IsNullOrEmpty(treeId))
            {
                treeId = Guid.NewGuid().ToString();
            }
            
            // 노드 유효성 검사
            HashSet<int> nodeIds = new HashSet<int>();
            
            foreach (var node in nodes)
            {
                if (nodeIds.Contains(node.NodeId))
                {
                    Debug.LogWarning($"중복된 노드 ID: {node.NodeId}");
                }
                else
                {
                    nodeIds.Add(node.NodeId);
                }
                
                // 선행 노드 유효성 검사
                foreach (int prereqId in node.Prerequisites)
                {
                    if (!nodeIds.Contains(prereqId))
                    {
                        Debug.LogWarning($"노드 {node.NodeId}의 선행 노드 {prereqId}가 존재하지 않습니다.");
                    }
                }
            }
        }
    }
}
