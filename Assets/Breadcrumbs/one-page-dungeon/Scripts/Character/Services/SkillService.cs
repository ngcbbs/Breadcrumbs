using System;
using System.Collections.Generic;
using System.Linq;
using Breadcrumbs.Character.Skills;
using Breadcrumbs.DependencyInjection;
using Breadcrumbs.EventSystem;
using Breadcrumbs.Singletons;
using UnityEngine;

namespace Breadcrumbs.Character.Services
{
    /// <summary>
    /// 스킬 관리 서비스 구현
    /// </summary>
    public class SkillService : Singleton<SkillService>, ISkillService, IDependencyProvider
    {
        [Header("설정")]
        [SerializeField] private SkillDefinition[] skillDefinitions;
        [SerializeField] private SkillTree[] skillTrees;
        
        // 캐릭터별 스킬 데이터
        private Dictionary<ICharacter, Dictionary<string, Skill>> characterSkills = new Dictionary<ICharacter, Dictionary<string, Skill>>();
        private Dictionary<ICharacter, Dictionary<string, SkillTree>> characterTrees = new Dictionary<ICharacter, Dictionary<string, SkillTree>>();
        private Dictionary<ICharacter, Dictionary<int, bool>> unlockedNodes = new Dictionary<ICharacter, Dictionary<int, bool>>();
        private Dictionary<ICharacter, int> skillPoints = new Dictionary<ICharacter, int>();
        
        // 스킬 정의 캐시
        private Dictionary<string, SkillDefinition> definitionCache;
        private Dictionary<string, SkillTree> treeCache;
        
        // 이벤트
        public event Action<ICharacter, string> OnSkillLearned;
        public event Action<ICharacter, string, Transform> OnSkillUsed;
        public event Action<ICharacter, string, int> OnSkillUpgraded;
        
        [Provide]
        public ISkillService ProvideSkillService()
        {
            return this;
        }
        
        protected override void Awake()
        {
            base.Awake();
            InitializeCache();
        }
        
        private void Update()
        {
            // 스킬 쿨다운 업데이트
            float deltaTime = Time.deltaTime;
            
            foreach (var characterEntry in characterSkills)
            {
                foreach (var skill in characterEntry.Value.Values)
                {
                    skill.UpdateCooldown(deltaTime);
                }
            }
        }
        
        /// <summary>
        /// 스킬과 스킬 트리 캐시를 초기화합니다.
        /// </summary>
        private void InitializeCache()
        {
            // 스킬 정의 캐시 초기화
            definitionCache = new Dictionary<string, SkillDefinition>();
            if (skillDefinitions != null)
            {
                foreach (var definition in skillDefinitions)
                {
                    if (definition != null && !string.IsNullOrEmpty(definition.SkillId))
                    {
                        definitionCache[definition.SkillId] = definition;
                    }
                }
            }
            
            // 스킬 트리 캐시 초기화
            treeCache = new Dictionary<string, SkillTree>();
            if (skillTrees != null)
            {
                foreach (var tree in skillTrees)
                {
                    if (tree != null && !string.IsNullOrEmpty(tree.TreeId))
                    {
                        tree.Initialize();
                        treeCache[tree.TreeId] = tree;
                    }
                }
            }
            
            Debug.Log($"스킬 서비스 초기화 완료: {definitionCache.Count}개의 스킬, {treeCache.Count}개의 스킬 트리");
        }
        
        /// <summary>
        /// 캐릭터에 스킬과 스킬 트리를 초기화합니다.
        /// </summary>
        public void InitializeCharacter(ICharacter character)
        {
            if (character == null) return;
            
            // 스킬 딕셔너리 초기화
            if (!characterSkills.ContainsKey(character))
            {
                characterSkills[character] = new Dictionary<string, Skill>();
            }
            
            // 스킬 트리 딕셔너리 초기화
            if (!characterTrees.ContainsKey(character))
            {
                characterTrees[character] = new Dictionary<string, SkillTree>();
                
                // 클래스에 맞는 스킬 트리 할당
                foreach (var tree in treeCache.Values)
                {
                    if (tree.ClassRestriction == ClassType.None || tree.ClassRestriction == character.ClassType)
                    {
                        characterTrees[character][tree.TreeId] = tree;
                    }
                }
            }
            
            // 잠금 해제된 노드 딕셔너리 초기화
            if (!unlockedNodes.ContainsKey(character))
            {
                unlockedNodes[character] = new Dictionary<int, bool>();
            }
            
            // 스킬 포인트 초기화
            if (!skillPoints.ContainsKey(character))
            {
                skillPoints[character] = 0;
            }
            
            // 캐릭터 클래스에 맞는 기본 스킬 등록
            foreach (var definition in definitionCache.Values)
            {
                if (definition.RequiredLevel <= 1 && 
                    (definition.ClassRequirement == ClassType.None || definition.ClassRequirement == character.ClassType))
                {
                    Skill skill = new Skill(definition.SkillId, definition);
                    characterSkills[character][definition.SkillId] = skill;
                    
                    // 자동 습득 스킬이면 배우기
                    if (definition.RequiredLevel == 1)
                    {
                        skill.Learn();
                    }
                }
            }
            
            Debug.Log($"{character.Name} 스킬 초기화 완료");
        }
        
