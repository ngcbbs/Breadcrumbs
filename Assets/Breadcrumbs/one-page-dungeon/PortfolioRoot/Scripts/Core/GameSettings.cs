using UnityEngine;

namespace GamePortfolio.Core {
    /// <summary>
    /// Stores global game settings that affect gameplay and configuration
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "GamePortfolio/Game Settings")]
    public class GameSettings : ScriptableObject {
        [Header("Game Configuration")]
        public GameMode GameMode = GameMode.SinglePlayer;
        public string PlayerName = "Player";
        public string SelectedCharacterClass = "Warrior";

        [Header("Server Configuration")]
        public string ServerAddress = "localhost";
        public int ServerPort = 12345;

        [Header("Graphics Settings")]
        public bool FullScreen = true;
        public int TargetFrameRate = 60;
        public QualityLevel QualityLevel = QualityLevel.Medium;

        [Header("Audio Settings")]
        [Range(0f, 1f)]
        public float MasterVolume = 1.0f;
        [Range(0f, 1f)]
        public float MusicVolume = 0.8f;
        [Range(0f, 1f)]
        public float SfxVolume = 1.0f;

        [Header("Gameplay Settings")]
        [Range(0.1f, 2.0f)]
        public float GameSpeed = 1.0f;
        public bool ShowTutorial = true;
        public bool ShowHints = true;
        public DifficultyLevel Difficulty = DifficultyLevel.Normal;

        [Header("Dungeon Generation")]
        public int DungeonSeed = 0;
        public bool UseRandomSeed = true;
        public int DungeonWidth = 100;
        public int DungeonHeight = 100;
        public int RoomCount = 10;
        public int MaxDepth = 5;

        /// <summary>
        /// Reset settings to defaults
        /// </summary>
        public void SetDefaults() {
            GameMode = GameMode.SinglePlayer;
            PlayerName = "Player";
            SelectedCharacterClass = "Warrior";

            ServerAddress = "localhost";
            ServerPort = 12345;

            FullScreen = true;
            TargetFrameRate = 60;
            QualityLevel = QualityLevel.Medium;

            MasterVolume = 1.0f;
            MusicVolume = 0.8f;
            SfxVolume = 1.0f;

            GameSpeed = 1.0f;
            ShowTutorial = true;
            ShowHints = true;
            Difficulty = DifficultyLevel.Normal;

            UseRandomSeed = true;
            DungeonSeed = 0;
            DungeonWidth = 100;
            DungeonHeight = 100;
            RoomCount = 10;
            MaxDepth = 5;
        }

        /// <summary>
        /// Apply settings to the game
        /// </summary>
        public void ApplySettings() {
            // Apply graphics settings
            Screen.fullScreen = FullScreen;
            Application.targetFrameRate = TargetFrameRate;
            QualitySettings.SetQualityLevel((int)QualityLevel, true);

            // Apply time scale
            Time.timeScale = GameSpeed;

            // Apply audio settings through AudioManager
            if (AudioManager.HasInstance) {
                AudioManager.Instance.SetMasterVolume(MasterVolume);
                AudioManager.Instance.SetMusicVolume(MusicVolume);
                AudioManager.Instance.SetSfxVolume(SfxVolume);
            }
        }

        /// <summary>
        /// Generate a random seed for dungeon generation if using random seed
        /// </summary>
        public int GetDungeonSeed() {
            if (UseRandomSeed) {
                DungeonSeed = Random.Range(0, 999999);
            }

            return DungeonSeed;
        }
    }

    /// <summary>
    /// Quality levels for graphics settings
    /// </summary>
    public enum QualityLevel {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }

    /// <summary>
    /// Difficulty levels
    /// </summary>
    public enum DifficultyLevel {
        Easy,
        Normal,
        Hard,
        Nightmare
    }
}