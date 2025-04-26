using System;
using System.Collections;
using GamePortfolio.Core;
using UnityEngine;
using GamePortfolio.Dungeon.Generation;
using GamePortfolio.Dungeon.Visualization;

namespace GamePortfolio.Dungeon
{
    /// <summary>
    /// Manages dungeon generation, loading, and state
    /// </summary>
    public class DungeonManager : MonoBehaviour
    {
        [Header("Generator Settings")]
        [SerializeField] private DungeonGenerationSettings generationSettings;
        
        [Header("Visualization")]
        [SerializeField] private DungeonVisualizer visualizer;
        
        // Current active dungeon
        private Generation.Dungeon currentDungeon;
        
        // Current dungeon data
        public Generation.Dungeon CurrentDungeon => currentDungeon;
        
        // Events
        public event Action<Generation.Dungeon> OnDungeonGenerated;
        public event Action<Generation.Dungeon> OnDungeonLoaded;
        public event Action OnDungeonCleared;
        
        private bool isInitialized = false;
        
        /// <summary>
        /// Initialize the dungeon manager
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;
            
            // Create defaults if not set
            if (generationSettings == null)
            {
                generationSettings = ScriptableObject.CreateInstance<DungeonGenerationSettings>();
            }
            
            if (visualizer == null)
            {
                visualizer = GetComponent<DungeonVisualizer>();
                if (visualizer == null && gameObject.transform.childCount > 0)
                {
                    visualizer = gameObject.transform.GetChild(0).GetComponent<DungeonVisualizer>();
                }
            }
            
            isInitialized = true;
            Debug.Log("DungeonManager initialized");
        }
        
        /// <summary>
        /// Generate a new dungeon using the current settings
        /// </summary>
        public void GenerateDungeon()
        {
            // Get seed from settings
            int seed = GetSeed();
            
            // Create dungeon generator
            DungeonGenerator generator = new DungeonGenerator
            {
                Width = generationSettings.Width,
                Height = generationSettings.Height,
                MinRoomSize = generationSettings.MinRoomSize,
                MaxDepth = generationSettings.MaxDepth,
                SplitRandomness = generationSettings.SplitRandomness,
                RoomPadding = generationSettings.RoomPadding,
                CorridorWidth = generationSettings.CorridorWidth
            };
            
            // Generate dungeon
            currentDungeon = generator.GenerateDungeon(seed);
            
            // Set special rooms if enabled
            if (generationSettings.GenerateSpecialRooms)
            {
                SetSpecialRooms(currentDungeon);
            }
            
            // Visualize dungeon
            if (visualizer != null)
            {
                visualizer.VisualizeDungeon(currentDungeon);
            }
            
            // Trigger event
            OnDungeonGenerated?.Invoke(currentDungeon);
            
            Debug.Log($"Generated dungeon with seed {seed}: {currentDungeon.Rooms.Count} rooms, {currentDungeon.Corridors.Count} corridors");
        }
        
        /// <summary>
        /// Coroutine for generating dungeon with progress reporting
        /// </summary>
        public IEnumerator GenerateDungeonRoutine()
        {
            // Get seed from settings
            int seed = GetSeed();
            
            // Create dungeon generator
            DungeonGenerator generator = new DungeonGenerator
            {
                Width = generationSettings.Width,
                Height = generationSettings.Height,
                MinRoomSize = generationSettings.MinRoomSize,
                MaxDepth = generationSettings.MaxDepth,
                SplitRandomness = generationSettings.SplitRandomness,
                RoomPadding = generationSettings.RoomPadding,
                CorridorWidth = generationSettings.CorridorWidth
            };
            
            // Step 1: Generate BSP tree
            yield return null;
            
            // Step 2: Generate dungeon
            currentDungeon = generator.GenerateDungeon(seed);
            yield return null;
            
            // Step 3: Set special rooms if enabled
            if (generationSettings.GenerateSpecialRooms)
            {
                SetSpecialRooms(currentDungeon);
            }
            yield return null;
            
            // Step 4: Visualize dungeon
            if (visualizer != null)
            {
                yield return StartCoroutine(visualizer.VisualizeDungeonRoutine(currentDungeon));
            }
            
            // Trigger event
            OnDungeonGenerated?.Invoke(currentDungeon);
            
            Debug.Log($"Generated dungeon with seed {seed}: {currentDungeon.Rooms.Count} rooms, {currentDungeon.Corridors.Count} corridors");
        }
        
