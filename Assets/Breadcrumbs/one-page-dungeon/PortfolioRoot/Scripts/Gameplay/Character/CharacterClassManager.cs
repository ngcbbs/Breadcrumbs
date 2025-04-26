using System;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Core;
using GamePortfolio.Gameplay.Character.Classes;

namespace GamePortfolio.Gameplay.Character {
    public class CharacterClassManager : MonoBehaviour {
        [Header("Available Classes")]
        [SerializeField]
        private GameObject warriorClassPrefab;
        [SerializeField]
        private GameObject archerClassPrefab;

        [Header("Class Selection")]
        [SerializeField]
        private string defaultClass = "Warrior";

        private Dictionary<string, GameObject> classPrefabs = new Dictionary<string, GameObject>();
        private BaseCharacterClass currentClass;

        private CharacterLevelManager levelManager;
        private StatAllocationSystem statAllocationSystem;
        private SkillPointManager skillPointManager;
        private CharacterCustomization customizationSystem;
        private SpecialStatSystem specialStatSystem;

        public event Action<BaseCharacterClass> OnClassChanged;

        private void Awake() {
            CacheComponents();
            RegisterClassPrefabs();
        }

        private void Start() {
            LoadSelectedClass();
        }

        private void CacheComponents() {
            levelManager = GetComponent<CharacterLevelManager>();
            statAllocationSystem = GetComponent<StatAllocationSystem>();
            skillPointManager = GetComponent<SkillPointManager>();
            customizationSystem = GetComponent<CharacterCustomization>();
            specialStatSystem = GetComponent<SpecialStatSystem>();

            if (levelManager == null)
                Debug.LogWarning("CharacterClassManager: CharacterLevelManager component not found!");

            if (statAllocationSystem == null)
                Debug.LogWarning("CharacterClassManager: StatAllocationSystem component not found!");

            if (skillPointManager == null)
                Debug.LogWarning("CharacterClassManager: SkillPointManager component not found!");

            if (customizationSystem == null)
                Debug.LogWarning("CharacterClassManager: CharacterCustomization component not found!");

            if (specialStatSystem == null)
                Debug.LogWarning("CharacterClassManager: SpecialStatSystem component not found!");
        }

        private void RegisterClassPrefabs() {
            if (warriorClassPrefab != null)
                classPrefabs["Warrior"] = warriorClassPrefab;

            if (archerClassPrefab != null)
                classPrefabs["Archer"] = archerClassPrefab;
        }

        private void LoadSelectedClass() {
            string classToLoad = defaultClass;

            // Load from game settings if available
            if (GameManager.HasInstance && !string.IsNullOrEmpty(GameManager.Instance.Settings.SelectedCharacterClass)) {
                classToLoad = GameManager.Instance.Settings.SelectedCharacterClass;
            }

            ChangeClass(classToLoad);
        }

        public void ChangeClass(string className) {
            if (string.IsNullOrEmpty(className) || !classPrefabs.ContainsKey(className)) {
                Debug.LogError($"CharacterClassManager: Class '{className}' not found!");
                return;
            }

            // Remove current class component if exists
            if (currentClass != null) {
                Destroy(currentClass);
            }

            // Instantiate the new class prefab
            GameObject classPrefab = classPrefabs[className];

            // For this implementation, instead of instantiating the entire prefab,
            // we'll add the class component to the current GameObject
            // In a real implementation, we might handle this differently based on the game's architecture

            if (className == "Warrior") {
                currentClass = gameObject.AddComponent<WarriorClass>();
            } else if (className == "Archer") {
                currentClass = gameObject.AddComponent<ArcherClass>();
            }

            // Reset character systems for class change
            ResetCharacterSystems();

            // Update game settings
            if (GameManager.HasInstance) {
                GameManager.Instance.Settings.SelectedCharacterClass = className;
                GameManager.Instance.SaveSettings();
            }

            // Notify listeners
            OnClassChanged?.Invoke(currentClass);
        }

        private void ResetCharacterSystems() {
            // Reset skill points
            if (skillPointManager != null) {
                skillPointManager.ResetSkills();
            }

            // Reset stats
            if (statAllocationSystem != null) {
                statAllocationSystem.ResetStats();
            }

            // Reset special stats
            if (specialStatSystem != null) {
                specialStatSystem.ClearAllStatModifiers();
            }

            // In a real implementation, we might also need to:
            // - Reset equipment
            // - Reset abilities
            // - Update character visuals
            // - etc.
        }

        public BaseCharacterClass GetCurrentClass() {
            return currentClass;
        }

        public List<string> GetAvailableClasses() {
            return new List<string>(classPrefabs.Keys);
        }

        public bool IsClassAvailable(string className) {
            return classPrefabs.ContainsKey(className);
        }
    }
}