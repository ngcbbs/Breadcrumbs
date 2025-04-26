using System;
using UnityEngine;
using GamePortfolio.Core;
using GamePortfolio.Network;
using GamePortfolio.Network.GameHub;
using GameState = GamePortfolio.Core.GameState;

namespace GamePortfolio.Gameplay.Character {
    /// <summary>
    /// Manages player character statistics
    /// </summary>
    public class PlayerStats : MonoBehaviour {
        [Header("Base Stats")]
        [SerializeField]
        private int maxHealth = 100;
        [SerializeField]
        private int maxStamina = 100;
        [SerializeField]
        private float moveSpeed = 5f;
        [SerializeField]
        private float attackSpeed = 1f;
        [SerializeField]
        private int attackDamage = 10;
        [SerializeField]
        private float attackRange = 2f;

        [Header("Progression")]
        [SerializeField]
        private int level = 1;
        [SerializeField]
        private int experience = 0;
        [SerializeField]
        private int experienceToNextLevel = 100;
        [SerializeField]
        private float healthGrowth = 10f;
        [SerializeField]
        private float staminaGrowth = 5f;
        [SerializeField]
        private float damageGrowth = 2f;

        [Header("Resources")]
        [SerializeField]
        private int gold = 0;
        [SerializeField]
        private int keys = 0;

        // Current values
        private int currentHealth;
        private int currentStamina;

        // Stat modifiers (from equipment, buffs, etc.)
        private int healthModifier = 0;
        private int staminaModifier = 0;
        private float moveSpeedModifier = 0f;
        private float attackSpeedModifier = 0f;
        private int attackDamageModifier = 0;
        private float attackRangeModifier = 0f;

        // Last damage time for invulnerability
        private float lastDamageTime = 0f;
        [SerializeField]
        private float invulnerabilityTime = 0.5f;

        // Events
        public event Action<int, int> OnHealthChanged;
        public event Action<int, int> OnStaminaChanged;
        public event Action<int> OnLevelChanged;
        public event Action<int> OnGoldChanged;
        public event Action<int> OnExperienceChanged;
        public event Action OnDeath;

        // Properties for stats with modifiers
        public int MaxHealth => maxHealth + healthModifier;
        public int MaxStamina => maxStamina + staminaModifier;
        public float MoveSpeed => moveSpeed + moveSpeedModifier;
        public float AttackSpeed => attackSpeed + attackSpeedModifier;
        public int AttackDamage => attackDamage + attackDamageModifier;
        public float AttackRange => attackRange + attackRangeModifier;

        // Current value properties
        public int CurrentHealth => currentHealth;
        public int CurrentStamina => currentStamina;
        public int Level => level;
        public int Experience => experience;
        public int ExperienceToNextLevel => experienceToNextLevel;
        public int Gold => gold;
        public int Keys => keys;

        private void Awake() {
            // Initialize current values to max
            currentHealth = MaxHealth;
            currentStamina = MaxStamina;
        }

        private void Start() {
            // Trigger initial events
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
            OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
            OnLevelChanged?.Invoke(level);
            OnGoldChanged?.Invoke(gold);
            OnExperienceChanged?.Invoke(experience);
        }

        /// <summary>
        /// Take damage
        /// </summary>
        /// <param name="amount">Damage amount</param>
        /// <param name="damageSource">Source of damage</param>
        /// <returns>Actual damage taken</returns>
        public int TakeDamage(int amount, GameObject damageSource = null) {
            // Check invulnerability
            if (Time.time - lastDamageTime < invulnerabilityTime) {
                return 0;
            }

            // Update last damage time
            lastDamageTime = Time.time;

            // Apply damage
            int damageTaken = Mathf.Min(currentHealth, amount);
            currentHealth -= damageTaken;

            // Notify listeners
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);

            // Play damage sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("PlayerHit");
            }

            // Check for death
            if (currentHealth <= 0) {
                Die();
            }

