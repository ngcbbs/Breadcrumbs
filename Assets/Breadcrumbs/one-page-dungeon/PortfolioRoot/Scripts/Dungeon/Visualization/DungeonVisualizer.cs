using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePortfolio.Dungeon.Generation;

namespace GamePortfolio.Dungeon.Visualization
{
    /// <summary>
    /// Visualizes a generated dungeon in the Unity scene
    /// </summary>
    public class DungeonVisualizer : MonoBehaviour
    {
        [Header("Parent Transforms")]
        [SerializeField] private Transform floorParent;
        [SerializeField] private Transform wallParent;
        [SerializeField] private Transform propParent;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject doorPrefab;
        [SerializeField] private GameObject stairUpPrefab;
        [SerializeField] private GameObject stairDownPrefab;
        [SerializeField] private GameObject waterPrefab;
        [SerializeField] private GameObject lavaPrefab;
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private GameObject shopPrefab;
        [SerializeField] private GameObject chestPrefab;
        
        [Header("Materials")]
        [SerializeField] private Material defaultFloorMaterial;
        [SerializeField] private Material defaultWallMaterial;
        [SerializeField] private Material doorMaterial;
        [SerializeField] private Material specialRoomMaterial;
        
        [Header("Visualization Settings")]
        [SerializeField] private bool usePooling = true;
        [SerializeField] private int batchSize = 100;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private float visualizationDelay = 0.05f;
        
        // Object pools for reusing GameObjects
        private Dictionary<TileType, List<GameObject>> objectPools = new Dictionary<TileType, List<GameObject>>();
        
        // Currently active dungeon objects
        private List<GameObject> activeObjects = new List<GameObject>();
        
        // Track instantiated parent transforms
        private bool parentsCreated = false;
        
        private void Awake()
        {
            // Create parent transforms if not assigned
            CreateParentTransforms();
        }
        
        /// <summary>
        /// Create necessary parent transforms for organization
        /// </summary>
        private void CreateParentTransforms()
        {
            if (parentsCreated) return;
            
            if (floorParent == null)
            {
                GameObject floorParentObj = new GameObject("Floors");
                floorParentObj.transform.SetParent(transform);
                floorParent = floorParentObj.transform;
            }
            
            if (wallParent == null)
            {
                GameObject wallParentObj = new GameObject("Walls");
                wallParentObj.transform.SetParent(transform);
                wallParent = wallParentObj.transform;
            }
            
            if (propParent == null)
            {
                GameObject propParentObj = new GameObject("Props");
                propParentObj.transform.SetParent(transform);
                propParent = propParentObj.transform;
            }
            
            parentsCreated = true;
        }
        
        /// <summary>
        /// Visualize a dungeon immediately
        /// </summary>
        /// <param name="dungeon">Dungeon to visualize</param>
        public void VisualizeDungeon(Generation.Dungeon dungeon)
        {
            StartCoroutine(VisualizeDungeonRoutine(dungeon));
        }
        
        /// <summary>
        /// Visualize a dungeon with a coroutine for gradual loading
        /// </summary>
        /// <param name="dungeon">Dungeon to visualize</param>
        /// <returns>IEnumerator for coroutine</returns>
        public IEnumerator VisualizeDungeonRoutine(Generation.Dungeon dungeon)
        {
            // Clear any existing dungeon
            ClearDungeon();
            
            // Create parent transforms if needed
            CreateParentTransforms();
            
            // Get tile grid
            TileType[,] grid = dungeon.CreateTileGrid();
            
            // Calculate total tiles to render
            int totalTiles = 0;
            for (int x = 0; x < dungeon.Width; x++)
            {
                for (int y = 0; y < dungeon.Height; y++)
                {
                    if (grid[x, y] != TileType.Wall || ShouldRenderWall(grid, x, y, dungeon.Width, dungeon.Height))
                    {
                        totalTiles++;
                    }
                }
            }
            
            // Pre-allocate object pools if using pooling
            if (usePooling)
            {
                InitializeObjectPools(totalTiles);
            }
            
            // Counter for batch processing
            int tilesProcessed = 0;
            
            // Visualize tile grid
            for (int x = 0; x < dungeon.Width; x++)
            {
                for (int y = 0; y < dungeon.Height; y++)
                {
                    TileType tileType = grid[x, y];
                    
                    // Skip walls that don't need to be rendered (not adjacent to non-walls)
                    if (tileType == TileType.Wall && !ShouldRenderWall(grid, x, y, dungeon.Width, dungeon.Height))
                    {
                        continue;
                    }
                    
                    // Create game object for this tile
                    InstantiateTile(tileType, new Vector3(x * tileSize, 0, y * tileSize));
                    tilesProcessed++;
                    
                    // Yield every batch size tiles
                    if (tilesProcessed % batchSize == 0)
                    {
                        yield return new WaitForSeconds(visualizationDelay);
                    }
                }
            }
            
            // Add special props for rooms
            yield return StartCoroutine(PlaceRoomProps(dungeon));
            
            Debug.Log($"Dungeon visualization complete: {tilesProcessed} tiles visualized.");
        }
        
