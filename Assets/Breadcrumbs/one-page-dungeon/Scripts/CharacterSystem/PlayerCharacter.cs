using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.CharacterSystem {
    public class PlayerCharacter : MonoBehaviour
    {
        [Header("기본 정보")]
        [SerializeField] private string characterName;
        [SerializeField] private GenderType gender;
        [SerializeField] private ClassType classType;
        [SerializeField] private ClassData classData;
        
        [Header("외형 커스터마이징")]
        [SerializeField] private CharacterCustomization customization;
        
        // 스탯 및 성장 관련
        private CharacterStats stats = new CharacterStats();
        private int statPoints = 0;  // 스탯 포인트
        private int skillPoints = 0; // 스킬 포인트
        
        // 스킬 매니저
        private SkillManager skillManager;
        
        // 직업별 컨트롤러
        private ClassController classController;
        
        // 장착 아이템 및 장비
        private Dictionary<EquipmentSlot, EquipmentItem> equippedItems = new Dictionary<EquipmentSlot, EquipmentItem>();
        
        // 레벨업 델리게이트
        public delegate void LevelUpHandler(int newLevel);
        public event LevelUpHandler OnLevelUp;
        
        // 프로퍼티
        public string CharacterName => characterName;
        public GenderType Gender => gender;
        public ClassType ClassType => classType;
        public CharacterStats Stats => stats;
        public int Level => stats.Level;
        public int StatPoints => statPoints;
        public int SkillPoints => skillPoints;
        public CharacterCustomization Customization => customization;
        public ClassController ClassController => classController;
        
        private void Awake()
        {
            // 스킬 매니저 초기화
            skillManager = GetComponent<SkillManager>();
            if (skillManager == null)
            {
                skillManager = gameObject.AddComponent<SkillManager>();
            }
            skillManager.Initialize(this);
            
            // 커스터마이징 초기화
            if (customization == null)
            {
                customization = new CharacterCustomization();
            }
        }
        
        private void Start()
        {
            InitializeCharacter();
        }
        
        private void Update()
        {
            // 스탯 업데이트
            stats.UpdateStats(Time.deltaTime);
            
            // 직업 컨트롤러 업데이트
            classController?.Update(Time.deltaTime);
        }
        
        // 초기화
        public void InitializeCharacter()
        {
            if (classData == null)
            {
                Debug.LogError("Class data is not assigned!");
                return;
            }
            
            // 기본 스탯 설정
            stats.SetBaseStat(StatType.Strength, classData.baseStrength);
            stats.SetBaseStat(StatType.Dexterity, classData.baseDexterity);
            stats.SetBaseStat(StatType.Intelligence, classData.baseIntelligence);
            stats.SetBaseStat(StatType.Vitality, classData.baseVitality);
            stats.SetBaseStat(StatType.Wisdom, classData.baseWisdom);
            stats.SetBaseStat(StatType.Luck, classData.baseLuck);
            
            // 파생 스탯 계산
            StatCalculator.CalculateDerivedStats(stats, classType);
            
            // 체력/마나 초기화
            stats.InitializeForCombat();
            
            // 직업 컨트롤러 생성
            CreateClassController();
            
            // 시작 스킬 추가
            foreach (var skill in classData.startingSkills)
            {
                skillManager.AddSkill(skill);
            }
            
            // 외형 적용
            ApplyCustomization();
        }
        
        // 직업 컨트롤러 생성
        private void CreateClassController()
        {
            switch (classType)
            {
                case ClassType.Warrior:
                    classController = new WarriorController(this);
                    break;
                case ClassType.Mage:
                    classController = new MageController(this);
                    break;
                case ClassType.Rogue:
                    classController = new RogueController(this);
                    break;
                case ClassType.Cleric:
                    classController = new ClericController(this);
                    break;
                default:
                    Debug.LogError($"Unsupported class type: {classType}");
                    break;
            }
            
            classController?.Initialize();
        }
        
        // 외형 적용
        public void ApplyCustomization()
        {
            // 외형 컨트롤러 찾기
            CharacterAppearanceController appearanceController = GetComponent<CharacterAppearanceController>();
            if (appearanceController != null)
            {
                appearanceController.SetCustomization(customization);
            }
            else
            {
                // 간단한 로그만 출력
                Debug.Log($"Applied customization: Hair style {customization.hairStyle}, Hair color {customization.hairColor}");
            }
        }
        
        // 경험치 획득
        public void GainExperience(int amount)
        {
            bool leveledUp = stats.AddExperience(amount);
            
            if (leveledUp)
            {
                OnLevelUp?.Invoke(stats.Level);
                HandleLevelUp();
            }
        }
        
        // 레벨업 처리
        private void HandleLevelUp()
        {
            Debug.Log($"Level up! New level: {stats.Level}");
            
            // 스탯 포인트 증가
            statPoints += 5;
            
            // 스킬 포인트 증가
            skillPoints += 1;
            
            // 직업별 성장치 적용
            stats.AddBonus(StatType.Strength, classData.strengthGrowth);
            stats.AddBonus(StatType.Dexterity, classData.dexterityGrowth);
            stats.AddBonus(StatType.Intelligence, classData.intelligenceGrowth);
            stats.AddBonus(StatType.Vitality, classData.vitalityGrowth);
            stats.AddBonus(StatType.Wisdom, classData.wisdomGrowth);
            stats.AddBonus(StatType.Luck, classData.luckGrowth);
            
            // 파생 스탯 재계산
            StatCalculator.CalculateDerivedStats(stats, classType);
            
            // 직업 컨트롤러에 레벨업 알림
            classController?.OnLevelUp();
            
            // 특정 레벨에서 새로운 스킬 획득
            CheckNewSkillsAtLevel();
        }
        
        // 현재 레벨에서 배울 수 있는 스킬 확인
        private void CheckNewSkillsAtLevel()
        {
            // 실제 구현에서는 스킬 데이터베이스에서 적절한 스킬을 검색
            Debug.Log($"Checking new skills at level {stats.Level}");
            
            // 예시
            /*
            foreach (var skill in SkillDatabase.GetSkillsForClass(classType))
            {
                if (skill.requiredLevel == stats.Level)
                {
                    // 자동으로 스킬 획득
                    skillManager.AddSkill(skill);
                }
            }
            */
        }
        
        // 스킬 사용
        public bool UseSkill(string skillId, Transform target = null)
        {
            return skillManager.UseSkill(skillId, target);
        }
        
        // 클래스 특수 능력 사용
        public void UseClassSpecial()
        {
            classController?.ActivateClassSpecial();
        }
        
        // 스탯 포인트 사용
        public bool UseStatPoint(StatType statType)
        {
            if (statPoints <= 0)
            {
                Debug.Log("No stat points available");
                return false;
            }
            
            // 스탯 증가
            stats.AddBonus(statType, 1);
            statPoints--;
            
            // 파생 스탯 재계산
            StatCalculator.CalculateDerivedStats(stats, classType);
            
            Debug.Log($"Increased {statType} by 1. Remaining points: {statPoints}");
            return true;
        }
        
        // 스킬 포인트 사용
        public bool UseSkillPoint(string skillId)
        {
            if (skillPoints <= 0)
            {
                Debug.Log("No skill points available");
                return false;
            }
            
            if (skillManager.LevelUpSkill(skillId))
            {
                skillPoints--;
                Debug.Log($"Leveled up skill {skillId}. Remaining points: {skillPoints}");
                return true;
            }
            
            return false;
        }
        
        // 장비 장착
        public bool EquipItem(EquipmentItem item)
        {
            if (item == null)
                return false;
            
            // 클래스 착용 가능 여부 확인
            if (!CanEquipItem(item))
            {
                Debug.Log($"Cannot equip {item.itemName}: class or level restriction");
                return false;
            }
            
            // 이미 장착된 기존 아이템 제거
            if (equippedItems.TryGetValue(item.equipSlot, out EquipmentItem oldItem))
            {
                UnequipItem(item.equipSlot);
            }
            
            // 새 아이템 장착
            equippedItems[item.equipSlot] = item;
            
            // 아이템 스탯 적용
            foreach (var stat in item.stats)
            {
                StatModifier mod = new StatModifier(stat.value, stat.type, item);
                stats.AddModifier(stat.statType, mod);
            }
            
            Debug.Log($"Equipped {item.itemName} in {item.equipSlot} slot");
            
            // 파생 스탯 재계산
            StatCalculator.CalculateDerivedStats(stats, classType);
            
            return true;
        }
        
        // 장비 해제
        public EquipmentItem UnequipItem(EquipmentSlot slot)
        {
            if (equippedItems.TryGetValue(slot, out EquipmentItem item))
            {
                // 아이템에서 부여한 스탯 제거
                stats.RemoveAllModifiersFromSource(item);
                
                // 장비 목록에서 제거
                equippedItems.Remove(slot);
                
                Debug.Log($"Unequipped {item.itemName} from {slot} slot");
                
                // 파생 스탯 재계산
                StatCalculator.CalculateDerivedStats(stats, classType);
                
                return item;
            }
            
            return null;
        }
        
        // 아이템 장착 가능 여부 확인
        private bool CanEquipItem(EquipmentItem item)
        {
            // 레벨 확인
            if (stats.Level < item.requiredLevel)
                return false;
            
            // 클래스 확인
            if (item.classType != ClassType.None && item.classType != classType)
                return false;
            
            // 무기 타입 확인
            if (item is WeaponItem weapon)
            {
                return classData.usableWeaponTypes.Contains(weapon.weaponType);
            }
            
            // 방어구 타입 확인
            if (item is ArmorItem armor)
            {
                return classData.usableArmorTypes.Contains(armor.armorType);
            }
            
            return true;
        }
        
        // 캐릭터 정보 표시 (디버그용)
        public void DisplayCharacterInfo()
        {
            Debug.Log($"====== Character Info: {characterName} ======");
            Debug.Log($"Class: {classType}, Level: {stats.Level}, Exp: {stats.Experience}/{stats.ExperienceToNextLevel}");
            Debug.Log($"Stats: STR {stats.GetStat(StatType.Strength)}, DEX {stats.GetStat(StatType.Dexterity)}, " +
                      $"INT {stats.GetStat(StatType.Intelligence)}, VIT {stats.GetStat(StatType.Vitality)}, " +
                      $"WIS {stats.GetStat(StatType.Wisdom)}, LUCK {stats.GetStat(StatType.Luck)}");
            Debug.Log($"HP: {stats.CurrentHealth}/{stats.GetStat(StatType.MaxHealth)}, " +
                      $"MP: {stats.CurrentMana}/{stats.GetStat(StatType.MaxMana)}");
            Debug.Log($"Attack: Physical {stats.GetStat(StatType.PhysicalAttack)}, Magical {stats.GetStat(StatType.MagicAttack)}");
            Debug.Log($"Defense: Physical {stats.GetStat(StatType.PhysicalDefense)}, Magical {stats.GetStat(StatType.MagicDefense)}");
            Debug.Log($"Crit: {stats.GetStat(StatType.CriticalChance)*100}% chance, {stats.GetStat(StatType.CriticalDamage)}x damage");
            Debug.Log($"Stat Points: {statPoints}, Skill Points: {skillPoints}");
            
            // 장착한 아이템 정보
            Debug.Log("Equipped Items:");
            foreach (var pair in equippedItems)
            {
                Debug.Log($"  - {pair.Key}: {pair.Value.itemName} (iLvl {pair.Value.itemLevel}, {pair.Value.rarity})");
            }
            
            Debug.Log("=======================================");
        }
        
        // 캐릭터 생성 정보 (저장용)
        [Serializable]
        public class CharacterSaveData
        {
            public string characterName;
            public GenderType gender;
            public ClassType classType;
            public int level;
            public int experience;
            public CharacterCustomization customization;
            public Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>();
            public Dictionary<StatType, float> bonusStats = new Dictionary<StatType, float>();
            public List<string> learnedSkills = new List<string>();
            public Dictionary<string, int> skillLevels = new Dictionary<string, int>();
            public Dictionary<EquipmentSlot, string> equippedItems = new Dictionary<EquipmentSlot, string>();
        }
        
        // 캐릭터 데이터 저장
        public CharacterSaveData SaveCharacterData()
        {
            CharacterSaveData saveData = new CharacterSaveData
            {
                characterName = this.characterName,
                gender = this.gender,
                classType = this.classType,
                level = stats.Level,
                experience = stats.Experience,
                customization = this.customization.Clone()
            };
            
            // 기본 스탯 저장
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                Stat statRef = stats.GetStatReference(statType);
                if (statRef != null)
                {
                    saveData.baseStats[statType] = statRef.BaseValue;
                    saveData.bonusStats[statType] = statRef.BonusValue;
                }
            }
            
            // 스킬 정보 저장
            Dictionary<string, Skill> allSkills = skillManager.GetAllSkills();
            foreach (var skillPair in allSkills)
            {
                saveData.learnedSkills.Add(skillPair.Key);
                saveData.skillLevels[skillPair.Key] = skillPair.Value.skillLevel;
            }
            
            // 장착 아이템 저장 (실제로는 아이템 ID만 저장하고 별도 아이템 DB에서 복원함)
            foreach (var itemPair in equippedItems)
            {
                saveData.equippedItems[itemPair.Key] = itemPair.Value.itemId;
            }
            
            return saveData;
        }
        
        // 캐릭터 데이터 로드
        public void LoadCharacterData(CharacterSaveData saveData, ItemDatabase itemDatabase)
        {
            if (saveData == null)
                return;
                
            // 기본 정보 복원
            this.characterName = saveData.characterName;
            this.gender = saveData.gender;
            this.classType = saveData.classType;
            
            // 커스터마이징 복원
            if (saveData.customization != null)
            {
                this.customization.CopyFrom(saveData.customization);
                ApplyCustomization();
            }
            
            // 스탯 복원
            foreach (var statPair in saveData.baseStats)
            {
                stats.SetBaseStat(statPair.Key, statPair.Value);
            }
            
            foreach (var statPair in saveData.bonusStats)
            {
                stats.AddBonus(statPair.Key, statPair.Value);
            }
            
            // 레벨 및 경험치 설정을 위한 내부 접근자가 필요함
            // 실제 구현에서는 CharacterStats 클래스에 LoadLevelData 메서드 추가 필요
            
            // 스킬 복원
            foreach (string skillId in saveData.learnedSkills)
            {
                // 스킬 데이터베이스에서 스킬 정보 로드
                // SkillData skillData = SkillDatabase.GetSkill(skillId);
                // if (skillData != null)
                // {
                //     skillManager.AddSkill(skillData);
                //     
                //     // 스킬 레벨 설정
                //     if (saveData.skillLevels.TryGetValue(skillId, out int level))
                //     {
                //         for (int i = 1; i < level; i++) // 시작 레벨이 1이므로 레벨-1번 레벨업
                //         {
                //             skillManager.LevelUpSkill(skillId);
                //         }
                //     }
                // }
            }
            
            // 장비 복원
            foreach (var itemPair in saveData.equippedItems)
            {
                // 아이템 데이터베이스에서 아이템 로드
                EquipmentItem item = itemDatabase.GetItemById(itemPair.Value);
                if (item != null)
                {
                    EquipItem(item);
                }
            }
            
            // 직업 컨트롤러 생성 및 초기화
            CreateClassController();
            
            // 파생 스탯 재계산
            StatCalculator.CalculateDerivedStats(stats, classType);
            
            // 전투 준비
            stats.InitializeForCombat();
            
            Debug.Log($"Character data loaded: {characterName}, Level {stats.Level}");
        }
    }
}