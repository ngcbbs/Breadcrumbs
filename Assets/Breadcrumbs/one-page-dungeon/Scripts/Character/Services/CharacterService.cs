using System;
using System.Collections.Generic;
using System.Linq;
using Breadcrumbs.DependencyInjection;
using Breadcrumbs.Singletons;
using UnityEngine;

namespace Breadcrumbs.Character.Services
{
    /// <summary>
    /// Implementation of the character service using dependency injection
    /// </summary>
    public class CharacterService : Singleton<CharacterService>, ICharacterService, IDependencyProvider
    {
        // Active character references
        private Dictionary<string, ICharacter> characters = new Dictionary<string, ICharacter>();
        private ICharacter activePlayerCharacter;
        
        // Resources
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private Transform characterContainer;
        
        // Services
        [Inject] private IStatService statService;
        
        // Events
        public event Action<ICharacter, int> OnCharacterLevelUp;
        public event Action<ICharacter, StatType, float> OnCharacterStatChanged;
        
        /// <summary>
        /// Service provider for character service
        /// </summary>
        [Provide]
        public ICharacterService ProvideCharacterService()
        {
            return this;
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            // Set up character container if needed
            if (characterContainer == null)
            {
                characterContainer = new GameObject("Characters").transform;
                characterContainer.SetParent(transform);
            }
        }
        
        private void Start()
        {
            // Hook up events from characters
            foreach (var character in GetComponentsInChildren<Character>())
            {
                RegisterCharacter(character);
            }
        }
        
        /// <summary>
        /// Creates a new character with the specified parameters
        /// </summary>
        public ICharacter CreateCharacter(string name, GenderType gender, ClassType classType)
        {
            if (characterPrefab == null)
            {
                Debug.LogError("Character prefab not set in CharacterService");
                return null;
            }
            
            // Create character game object
            GameObject characterObj = Instantiate(characterPrefab, characterContainer);
            characterObj.name = $"Character_{name}";
            
            // Set up Character component
            Character character = characterObj.GetComponent<Character>();
            if (character == null)
            {
                Debug.LogError("Character prefab does not have a Character component");
                Destroy(characterObj);
                return null;
            }
            
            /*
            // Set basic properties using reflection or a setup method
            // For now, we'll assume Character has setter methods or this would be exposed through Unity serialization
            character.Setup(name, gender, classType);
            // */
            Debug.Log("character.Setup(name, gender, classType); // need fix?");
            
            // Initialize the character
            character.Initialize();
            
            // Register the character
            RegisterCharacter(character);
            
            return character;
        }
        
        /// <summary>
        /// Registers a character with the service
        /// </summary>
        private void RegisterCharacter(ICharacter character)
        {
            string id = ((Character)character).GetInstanceID().ToString();
            
            if (!characters.ContainsKey(id))
            {
                characters.Add(id, character);
                
                // Subscribe to events
                character.OnLevelUp += level => OnCharacterLevelUp?.Invoke(character, level);
                
                if (character.Stats != null)
                {
                    character.Stats.OnStatChanged += (statType, value) => 
                        OnCharacterStatChanged?.Invoke(character, statType, value);
                }
                
                // Set as player character if it's the first one
                if (activePlayerCharacter == null)
                {
                    activePlayerCharacter = character;
                }
            }
        }
        