        /// <summary>
        /// Place props in rooms based on room type
        /// </summary>
        /// <param name="dungeon">Dungeon to visualize</param>
        /// <returns>IEnumerator for coroutine</returns>
        private IEnumerator PlaceRoomProps(Generation.Dungeon dungeon)
        {
            foreach (Room room in dungeon.Rooms)
            {
                Vector2Int center = room.GetCenter();
                Vector3 centerPosition = new Vector3(center.x * tileSize, 0, center.y * tileSize);
                
                switch (room.Type)
                {
                    case RoomType.Entrance:
                        // Place stairs up at entrance
                        InstantiateProp(stairUpPrefab, centerPosition + new Vector3(0, 0.01f, 0));
                        break;
                        
                    case RoomType.Exit:
                        // Place stairs down at exit
                        InstantiateProp(stairDownPrefab, centerPosition + new Vector3(0, 0.01f, 0));
                        break;
                        
                    case RoomType.Shop:
                        // Place shop prop
                        InstantiateProp(shopPrefab, centerPosition + new Vector3(0, 0.01f, 0));
                        break;
                        
                    case RoomType.Treasure:
                        // Place chest in the center of treasure room
                        InstantiateProp(chestPrefab, centerPosition + new Vector3(0, 0.01f, 0));
                        break;
                        
                    case RoomType.Boss:
                        // Add some visual indicator for boss room
                        ColorRoomFloor(room, Color.red);
                        break;
                        
                    case RoomType.Secret:
                        // Maybe no props for secret rooms to keep them hidden
                        break;
                }
                
                // Yield occasionally to prevent freezing
                if (room.Type != RoomType.Normal)
                {
                    yield return null;
                }
            }
        }
        