            return damageTaken;
        }

        /// <summary>
        /// Heal health
        /// </summary>
        /// <param name="amount">Heal amount</param>
        /// <returns>Actual amount healed</returns>
        public int Heal(int amount) {
            int startHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);

            // Calculate actual healing done
            int healingDone = currentHealth - startHealth;

            // Notify listeners if healing occurred
            if (healingDone > 0) {
                OnHealthChanged?.Invoke(currentHealth, MaxHealth);

                // Play heal sound
                if (AudioManager.HasInstance) {
                    AudioManager.Instance.PlaySfx("Heal");
                }
            }

            return healingDone;
        }

        /// <summary>
        /// Use stamina
        /// </summary>
        /// <param name="amount">Stamina amount to use</param>
        /// <returns>Whether stamina was successfully used</returns>
        public bool UseStamina(int amount) {
            if (currentStamina >= amount) {
                currentStamina -= amount;
                OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Restore stamina
        /// </summary>
        /// <param name="amount">Amount to restore</param>
        /// <returns>Actual amount restored</returns>
        public int RestoreStamina(int amount) {
            int startStamina = currentStamina;
            currentStamina = Mathf.Min(currentStamina + amount, MaxStamina);

            // Calculate actual restoration
            int staminaRestored = currentStamina - startStamina;

            // Notify listeners if restoration occurred
            if (staminaRestored > 0) {
                OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
            }

            return staminaRestored;
        }

        /// <summary>
        /// Add experience points
        /// </summary>
        /// <param name="amount">Experience amount</param>
        /// <returns>Whether level up occurred</returns>
        public bool AddExperience(int amount) {
            experience += amount;
            OnExperienceChanged?.Invoke(experience);

            // Check for level up
            bool leveledUp = false;
            while (experience >= experienceToNextLevel) {
                // Level up
                LevelUp();
                leveledUp = true;
            }

            return leveledUp;
        }

        /// <summary>
        /// Add gold
        /// </summary>
        /// <param name="amount">Gold amount</param>
        public void AddGold(int amount) {
            if (amount <= 0) return;

            gold += amount;
            OnGoldChanged?.Invoke(gold);

            // Play gold pickup sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("CoinPickup");
            }
        }

        /// <summary>
        /// Spend gold
        /// </summary>
        /// <param name="amount">Gold amount</param>
        /// <returns>Whether gold was successfully spent</returns>
        public bool SpendGold(int amount) {
            if (gold >= amount) {
                gold -= amount;
                OnGoldChanged?.Invoke(gold);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a key
        /// </summary>
        public void AddKey() {
            keys++;

            // Play key pickup sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("KeyPickup");
            }
        }

        /// <summary>
        /// Use a key
        /// </summary>
        /// <returns>Whether key was successfully used</returns>
        public bool UseKey() {
            if (keys > 0) {
                keys--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Level up the character
        /// </summary>
        private void LevelUp() {
            level++;

            // Increase stats based on growth rates
            maxHealth += Mathf.RoundToInt(healthGrowth);
            maxStamina += Mathf.RoundToInt(staminaGrowth);
            attackDamage += Mathf.RoundToInt(damageGrowth);

            // Heal to full on level up
            currentHealth = MaxHealth;
            currentStamina = MaxStamina;

            // Calculate new experience for next level
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.5f);

            // Notify listeners
            OnLevelChanged?.Invoke(level);
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
            OnStaminaChanged?.Invoke(currentStamina, MaxStamina);

            // Play level up sound and effect
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("LevelUp");
            }
        }

        /// <summary>
        /// Handle death
        /// </summary>
        private void Die() {
            // Notify about death
            OnDeath?.Invoke();

            // Play death sound
            if (AudioManager.HasInstance) {
                AudioManager.Instance.PlaySfx("PlayerDeath");
            }

            // Set game over state if game manager exists
            if (GameManager.HasInstance) {
                GameManager.Instance.ChangeState(GameState.GameOver);
            }
        }

        /// <summary>
        /// Add a stat modifier
        /// </summary>
        /// <param name="statType">Type of stat</param>
        /// <param name="value">Modifier value</param>
        public void AddStatModifier(StatType statType, float value) {
            switch (statType) {
                case StatType.Health:
                    healthModifier += Mathf.RoundToInt(value);
                    // Update current health percentage
                    float healthPercent = (float)currentHealth / (MaxHealth - Mathf.RoundToInt(value));
                    currentHealth = Mathf.RoundToInt(healthPercent * MaxHealth);
                    OnHealthChanged?.Invoke(currentHealth, MaxHealth);
                    break;

                case StatType.Stamina:
                    staminaModifier += Mathf.RoundToInt(value);
                    // Update current stamina percentage
                    float staminaPercent = (float)currentStamina / (MaxStamina - Mathf.RoundToInt(value));
                    currentStamina = Mathf.RoundToInt(staminaPercent * MaxStamina);
                    OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
                    break;

                case StatType.MoveSpeed:
                    moveSpeedModifier += value;
                    break;

                case StatType.AttackSpeed:
                    attackSpeedModifier += value;
                    break;

                case StatType.AttackDamage:
                    attackDamageModifier += Mathf.RoundToInt(value);
                    break;

                case StatType.AttackRange:
                    attackRangeModifier += value;
                    break;
            }
        }

        /// <summary>
        /// Remove a stat modifier
        /// </summary>
        /// <param name="statType">Type of stat</param>
        /// <param name="value">Modifier value</param>
        public void RemoveStatModifier(StatType statType, float value) {
            switch (statType) {
                case StatType.Health:
                    // Update current health percentage before changing max
                    float healthPercent = (float)currentHealth / MaxHealth;
                    healthModifier -= Mathf.RoundToInt(value);
                    currentHealth = Mathf.RoundToInt(healthPercent * MaxHealth);
                    currentHealth = Mathf.Min(currentHealth, MaxHealth);
                    OnHealthChanged?.Invoke(currentHealth, MaxHealth);
                    break;

                case StatType.Stamina:
                    // Update current stamina percentage before changing max
                    float staminaPercent = (float)currentStamina / MaxStamina;
                    staminaModifier -= Mathf.RoundToInt(value);
                    currentStamina = Mathf.RoundToInt(staminaPercent * MaxStamina);
                    currentStamina = Mathf.Min(currentStamina, MaxStamina);
                    OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
                    break;

                case StatType.MoveSpeed:
                    moveSpeedModifier -= value;
                    break;

                case StatType.AttackSpeed:
                    attackSpeedModifier -= value;
                    break;

                case StatType.AttackDamage:
                    attackDamageModifier -= Mathf.RoundToInt(value);
                    break;

                case StatType.AttackRange:
                    attackRangeModifier -= value;
                    break;
            }
        }

        /// <summary>
        /// Reset all stats to base values
        /// </summary>
        public void ResetStats() {
            healthModifier = 0;
            staminaModifier = 0;
            moveSpeedModifier = 0f;
            attackSpeedModifier = 0f;
            attackDamageModifier = 0;
            attackRangeModifier = 0f;

            currentHealth = MaxHealth;
            currentStamina = MaxStamina;

            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
            OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
        }

        /// <summary>
        /// Serialize player stats to network format
        /// </summary>
        /// <returns>PlayerInfo for network</returns>
        public PlayerInfo ToPlayerInfo() {
            if (!NetworkManager.HasInstance) return null;

            return new PlayerInfo {
                PlayerId = NetworkManager.Instance.PlayerId,
                PlayerName = GameManager.HasInstance ? GameManager.Instance.Settings.PlayerName : "Player",
                CharacterClass = GameManager.HasInstance ? GameManager.Instance.Settings.SelectedCharacterClass : "Warrior",
                Position = transform.position,
                Rotation = transform.rotation,
                Level = level,
                Health = currentHealth,
                MaxHealth = MaxHealth,
                Status = PlayerStatus.Online
            };
        }
    }

    /// <summary>
    /// Types of character statistics
    /// </summary>
    public enum StatType {
        Health,
        Stamina,
        MoveSpeed,
        AttackSpeed,
        AttackDamage,
        AttackRange
    }
}