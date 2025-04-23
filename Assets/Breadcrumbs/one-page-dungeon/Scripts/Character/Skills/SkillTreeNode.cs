using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Character.Skills
{
    /// <summary>
    /// 스킬 트리의 노드를 나타내는 클래스
    /// </summary>
    [Serializable]
    public class SkillTreeNode
    {
        [SerializeField] private int nodeId;
        [SerializeField] private string skillId;
        [SerializeField] private int requiredPoints;
        [SerializeField] private List<int> prerequisites = new List<int>();
        [SerializeField] private Vector2 position;
        
        // 런타임 참조
        private SkillDefinition skillDefinition;
        
        // 프로퍼티
        public int NodeId => nodeId;
        public string SkillId => skillId;
        public int RequiredPoints => requiredPoints;
        public IReadOnlyList<int> Prerequisites => prerequisites;
        public Vector2 Position => position;
        public SkillDefinition SkillDefinition => skillDefinition;
        
        /// <summary>
        /// 스킬 정의를 설정합니다.
        /// </summary>
        public void SetSkillDefinition(SkillDefinition definition)
        {
            if (definition != null && definition.SkillId == skillId)
            {
                skillDefinition = definition;
            }
        }
        
        /// <summary>
        /// 노드가 잠금 해제 가능한지 확인합니다.
        /// </summary>
        public bool CanUnlock(IReadOnlyDictionary<int, bool> unlockedNodes, int availablePoints)
        {
            if (availablePoints < requiredPoints)
            {
                return false;
            }
            
            // 선행 노드가 없으면 바로 해제 가능
            if (prerequisites.Count == 0)
            {
                return true;
            }
            
            // 모든 선행 노드가 잠금 해제되었는지 확인
            foreach (int prereqId in prerequisites)
            {
                if (!unlockedNodes.TryGetValue(prereqId, out bool isUnlocked) || !isUnlocked)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}