        /// <summary>
        /// Color the floor of a room with a specific color
        /// </summary>
        /// <param name="room">Room to color</param>
        /// <param name="color">Color to apply</param>
        private void ColorRoomFloor(Room room, Color color)
        {
            Material coloredMaterial = null;
            
            if (specialRoomMaterial != null)
            {
                coloredMaterial = new Material(specialRoomMaterial);
                coloredMaterial.color = color;
            }
            
            // Find floor objects in this room
            foreach (GameObject obj in activeObjects)
            {
                if (obj != null && obj.transform.parent == floorParent)
                {
                    Vector2Int position = new Vector2Int(
                        Mathf.RoundToInt(obj.transform.position.x / tileSize),
                        Mathf.RoundToInt(obj.transform.position.z / tileSize)
                    );
                    
                    if (room.Contains(position))
                    {
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer != null && coloredMaterial != null)
                        {
                            renderer.material = coloredMaterial;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Determine if a wall tile should be rendered
        /// (only render walls that are adjacent to non-wall tiles)
        /// </summary>
        private bool ShouldRenderWall(TileType[,] grid, int x, int y, int width, int height)
        {
            // Check all adjacent tiles
            for (int nx = Mathf.Max(0, x - 1); nx <= Mathf.Min(width - 1, x + 1); nx++)
            {
                for (int ny = Mathf.Max(0, y - 1); ny <= Mathf.Min(height - 1, y + 1); ny++)
                {
                    // If an adjacent tile is not a wall, we should render this wall
                    if (grid[nx, ny] != TileType.Wall)
                    {
                        return true;
                    }
                }
            }
            
            // No non-wall adjacent tiles, no need to render
            return false;
        }
        
        /// <summary>
        /// Initialize object pools for tile types
        /// </summary>
        /// <param name="estimatedCount">Estimated count of objects</param>
        private void InitializeObjectPools(int estimatedCount)
        {
            objectPools.Clear();
            
            // Create pools for each tile type
            objectPools[TileType.Floor] = new List<GameObject>();
            objectPools[TileType.Wall] = new List<GameObject>();
            objectPools[TileType.Door] = new List<GameObject>();
            objectPools[TileType.Entrance] = new List<GameObject>();
            objectPools[TileType.Exit] = new List<GameObject>();
            objectPools[TileType.Water] = new List<GameObject>();
            objectPools[TileType.Lava] = new List<GameObject>();
            objectPools[TileType.Trap] = new List<GameObject>();
            objectPools[TileType.Shop] = new List<GameObject>();
            objectPools[TileType.Chest] = new List<GameObject>();
        }
        
        /// <summary>
        /// Instantiate a tile GameObject for a specific tile type
        /// </summary>
        /// <param name="tileType">Type of tile</param>
        /// <param name="position">World position</param>
        /// <returns>Instantiated GameObject</returns>
        private GameObject InstantiateTile(TileType tileType, Vector3 position)
        {
            GameObject tilePrefab = null;
            Transform parent = null;
            Material material = null;
            
            // Set prefab and parent based on tile type
            switch (tileType)
            {
                case TileType.Floor:
                    tilePrefab = floorPrefab;
                    parent = floorParent;
                    material = defaultFloorMaterial;
                    break;
                    
                case TileType.Wall:
                    tilePrefab = wallPrefab;
                    parent = wallParent;
                    material = defaultWallMaterial;
                    break;
                    
                case TileType.Door:
                    tilePrefab = doorPrefab;
                    parent = wallParent;
                    material = doorMaterial;
                    break;
                    
                case TileType.Water:
                    tilePrefab = waterPrefab;
                    parent = floorParent;
                    break;
                    
                case TileType.Lava:
                    tilePrefab = lavaPrefab;
                    parent = floorParent;
                    break;
                    
                case TileType.Trap:
                    tilePrefab = trapPrefab;
                    parent = floorParent;
                    break;
                    
                // For these types, use floor tile and add props separately
                case TileType.Entrance:
                case TileType.Exit:
                case TileType.Shop:
                case TileType.Chest:
                    tilePrefab = floorPrefab;
                    parent = floorParent;
                    material = defaultFloorMaterial;
                    break;
                    
                default:
                    tilePrefab = floorPrefab;
                    parent = floorParent;
                    material = defaultFloorMaterial;
                    break;
            }
            
            // Guard against missing prefab
            if (tilePrefab == null)
            {
                Debug.LogWarning($"Missing prefab for tile type {tileType}");
                return null;
            }
            
            // Get from pool or instantiate
            GameObject tileObject = GetObjectFromPool(tileType);
            if (tileObject == null)
            {
                tileObject = Instantiate(tilePrefab, position, Quaternion.identity, parent);
                tileObject.name = $"{tileType}_{activeObjects.Count}";
            }
            else
            {
                tileObject.transform.position = position;
                tileObject.transform.rotation = Quaternion.identity;
                tileObject.transform.SetParent(parent);
                tileObject.SetActive(true);
            }
            
            // Set material if available
            if (material != null)
            {
                Renderer renderer = tileObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = material;
                }
            }
            
            // Add to active objects
            activeObjects.Add(tileObject);
            
            return tileObject;
        }
        
        /// <summary>
        /// Instantiate a prop GameObject
        /// </summary>
        /// <param name="propPrefab">Prefab to instantiate</param>
        /// <param name="position">World position</param>
        /// <returns>Instantiated GameObject</returns>
        private GameObject InstantiateProp(GameObject propPrefab, Vector3 position)
        {
            if (propPrefab == null)
            {
                Debug.LogWarning("Missing prop prefab");
                return null;
            }
            
            GameObject propObject = Instantiate(propPrefab, position, propPrefab.transform.rotation, propParent);
            propObject.name = $"Prop_{activeObjects.Count}";
            
            // Add to active objects
            activeObjects.Add(propObject);
            
            return propObject;
        }
        
        /// <summary>
        /// Get an object from the pool for a specific tile type
        /// </summary>
        /// <param name="tileType">Type of tile</param>
        /// <returns>GameObject from pool or null if none available</returns>
        private GameObject GetObjectFromPool(TileType tileType)
        {
            // Skip if not using pooling
            if (!usePooling) return null;
            
            // Skip if pool doesn't exist
            if (!objectPools.ContainsKey(tileType)) return null;
            
            List<GameObject> pool = objectPools[tileType];
            
            // Find inactive object in pool
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && !pool[i].activeInHierarchy)
                {
                    return pool[i];
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Return an object to its pool
        /// </summary>
        /// <param name="obj">GameObject to return</param>
        /// <param name="tileType">Type of tile</param>
        private void ReturnToPool(GameObject obj, TileType tileType)
        {
            // Skip if not using pooling
            if (!usePooling) return;
            
            // Skip if pool doesn't exist
            if (!objectPools.ContainsKey(tileType)) return;
            
            // Add to pool if not already there
            List<GameObject> pool = objectPools[tileType];
            if (!pool.Contains(obj))
            {
                pool.Add(obj);
            }
            
            // Deactivate object
            obj.SetActive(false);
        }
        
        /// <summary>
        /// Clear the current dungeon visualization
        /// </summary>
        public void ClearDungeon()
        {
            // If using pooling, return objects to pool
            if (usePooling)
            {
                foreach (GameObject obj in activeObjects)
                {
                    if (obj != null)
                    {
                        // Determine type from name (crude but effective for this purpose)
                        TileType type = TileType.Floor;
                        if (obj.name.StartsWith("Wall")) type = TileType.Wall;
                        else if (obj.name.StartsWith("Door")) type = TileType.Door;
                        
                        ReturnToPool(obj, type);
                    }
                }
            }
            else
            {
                // Destroy all active objects
                foreach (GameObject obj in activeObjects)
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }
            
            // Clear active objects list
            activeObjects.Clear();
        }
    }
}