        /// <summary>
        /// Load a previously generated dungeon
        /// </summary>
        public void LoadDungeon(Generation.DungeonData dungeonData)
        {
            // Deserialize dungeon data
            Generation.Dungeon loadedDungeon = new Generation.Dungeon(dungeonData);
            
            // Set as current dungeon
            currentDungeon = loadedDungeon;
            
            // Visualize dungeon
            if (visualizer != null)
            {
                visualizer.VisualizeDungeon(currentDungeon);
            }
            
            // Trigger event
            OnDungeonLoaded?.Invoke(currentDungeon);
            
            Debug.Log($"Loaded dungeon with seed {dungeonData.Seed}: {currentDungeon.Rooms.Count} rooms, {currentDungeon.Corridors.Count} corridors");
        }
        
        /// <summary>
        /// Clear the current dungeon
        /// </summary>
        public void ClearDungeon()
        {
            // Clear visualization
            if (visualizer != null)
            {
                visualizer.ClearDungeon();
            }
            
            // Clear current dungeon
            currentDungeon = null;
            
            // Trigger event
            OnDungeonCleared?.Invoke();
            
            Debug.Log("Dungeon cleared");
        }
        
        /// <summary>
        /// Get the current dungeon
        /// </summary>
        public Generation.Dungeon GetCurrentDungeon()
        {
            return currentDungeon;
        }
        
        /// <summary>
        /// Get seed for dungeon generation
        /// </summary>
        private int GetSeed()
        {
            int seed = generationSettings.Seed;
            
            // Use random seed if enabled
            if (generationSettings.UseRandomSeed)
            {
                seed = UnityEngine.Random.Range(0, 999999);
            }
            // Or use from game settings if available
            else if (GameManager.HasInstance && GameManager.Instance.Settings != null)
            {
                seed = GameManager.Instance.Settings.GetDungeonSeed();
            }
            
            return seed;
        }
        
        /// <summary>
        /// Set special rooms in the dungeon
        /// </summary>
        private void SetSpecialRooms(Generation.Dungeon dungeon)
        {
            if (dungeon.Rooms.Count <= 0) return;
            
            // Find the largest and most distant rooms for special purposes
            Room largestRoom = null;
            Room mostDistantRoom = null;
            Room startRoom = null;
            Vector2Int center = new Vector2Int(dungeon.Width / 2, dungeon.Height / 2);
            float maxSize = 0f;
            float maxDistance = 0f;
            
            foreach (Room room in dungeon.Rooms)
            {
                // Find largest room
                float roomSize = room.Bounds.width * room.Bounds.height;
                if (roomSize > maxSize)
                {
                    maxSize = roomSize;
                    largestRoom = room;
                }
                
                // Find most distant room from center
                Vector2Int roomCenter = room.GetCenter();
                float distance = Vector2.Distance(roomCenter, center);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    mostDistantRoom = room;
                }
                
                // Find room closest to (0,0) for start room
                float distanceToOrigin = Vector2.Distance(roomCenter, Vector2.zero);
                if (startRoom == null || distanceToOrigin < Vector2.Distance(startRoom.GetCenter(), Vector2.zero))
                {
                    startRoom = room;
                }
            }
            
            // Set special rooms
            if (generationSettings.GenerateEntranceRoom && startRoom != null)
            {
                startRoom.Type = RoomType.Entrance;
            }
            
            if (generationSettings.GenerateExitRoom && mostDistantRoom != null)
            {
                mostDistantRoom.Type = RoomType.Exit;
            }
            
            // Set boss room (usually the largest)
            if (largestRoom != null && largestRoom != startRoom && largestRoom != mostDistantRoom)
            {
                largestRoom.Type = RoomType.Boss;
            }
            
            // Set treasure rooms
            int treasureRoomsSet = 0;
            int attempts = 0;
            
            while (treasureRoomsSet < generationSettings.TreasureRoomCount && attempts < dungeon.Rooms.Count * 2)
            {
                attempts++;
                
                int randomIndex = UnityEngine.Random.Range(0, dungeon.Rooms.Count);
                Room randomRoom = dungeon.Rooms[randomIndex];
                
                // Skip if room is already a special room
                if (randomRoom.Type != RoomType.Normal)
                {
                    continue;
                }
                
                // Set as treasure room
                randomRoom.Type = RoomType.Treasure;
                treasureRoomsSet++;
            }
        }
    }
}