        /// <summary>
        /// 캐릭터에 스킬을 배웁니다.
        /// </summary>
        public bool LearnSkill(ICharacter character, string skillId)
        {
            if (character == null || string.IsNullOrEmpty(skillId)) return false;
            
            // 스킬 정의 확인
            if (!definitionCache.TryGetValue(skillId, out SkillDefinition definition))
            {
                Debug.LogWarning($"스킬 {skillId}를 찾을 수 없습니다.");
                return false;
            }
            
            // 캐릭터 스킬 딕셔너리 초기화 확인
            if (!characterSkills.TryGetValue(character, out var skills))
            {
                skills = new Dictionary<string, Skill>();
                characterSkills[character] = skills;
            }
            
            // 스킬이 이미 존재하는지 확인
            if (skills.TryGetValue(skillId, out Skill skill))
            {
                // 이미 배운 스킬인지 확인
                if (skill.IsLearned)
                {
                    Debug.Log($"{character.Name}은(는) 이미 {definition.SkillName} 스킬을 배웠습니다.");
                    return false;
                }
            }
            else
            {
                // 새 스킬 생성
                skill = new Skill(skillId, definition);
                skills[skillId] = skill;
            }
            
            // 스킬 요구사항 확인
            if (character.Level < definition.RequiredLevel)
            {
                Debug.LogWarning($"{character.Name}은(는) {definition.SkillName} 스킬을 배우기 위한 레벨이 부족합니다. (필요 레벨: {definition.RequiredLevel})");
                return false;
            }
            
            if (definition.ClassRequirement != ClassType.None && definition.ClassRequirement != character.ClassType)
            {
                Debug.LogWarning($"{character.Name}은(는) {definition.SkillName} 스킬을 배울 수 없는 클래스입니다.");
                return false;
            }
            
            // 선행 스킬 확인
            if (definition.PrerequisiteSkills != null)
            {
                foreach (var prereqId in definition.PrerequisiteSkills)
                {
                    if (!HasSkill(character, prereqId))
                    {
                        SkillDefinition prereqDef = definitionCache.TryGetValue(prereqId, out var def) ? def : null;
                        string prereqName = prereqDef != null ? prereqDef.SkillName : prereqId;
                        
                        Debug.LogWarning($"{character.Name}은(는) 선행 스킬 {prereqName}을(를) 배우지 않았습니다.");
                        return false;
                    }
                }
            }
            
            // 스킬 배우기
            skill.Learn();
            
            // 이벤트 발생
            OnSkillLearned?.Invoke(character, skillId);
            
            Debug.Log($"{character.Name}이(가) {definition.SkillName} 스킬을 배웠습니다.");
            return true;
        }
        
