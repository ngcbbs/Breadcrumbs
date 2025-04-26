using UnityEngine;

namespace GamePortfolio.Dungeon.Generation
{
    /// <summary>
    /// ScriptableObject that stores parameters for dungeon generation
    /// </summary>
    [CreateAssetMenu(fileName = "DungeonGenerationSettings", menuName = "GamePortfolio/Dungeon/Generation Settings")]
    public class DungeonGenerationSettings : ScriptableObject
    {
        [Header("Dungeon Size")]
        [Range(20, 200)]
        [Tooltip("Width of the dungeon in tiles")]
        public int Width = 100;
        
        [Range(20, 200)]
        [Tooltip("Height of the dungeon in tiles")]
        public int Height = 100;
        
        [Header("BSP Settings")]
        [Range(3, 20)]
        [Tooltip("Minimum size of a room")]
        public int MinRoomSize = 10;
        
        [Range(2, 10)]
        [Tooltip("Maximum recursion depth for BSP partitioning")]
        public int MaxDepth = 5;
        
        [Range(0f, 0.5f)]
        [Tooltip("Randomness factor for split positions (0 = middle split, 0.5 = fully random)")]
        public float SplitRandomness = 0.3f;
        
        [Header("Room Settings")]
        [Range(0, 5)]
        [Tooltip("Padding between room edge and BSP cell edge")]
        public int RoomPadding = 2;
        
        [Header("Corridor Settings")]
        [Range(1, 5)]
        [Tooltip("Width of corridors connecting rooms")]
        public int CorridorWidth = 2;
        
        [Header("Special Rooms")]
        [Tooltip("Whether to generate special rooms like entrance, exit, treasure rooms")]
        public bool GenerateSpecialRooms = true;
        
        [Tooltip("Always create entrance and exit rooms")]
        public bool GenerateEntranceRoom = true;
        
        [Tooltip("Always create entrance and exit rooms")]
        public bool GenerateExitRoom = true;
        
        [Range(0, 10)]
        [Tooltip("Number of treasure rooms to generate")]
        public int TreasureRoomCount = 2;
        
        [Header("Seed")]
        [Tooltip("Random seed for dungeon generation")]
        public int Seed = 0;
        
        [Tooltip("Whether to use a random seed each time")]
        public bool UseRandomSeed = true;
        
        /// <summary>
        /// Reset settings to default values
        /// </summary>
        public void ResetToDefaults()
        {
            Width = 100;
            Height = 100;
            MinRoomSize = 10;
            MaxDepth = 5;
            SplitRandomness = 0.3f;
            RoomPadding = 2;
            CorridorWidth = 2;
            GenerateSpecialRooms = true;
            GenerateEntranceRoom = true;
            GenerateExitRoom = true;
            TreasureRoomCount = 2;
            Seed = 0;
            UseRandomSeed = true;
        }
    }
}