        /// <summary>
        /// Gets a character by ID
        /// </summary>
        public ICharacter GetCharacter(string characterId)
        {
            if (characters.TryGetValue(characterId, out ICharacter character))
            {
                return character;
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets all available characters
        /// </summary>
        public IEnumerable<ICharacter> GetAllCharacters()
        {
            return characters.Values;
        }
        
        /// <summary>
        /// Gets the active player character
        /// </summary>
        public ICharacter GetPlayerCharacter()
        {
            return activePlayerCharacter;
        }
        
        /// <summary>
        /// Sets the active player character
        /// </summary>
        public void SetPlayerCharacter(ICharacter character)
        {
            if (characters.ContainsValue(character))
            {
                activePlayerCharacter = character;
            }
        }
        
        /// <summary>
        /// Applies experience to a character
        /// </summary>
        public bool GainExperience(ICharacter character, int amount)
        {
            if (character == null || amount <= 0) return false;
            
            return character.GainExperience(amount);
        }
        
        /// <summary>
        /// Applies a stat point for the character
        /// </summary>
        public bool ApplyStatPoint(ICharacter character, StatType statType)
        {
            // Delegate to specialized stat service if available
            if (statService != null)
            {
                return statService.ApplyStatPoint(character, statType);
            }
            
            // todo: Fallback implementation
            if (character != null && character.Stats != null)
            {
                // Check if points are available
                //if (character is PlayerCharacter playerCharacter && playerCharacter.StatPoints > 0)
                if (false)
                {
                    character.Stats.AddBonus(statType, 1);
                    
                    // Update derived stats
                    character.Stats.CalculateDerivedStats(character.ClassType);
                    
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Applies a skill point for the character
        /// </summary>
        public bool ApplySkillPoint(ICharacter character, string skillId)
        {
            if (character == null || string.IsNullOrEmpty(skillId)) return false;
            
            return character.UpgradeSkill(skillId);
        }
        
        /// <summary>
        /// Saves character data to persistent storage
        /// </summary>
        public bool SaveCharacterData(ICharacter character)
        {
            if (character == null) return false;
            
            try
            {
                // Convert character to serializable format
                string characterJson = JsonUtility.ToJson(CreateSaveData(character));
                
                // Save to PlayerPrefs for simplicity
                // In a real implementation, this would use proper file I/O
                string key = $"Character_{((Character)character).GetInstanceID()}";
                PlayerPrefs.SetString(key, characterJson);
                PlayerPrefs.Save();
                
                Debug.Log($"Character data saved: {character.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save character data: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Loads character data from persistent storage
        /// </summary>
        public ICharacter LoadCharacterData(string characterId)
        {
            try
            {
                // In a real implementation, this would use proper file I/O
                string key = $"Character_{characterId}";
                if (!PlayerPrefs.HasKey(key))
                {
                    Debug.LogWarning($"No saved data found for character ID: {characterId}");
                    return null;
                }
                
                string characterJson = PlayerPrefs.GetString(key);
                CharacterSaveData saveData = JsonUtility.FromJson<CharacterSaveData>(characterJson);
                
                // Create or update character
                ICharacter character = GetCharacter(characterId);
                if (character == null)
                {
                    // Create new character
                    character = CreateCharacter(saveData.name, saveData.gender, saveData.classType);
                }
                
                // Apply saved data
                ApplySaveData(character, saveData);
                
                Debug.Log($"Character data loaded: {character.Name}");
                return character;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load character data: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Equips an item on a character
        /// </summary>
        public bool EquipItem(ICharacter character, IEquipmentItem item)
        {
            if (character == null || item == null) return false;
            
            return character.EquipItem(item);
        }
        
        /// <summary>
        /// Unequips an item from a character
        /// </summary>
        public IEquipmentItem UnequipItem(ICharacter character, EquipmentSlot slot)
        {
            if (character == null) return null;
            
            return character.UnequipItem(slot);
        }
        
        #region Save Data Helpers
        
        /// <summary>
        /// Serializable class for character save data
        /// </summary>
        [Serializable]
        private class CharacterSaveData
        {
            public string name;
            public GenderType gender;
            public ClassType classType;
            public int level;
            public int experience;
            public SerializableDictionary<StatType, float> baseStats;
            public SerializableDictionary<StatType, float> bonusStats;
            public List<string> learnedSkills;
            public SerializableDictionary<string, int> skillLevels;
            public SerializableDictionary<EquipmentSlot, string> equippedItems;
        }
        
        /// <summary>
        /// Creates save data from a character
        /// </summary>
        private CharacterSaveData CreateSaveData(ICharacter character)
        {
            CharacterSaveData saveData = new CharacterSaveData
            {
                name = character.Name,
                gender = character.Gender,
                classType = character.ClassType,
                level = character.Level,
                experience = character.Stats.Experience,
                baseStats = new SerializableDictionary<StatType, float>(),
                bonusStats = new SerializableDictionary<StatType, float>(),
                learnedSkills = new List<string>(),
                skillLevels = new SerializableDictionary<string, int>(),
                equippedItems = new SerializableDictionary<EquipmentSlot, string>()
            };
            
            // Save stats (would need reflection or accessor methods for internal stat values)
            // This is a placeholder for the actual implementation
            
            // Save equipped items
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                IEquipmentItem item = character.GetEquippedItem(slot);
                if (item != null)
                {
                    saveData.equippedItems.Add(slot, item.ItemId);
                }
            }
            
            return saveData;
        }
        
        /// <summary>
        /// Applies save data to a character
        /// </summary>
        private void ApplySaveData(ICharacter character, CharacterSaveData saveData)
        {
            // This is a placeholder for the actual implementation
            // Would need to set stats, skills, items, etc.
        }
        
        #endregion
    }
    
    /// <summary>
    /// Serializable dictionary for saving data
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [Serializable]
        public struct Pair
        {
            public TKey Key;
            public TValue Value;
        }
        
        [SerializeField]
        private List<Pair> pairs = new List<Pair>();
        
        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        
        public void Add(TKey key, TValue value)
        {
            pairs.Add(new Pair { Key = key, Value = value });
            dictionary[key] = value;
        }
        
        public Dictionary<TKey, TValue> ToDictionary()
        {
            if (dictionary.Count == 0 && pairs.Count > 0)
            {
                dictionary = pairs.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            
            return dictionary;
        }
    }
}