        /// <summary>
        /// 캐릭터가 스킬을 사용합니다.
        /// </summary>
        public bool UseSkill(ICharacter character, string skillId, Transform target = null)
        {
            if (character == null || string.IsNullOrEmpty(skillId)) return false;
            
            // 캐릭터가 스킬을 가지고 있는지 확인
            if (!characterSkills.TryGetValue(character, out var skills) || !skills.TryGetValue(skillId, out Skill skill) || !skill.IsLearned)
            {
                Debug.LogWarning($"{character.Name}은(는) {skillId} 스킬을 배우지 않았습니다.");
                return false;
            }
            
            // 스킬 사용
            if (skill.Use(character, target))
            {
                // 스킬 효과 적용
                ApplySkillEffects(character, skill, target);
                
                // 이벤트 발생
                OnSkillUsed?.Invoke(character, skillId, target);
                
                SkillDefinition definition = skill.Definition;
                Debug.Log($"{character.Name}이(가) {definition.SkillName} 스킬을 사용했습니다.");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 스킬 효과를 적용합니다.
        /// </summary>
        private void ApplySkillEffects(ICharacter caster, Skill skill, Transform target)
        {
            if (caster == null || skill == null || skill.Definition == null) return;
            
            SkillDefinition definition = skill.Definition;
            int skillLevel = skill.SkillLevel;
            
            // 타겟 캐릭터 참조 (필요시)
            ICharacter targetCharacter = null;
            if (target != null)
            {
                targetCharacter = target.GetComponent<ICharacter>() as ICharacter;
            }
            
            // 대상 유형에 따라 효과 적용
            switch (definition.TargetType)
            {
                case TargetType.Self:
                    // 자신에게 효과 적용
                    ApplyEffectsToTarget(definition.GetEffects(), caster, caster, skillLevel);
                    break;
                    
                case TargetType.SingleAlly:
                case TargetType.SingleEnemy:
                    // 단일 대상에게 효과 적용
                    if (targetCharacter != null)
                    {
                        ApplyEffectsToTarget(definition.GetEffects(), caster, targetCharacter, skillLevel);
                    }
                    break;
                    
                case TargetType.AllAllies:
                    // 모든 아군에게 효과 적용 (간소화된 구현)
                    ApplyEffectsToTarget(definition.GetEffects(), caster, caster, skillLevel);
                    break;
                    
                case TargetType.AllEnemies:
                    // 모든 적에게 효과 적용 (실제로는 주변 감지 로직 필요)
                    if (targetCharacter != null)
                    {
                        ApplyEffectsToTarget(definition.GetEffects(), caster, targetCharacter, skillLevel);
                    }
                    break;
                    
                case TargetType.Area:
                case TargetType.Direction:
                    // 영역/방향 효과 (실제로는 더 복잡한 구현 필요)
                    if (targetCharacter != null)
                    {
                        ApplyEffectsToTarget(definition.GetEffects(), caster, targetCharacter, skillLevel);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 특정 대상에게 효과를 적용합니다.
        /// </summary>
        private void ApplyEffectsToTarget(IReadOnlyList<SkillEffectData> effects, ICharacter caster, ICharacter target, int skillLevel)
        {
            if (effects == null || caster == null || target == null) return;
            
            foreach (var effect in effects)
            {
                float value = effect.GetValue(skillLevel);
                float duration = effect.GetDuration(skillLevel);
                
                switch (effect.EffectType)
                {
                    case EffectType.Damage:
                        // 데미지 적용
                        target.TakeDamage(value, effect.DamageType);
                        break;
                        
                    case EffectType.Heal:
                        // 치유 적용
                        target.Heal(value);
                        break;
                        
                    case EffectType.StatBoost:
                        // 스탯 증가 (버프로 적용)
                        if (target.Stats != null)
                        {
                            StatModifier modifier = new StatModifier(value, StatModifierType.PercentAdd, caster, duration);
                            target.Stats.AddModifier(effect.TargetStat, modifier);
                        }
                        break;
                        
                    case EffectType.StatReduction:
                        // 스탯 감소 (디버프로 적용)
                        if (target.Stats != null)
                        {
                            StatModifier modifier = new StatModifier(-value, StatModifierType.PercentAdd, caster, duration);
                            target.Stats.AddModifier(effect.TargetStat, modifier);
                        }
                        break;
                        
                    case EffectType.StatusEffect:
                        // 상태 효과 적용 (스턴, 빙결 등) - 버프 시스템 필요
                        Debug.Log($"{target.Name}에게 상태 효과 적용 (지속시간: {duration}초)");
                        break;
                        
                    case EffectType.Movement:
                        // 이동 효과 (점프, 대쉬 등) - 움직임 컨트롤러 필요
                        Debug.Log($"{target.Name}에게 이동 효과 적용");
                        break;
                        
                    case EffectType.Shield:
                        // 보호막 적용 - 쉴드 시스템 필요
                        Debug.Log($"{target.Name}에게 보호막 {value} 적용 (지속시간: {duration}초)");
                        break;
                }
            }
        }
        
        /// <summary>
        /// 캐릭터의 스킬을 업그레이드합니다.
        /// </summary>
        public bool UpgradeSkill(ICharacter character, string skillId)
        {
            if (character == null || string.IsNullOrEmpty(skillId)) return false;
            
            // 캐릭터가 스킬을 가지고 있는지 확인
            if (!characterSkills.TryGetValue(character, out var skills) || !skills.TryGetValue(skillId, out Skill skill))
            {
                Debug.LogWarning($"{character.Name}은(는) {skillId} 스킬을 가지고 있지 않습니다.");
                return false;
            }
            
            // 스킬 레벨 확인
            if (!skill.IsLearned)
            {
                Debug.LogWarning($"{character.Name}은(는) {skillId} 스킬을 배우지 않았습니다.");
                return false;
            }
            
            // 스킬 포인트 확인
            if (!skillPoints.TryGetValue(character, out int points) || points <= 0)
            {
                Debug.LogWarning($"{character.Name}은(는) 스킬 포인트가 부족합니다.");
                return false;
            }
            
            // 스킬 업그레이드
            if (skill.Upgrade())
            {
                // 스킬 포인트 차감
                skillPoints[character]--;
                
                // 이벤트 발생
                OnSkillUpgraded?.Invoke(character, skillId, skill.SkillLevel);
                
                SkillDefinition definition = skill.Definition;
                Debug.Log($"{character.Name}의 {definition.SkillName} 스킬이 레벨 {skill.SkillLevel}로 업그레이드되었습니다.");
                return true;
            }
            else
            {
                Debug.LogWarning($"{character.Name}의 {skillId} 스킬은 더 이상 업그레이드할 수 없습니다.");
                return false;
            }
        }
        
        /// <summary>
        /// 캐릭터가 스킬을 가지고 있는지 확인합니다.
        /// </summary>
        public bool HasSkill(ICharacter character, string skillId)
        {
            if (character == null || string.IsNullOrEmpty(skillId)) return false;
            
            return characterSkills.TryGetValue(character, out var skills) && 
                   skills.TryGetValue(skillId, out Skill skill) && 
                   skill.IsLearned;
        }
        
        /// <summary>
        /// 캐릭터의 스킬 레벨을 확인합니다.
        /// </summary>
        public int GetSkillLevel(ICharacter character, string skillId)
        {
            if (character == null || string.IsNullOrEmpty(skillId)) return 0;
            
            if (characterSkills.TryGetValue(character, out var skills) && 
                skills.TryGetValue(skillId, out Skill skill) && 
                skill.IsLearned)
            {
                return skill.SkillLevel;
            }
            
            return 0;
        }
        
        /// <summary>
        /// 캐릭터의 사용 가능한 스킬 포인트를 반환합니다.
        /// </summary>
        public int GetAvailableSkillPoints(ICharacter character)
        {
            if (character == null) return 0;
            
            return skillPoints.TryGetValue(character, out int points) ? points : 0;
        }
        
        /// <summary>
        /// 캐릭터의 사용 가능한 스킬 포인트를 설정합니다.
        /// </summary>
        public void SetAvailableSkillPoints(ICharacter character, int points)
        {
            if (character == null) return;
            
            skillPoints[character] = Mathf.Max(0, points);
        }
        
        /// <summary>
        /// 캐릭터의 모든 스킬 정보를 반환합니다.
        /// </summary>
        public IEnumerable<SkillInfo> GetAllSkills(ICharacter character)
        {
            if (character == null) return Enumerable.Empty<SkillInfo>();
            
            List<SkillInfo> result = new List<SkillInfo>();
            
            if (characterSkills.TryGetValue(character, out var skills))
            {
                foreach (var skill in skills.Values)
                {
                    SkillDefinition definition = skill.Definition;
                    if (definition != null)
                    {
                        SkillInfo info = new SkillInfo
                        {
                            SkillId = definition.SkillId,
                            Name = definition.SkillName,
                            Description = definition.GetFormattedDescription(skill.SkillLevel),
                            Type = definition.SkillType,
                            RequiredLevel = definition.RequiredLevel,
                            ClassRequirement = definition.ClassRequirement,
                            ManaCost = definition.GetManaCost(skill.SkillLevel),
                            Cooldown = definition.GetCooldown(skill.SkillLevel),
                            MaxLevel = definition.MaxLevel
                        };
                        
                        result.Add(info);
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 특정 클래스가 사용할 수 있는 모든 스킬 정보를 반환합니다.
        /// </summary>
        public IEnumerable<SkillInfo> GetAvailableSkillsForClass(ClassType classType)
        {
            List<SkillInfo> result = new List<SkillInfo>();
            
            foreach (var definition in definitionCache.Values)
            {
                if (definition.ClassRequirement == ClassType.None || definition.ClassRequirement == classType)
                {
                    SkillInfo info = new SkillInfo
                    {
                        SkillId = definition.SkillId,
                        Name = definition.SkillName,
                        Description = definition.GetFormattedDescription(1),
                        Type = definition.SkillType,
                        RequiredLevel = definition.RequiredLevel,
                        ClassRequirement = definition.ClassRequirement,
                        ManaCost = definition.GetManaCost(1),
                        Cooldown = definition.GetCooldown(1),
                        MaxLevel = definition.MaxLevel
                    };
                    
                    result.Add(info);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 캐릭터가 특정 레벨에서 배울 수 있는 스킬 정보를 반환합니다.
        /// </summary>
        public IEnumerable<SkillInfo> GetAvailableSkillsForLevel(ICharacter character, int level)
        {
            if (character == null) return Enumerable.Empty<SkillInfo>();
            
            List<SkillInfo> result = new List<SkillInfo>();
            ClassType classType = character.ClassType;
            
            foreach (var definition in definitionCache.Values)
            {
                if (definition.RequiredLevel == level && 
                    (definition.ClassRequirement == ClassType.None || definition.ClassRequirement == classType))
                {
                    SkillInfo info = new SkillInfo
                    {
                        SkillId = definition.SkillId,
                        Name = definition.SkillName,
                        Description = definition.GetFormattedDescription(1),
                        Type = definition.SkillType,
                        RequiredLevel = definition.RequiredLevel,
                        ClassRequirement = definition.ClassRequirement,
                        ManaCost = definition.GetManaCost(1),
                        Cooldown = definition.GetCooldown(1),
                        MaxLevel = definition.MaxLevel
                    };
                    
                    result.Add(info);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 스킬 트리 노드를 잠금 해제합니다.
        /// </summary>
        public bool UnlockSkillTreeNode(ICharacter character, string treeId, int nodeId)
        {
            if (character == null || string.IsNullOrEmpty(treeId)) return false;
            
            // 스킬 트리 확인
            if (!characterTrees.TryGetValue(character, out var trees) || !trees.TryGetValue(treeId, out SkillTree tree))
            {
                Debug.LogWarning($"{character.Name}은(는) {treeId} 스킬 트리를 가지고 있지 않습니다.");
                return false;
            }
            
            // 노드 확인
            SkillTreeNode node = tree.GetNode(nodeId);
            if (node == null)
            {
                Debug.LogWarning($"노드 ID {nodeId}를 찾을 수 없습니다.");
                return false;
            }
            
            // 잠금 해제 여부 확인
            if (!unlockedNodes.TryGetValue(character, out var nodes))
            {
                nodes = new Dictionary<int, bool>();
                unlockedNodes[character] = nodes;
            }
            
            if (nodes.TryGetValue(nodeId, out bool isUnlocked) && isUnlocked)
            {
                Debug.LogWarning($"{character.Name}은(는) 이미 이 노드를 잠금 해제했습니다.");
                return false;
            }
            
            // 스킬 포인트 확인
            if (!skillPoints.TryGetValue(character, out int points) || points < node.RequiredPoints)
            {
                Debug.LogWarning($"{character.Name}은(는) 스킬 포인트가 부족합니다. (필요: {node.RequiredPoints}, 보유: {points})");
                return false;
            }
            
            // 선행 노드 확인
            if (!node.CanUnlock(nodes, points))
            {
                Debug.LogWarning($"{character.Name}은(는) 이 노드를 잠금 해제하기 위한 조건을 충족하지 않습니다.");
                return false;
            }
            
            // 노드 잠금 해제
            nodes[nodeId] = true;
            
            // 스킬 포인트 차감
            skillPoints[character] -= node.RequiredPoints;
            
            // 노드에 연결된 스킬 배우기
            if (!string.IsNullOrEmpty(node.SkillId))
            {
                LearnSkill(character, node.SkillId);
            }
            
            Debug.Log($"{character.Name}이(가) 스킬 트리 노드를 잠금 해제했습니다: {nodeId}");
            return true;
        }
        
        /// <summary>
        /// 캐릭터의 레벨 업에 따른 스킬 포인트 증가
        /// </summary>
        public void OnCharacterLevelUp(ICharacter character, int newLevel)
        {
            if (character == null) return;
            
            // 레벨 업에 따른 스킬 포인트 추가
            if (!skillPoints.TryGetValue(character, out int points))
            {
                points = 0;
            }
            
            // 레벨마다 1 포인트씩 추가 (게임 규칙에 따라 조정 가능)
            skillPoints[character] = points + 1;
            
            Debug.Log($"{character.Name}이(가) 레벨 {newLevel}에 도달하여 스킬 포인트 1 획득 (총 {skillPoints[character]})");
            
            // 새 레벨에서 사용 가능한 스킬 알림
            var availableSkills = GetAvailableSkillsForLevel(character, newLevel);
            foreach (var skill in availableSkills)
            {
                Debug.Log($"{character.Name}이(가) 이제 새 스킬을 배울 수 있습니다: {skill.Name}");
            }
        }
    }
}
