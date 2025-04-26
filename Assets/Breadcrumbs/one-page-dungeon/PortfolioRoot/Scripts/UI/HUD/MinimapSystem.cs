using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GamePortfolio.Dungeon;
using GamePortfolio.Dungeon.Generation;

namespace GamePortfolio.UI.HUD
{
    /// <summary>
    /// Manages the minimap system, including rendering, icons, and fog of war
    /// </summary>
    public class MinimapSystem : MonoBehaviour
    {
        [Header("Minimap Settings")]
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private RectTransform minimapPanel;
        [SerializeField] private RectTransform playerIcon;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Vector2 minimapSize = new Vector2(256, 256);
        [SerializeField] private float zoomLevel = 1f;
        [SerializeField] private Color fogOfWarColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color revealedAreaColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private Color visitedRoomColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        [SerializeField] private Color wallColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        [SerializeField] private Color doorColor = new Color(0.8f, 0.6f, 0.2f, 1.0f);
        
        [Header("Icon Prefabs")]
        [SerializeField] private GameObject enemyIconPrefab;
        [SerializeField] private GameObject treasureIconPrefab;
        [SerializeField] private GameObject exitIconPrefab;
        [SerializeField] private GameObject entranceIconPrefab;
        [SerializeField] private GameObject objectiveIconPrefab;
        [SerializeField] private GameObject customMarkerIconPrefab;
        
        // Rendering components
        private Texture2D minimapTexture;
        private Dictionary<GameObject, RectTransform> trackedEntities = new Dictionary<GameObject, RectTransform>();
        private List<Vector2Int> revealedTiles = new List<Vector2Int>();
        private List<Room> discoveredRooms = new List<Room>();
        private Dictionary<Vector2Int, CustomMarker> customMarkers = new Dictionary<Vector2Int, CustomMarker>();
        
        // Minimap state
        private bool isDragging = false;
        private Vector2 dragStartPosition;
        private Vector2 minimapOffset = Vector2.zero;
        private bool isRotating = false;
        private float currentRotation = 0f;
        private bool showFullMap = false;
        private DungeonManager dungeonManager;
        
        /// <summary>
        /// Initialize the minimap system
        /// </summary>
        private void Awake()
        {
            // Create initial minimap texture
            minimapTexture = new Texture2D((int)minimapSize.x, (int)minimapSize.y, TextureFormat.RGBA32, false);
            minimapTexture.filterMode = FilterMode.Point;
            
            // Assign texture to image
            if (minimapImage != null)
            {
                minimapImage.texture = minimapTexture;
            }
            
            // Find player if not assigned
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
            
            // Get dungeon manager
            dungeonManager = FindObjectOfType<DungeonManager>();
        }
        
        /// <summary>
        /// Start the minimap system
        /// </summary>
        private void Start()
        {
            // Initial minimap generation
            InitializeMinimapTexture();
            
            // Add event listeners
            if (minimapPanel != null)
            {
                // Add drag event handlers here if needed
            }
        }
        
        /// <summary>
        /// Update the minimap
        /// </summary>
        private void Update()
        {
            // Only update if we have a player to track
            if (playerTransform == null)
                return;
                
            // Update player icon position
            UpdatePlayerIcon();
            
            // Update tracked entities
            UpdateTrackedEntities();
            
            // Check if new tiles are revealed
            CheckForTileRevealing();
            
            // Handle minimap input
            HandleMinimapInput();
        }
        
        /// <summary>
        /// Initialize the minimap texture
        /// </summary>
        private void InitializeMinimapTexture()
        {
            // Fill the entire texture with fog of war
            Color[] colors = new Color[minimapTexture.width * minimapTexture.height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = fogOfWarColor;
            }
            minimapTexture.SetPixels(colors);
            minimapTexture.Apply();
            
            // If we have a dungeon manager, get dungeon data
            if (dungeonManager != null && dungeonManager.CurrentDungeon != null)
            {
                // Find entrance room and reveal it
                Room entranceRoom = dungeonManager.CurrentDungeon.GetRoomByType(RoomType.Entrance);
                if (entranceRoom != null)
                {
                    RevealRoom(entranceRoom);
                }
            }
        }
        
        /// <summary>
        /// Update the player icon position on the minimap
        /// </summary>
        private void UpdatePlayerIcon()
        {
            if (playerIcon == null || playerTransform == null)
                return;
                
            // Calculate player position on minimap
            Vector2 playerMinimapPos = WorldToMinimapPosition(playerTransform.position);
            
            // Apply position to player icon
            playerIcon.anchoredPosition = playerMinimapPos;
            
            // Rotate player icon to match player rotation if not in rotating minimap mode
            if (!isRotating)
            {
                playerIcon.rotation = Quaternion.Euler(0, 0, -playerTransform.eulerAngles.y);
            }
            else
            {
                // If rotating minimap, keep player icon facing up
                playerIcon.rotation = Quaternion.identity;
                
                // Rotate minimap instead
                minimapImage.rectTransform.rotation = Quaternion.Euler(0, 0, playerTransform.eulerAngles.y);
            }
        }
        
        /// <summary>
        /// Update tracked entities on the minimap
        /// </summary>
        private void UpdateTrackedEntities()
        {
            foreach (var entity in trackedEntities)
            {
                if (entity.Key == null)
                    continue;
                    
                // Calculate entity position on minimap
                Vector2 entityMinimapPos = WorldToMinimapPosition(entity.Key.transform.position);
                
                // Apply position to entity icon
                entity.Value.anchoredPosition = entityMinimapPos;
            }
        }
        
        /// <summary>
        /// Convert world position to minimap position
        /// </summary>
        private Vector2 WorldToMinimapPosition(Vector3 worldPos)
        {
            // Scaling factor based on dungeon size and minimap size
            float scaleFactor = 1.0f;
            if (dungeonManager != null && dungeonManager.CurrentDungeon != null)
            {
                int dungeonSize = Mathf.Max(dungeonManager.CurrentDungeon.Width, dungeonManager.CurrentDungeon.Height);
                scaleFactor = minimapSize.x / (dungeonSize * zoomLevel);
            }
            
            // Calculate minimap position
            Vector2 minimapPos = new Vector2(
                worldPos.x * scaleFactor - minimapOffset.x,
                worldPos.z * scaleFactor - minimapOffset.y
            );
            
            return minimapPos;
        }
        
        /// <summary>
        /// Convert minimap position to world position
        /// </summary>
        private Vector3 MinimapToWorldPosition(Vector2 minimapPos)
        {
            // Scaling factor based on dungeon size and minimap size
            float scaleFactor = 1.0f;
            if (dungeonManager != null && dungeonManager.CurrentDungeon != null)
            {
                int dungeonSize = Mathf.Max(dungeonManager.CurrentDungeon.Width, dungeonManager.CurrentDungeon.Height);
                scaleFactor = minimapSize.x / (dungeonSize * zoomLevel);
            }
            
            // Calculate world position
            Vector3 worldPos = new Vector3(
                (minimapPos.x + minimapOffset.x) / scaleFactor,
                0,
                (minimapPos.y + minimapOffset.y) / scaleFactor
            );
            
            return worldPos;
        }
        
        /// <summary>
        /// Check for newly revealed tiles around the player
        /// </summary>
        private void CheckForTileRevealing()
        {
            if (playerTransform == null || dungeonManager == null || dungeonManager.CurrentDungeon == null)
                return;
                
            // Get player position in tile coordinates
            Vector3 playerPos = playerTransform.position;
            Vector2Int playerTile = new Vector2Int(
                Mathf.RoundToInt(playerPos.x),
                Mathf.RoundToInt(playerPos.z)
            );
            
            // Check if player is in a room
            Room currentRoom = dungeonManager.CurrentDungeon.GetRoomAt(playerTile);
            if (currentRoom != null && !discoveredRooms.Contains(currentRoom))
            {
                // Reveal the entire room
                RevealRoom(currentRoom);
                discoveredRooms.Add(currentRoom);
                
                // Mark room as discovered in dungeon data
                dungeonManager.CurrentDungeon.DiscoverRoom(currentRoom);
                
                // Add appropriate icons based on room type
                AddRoomTypeIcons(currentRoom);
            }
            
            // Reveal tiles in visibility radius
            int visibilityRadius = 5;
            for (int x = -visibilityRadius; x <= visibilityRadius; x++)
            {
                for (int y = -visibilityRadius; y <= visibilityRadius; y++)
                {
                    Vector2Int tilePos = new Vector2Int(playerTile.x + x, playerTile.y + y);
                    
                    // Check if within radius
                    if (Vector2Int.Distance(playerTile, tilePos) <= visibilityRadius)
                    {
                        RevealTile(tilePos);
                    }
                }
            }
        }
        
        /// <summary>
        /// Reveal a specific tile on the minimap
        /// </summary>
        private void RevealTile(Vector2Int tilePos)
        {
            // Check if already revealed
            if (revealedTiles.Contains(tilePos))
                return;
                
            // Add to revealed tiles
            revealedTiles.Add(tilePos);
            
            // Get tile type
            TileType tileType = dungeonManager.CurrentDungeon.GetTileAt(tilePos.x, tilePos.y);
            
            // Set color based on tile type
            Color tileColor;
            switch (tileType)
            {
                case TileType.Wall:
                    tileColor = wallColor;
                    break;
                case TileType.Door:
                    tileColor = doorColor;
                    break;
                case TileType.Entrance:
                    tileColor = Color.green;
                    break;
                case TileType.Exit:
                    tileColor = Color.red;
                    break;
                case TileType.Water:
                    tileColor = Color.blue;
                    break;
                case TileType.Lava:
                    tileColor = Color.red;
                    break;
                case TileType.Trap:
                    tileColor = Color.yellow;
                    break;
                case TileType.Chest:
                    tileColor = Color.yellow;
                    break;
                default:
                    tileColor = visitedRoomColor;
                    break;
            }
            
            // Calculate pixel position on texture
            int pixelX = Mathf.FloorToInt(minimapSize.x / 2 + tilePos.x);
            int pixelY = Mathf.FloorToInt(minimapSize.y / 2 + tilePos.y);
            
            // Ensure within texture bounds
            if (pixelX >= 0 && pixelX < minimapTexture.width && 
                pixelY >= 0 && pixelY < minimapTexture.height)
            {
                minimapTexture.SetPixel(pixelX, pixelY, tileColor);
            }
            
            // Apply changes
            minimapTexture.Apply();
        }
        
        /// <summary>
        /// Reveal an entire room on the minimap
        /// </summary>
        private void RevealRoom(Room room)
        {
            // Get room bounds
            RectInt bounds = room.Bounds;
            
            // Reveal each tile in the room
            for (int x = bounds.x; x < bounds.x + bounds.width; x++)
            {
                for (int y = bounds.y; y < bounds.y + bounds.height; y++)
                {
                    RevealTile(new Vector2Int(x, y));
                }
            }
        }
        
        /// <summary>
        /// Add appropriate icons based on room type
        /// </summary>
        private void AddRoomTypeIcons(Room room)
        {
            Vector2Int roomCenter = room.GetCenter();
            Vector3 worldPos = new Vector3(roomCenter.x, 0, roomCenter.y);
            
            GameObject iconPrefab = null;
            switch (room.Type)
            {
                case RoomType.Entrance:
                    iconPrefab = entranceIconPrefab;
                    break;
                case RoomType.Exit:
                    iconPrefab = exitIconPrefab;
                    break;
                case RoomType.Shop:
                    iconPrefab = objectiveIconPrefab;
                    break;
                // Add more room types as needed
            }
            
            if (iconPrefab != null)
            {
                AddIconToMinimap(worldPos, iconPrefab);
            }
            
            // Check for treasure in the room
            if (room.Properties is { Type: RoomType.Treasure })
            {
                AddIconToMinimap(worldPos, treasureIconPrefab);
            }
        }
        
        /// <summary>
        /// Add an entity to be tracked on the minimap
        /// </summary>
        public void TrackEntity(GameObject entity, GameObject iconPrefab)
        {
            // Check if already tracking
            if (trackedEntities.ContainsKey(entity))
                return;
                
            // Instantiate icon
            GameObject iconObj = Instantiate(iconPrefab, minimapPanel);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            
            // Add to tracked entities
            trackedEntities.Add(entity, iconRect);
            
            // Update position immediately
            Vector2 entityMinimapPos = WorldToMinimapPosition(entity.transform.position);
            iconRect.anchoredPosition = entityMinimapPos;
        }
        
        /// <summary>
        /// Stop tracking an entity on the minimap
        /// </summary>
        public void UntrackEntity(GameObject entity)
        {
            if (trackedEntities.TryGetValue(entity, out RectTransform iconRect))
            {
                Destroy(iconRect.gameObject);
                trackedEntities.Remove(entity);
            }
        }
        
        /// <summary>
        /// Add an icon to the minimap at the specified world position
        /// </summary>
        public void AddIconToMinimap(Vector3 worldPos, GameObject iconPrefab)
        {
            if (iconPrefab == null || minimapPanel == null)
                return;
                
            // Instantiate icon
            GameObject iconObj = Instantiate(iconPrefab, minimapPanel);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            
            // Set position
            Vector2 iconMinimapPos = WorldToMinimapPosition(worldPos);
            iconRect.anchoredPosition = iconMinimapPos;
        }
        
        /// <summary>
        /// Add a custom marker to the minimap
        /// </summary>
        public void AddCustomMarker(Vector3 worldPos, MarkerType markerType, string label = "")
        {
            Vector2Int tilePos = new Vector2Int(
                Mathf.RoundToInt(worldPos.x),
                Mathf.RoundToInt(worldPos.z)
            );
            
            // Create marker data
            CustomMarker marker = new CustomMarker
            {
                Position = tilePos,
                Type = markerType,
                Label = label
            };
            
            // Add to dictionary
            customMarkers[tilePos] = marker;
            
            // Add icon to minimap
            AddIconToMinimap(worldPos, customMarkerIconPrefab);
        }
        
        /// <summary>
        /// Remove a custom marker from the minimap
        /// </summary>
        public void RemoveCustomMarker(Vector3 worldPos)
        {
            Vector2Int tilePos = new Vector2Int(
                Mathf.RoundToInt(worldPos.x),
                Mathf.RoundToInt(worldPos.z)
            );
            
            if (customMarkers.ContainsKey(tilePos))
            {
                customMarkers.Remove(tilePos);
                
                // Remove icon (would need to track icons separately to actually implement this)
                // For now, this is a placeholder
            }
        }
        
        /// <summary>
        /// Toggle between player-centered and free-look minimap modes
        /// </summary>
        public void ToggleMinimapMode()
        {
            isDragging = false;
            minimapOffset = Vector2.zero;
        }
        
        /// <summary>
        /// Toggle between fixed north and player-relative rotation
        /// </summary>
        public void ToggleRotation()
        {
            isRotating = !isRotating;
            
            // Reset rotation when switching to fixed mode
            if (!isRotating)
            {
                minimapImage.rectTransform.rotation = Quaternion.identity;
            }
        }
        
        /// <summary>
        /// Toggle between normal view and full map view
        /// </summary>
        public void ToggleFullMap()
        {
            showFullMap = !showFullMap;
            
            if (showFullMap)
            {
                // Reveal the entire dungeon
                if (dungeonManager != null && dungeonManager.CurrentDungeon != null)
                {
                    foreach (Room room in dungeonManager.CurrentDungeon.Rooms)
                    {
                        RevealRoom(room);
                    }
                    
                    // Reveal corridors (would need additional data from the dungeon)
                }
            }
            else
            {
                // Reset to only showing discovered areas
                // To fully implement this, we'd need to recreate the texture
                // and only fill in the discovered areas
                InitializeMinimapTexture();
                
                // Re-reveal all discovered rooms
                foreach (Room room in discoveredRooms)
                {
                    RevealRoom(room);
                }
                
                // Re-reveal all discovered tiles
                foreach (Vector2Int tile in revealedTiles)
                {
                    RevealTile(tile);
                }
            }
        }
        
        /// <summary>
        /// Change the zoom level of the minimap
        /// </summary>
        public void SetZoomLevel(float newZoom)
        {
            zoomLevel = Mathf.Clamp(newZoom, 0.5f, 2.0f);
        }
        
        /// <summary>
        /// Handle minimap-specific input
        /// </summary>
        private void HandleMinimapInput()
        {
            // Example input handling (to be connected to UI buttons or keyboard shortcuts)
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleFullMap();
            }
            
            // Scroll wheel for zoom
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (scrollDelta != 0 && minimapPanel.rect.Contains(Input.mousePosition))
            {
                SetZoomLevel(zoomLevel + scrollDelta * 0.5f);
            }
        }
        
        /// <summary>
        /// Center the minimap on a specific world position
        /// </summary>
        public void CenterOnPosition(Vector3 worldPos)
        {
            // Calculate offset to center the position
            Vector2 minimapPos = WorldToMinimapPosition(worldPos);
            minimapOffset = minimapPos;
        }
        
        /// <summary>
        /// Get the reveal status of a tile
        /// </summary>
        public bool IsTileRevealed(Vector2Int tilePos)
        {
            return revealedTiles.Contains(tilePos);
        }
        
        /// <summary>
        /// Clear the minimap when leaving a dungeon
        /// </summary>
        public void ClearMinimap()
        {
            // Reset all tracking data
            revealedTiles.Clear();
            discoveredRooms.Clear();
            customMarkers.Clear();
            
            // Destroy all tracked entity icons
            foreach (var entity in trackedEntities)
            {
                if (entity.Value != null)
                {
                    Destroy(entity.Value.gameObject);
                }
            }
            trackedEntities.Clear();
            
            // Reset the texture
            InitializeMinimapTexture();
        }
        
        /// <summary>
        /// Clean up resources when destroyed
        /// </summary>
        private void OnDestroy()
        {
            // Clean up texture
            if (minimapTexture != null)
            {
                Destroy(minimapTexture);
            }
        }
    }
    
    /// <summary>
    /// Types of custom markers that can be placed on the minimap
    /// </summary>
    public enum MarkerType
    {
        Objective,
        Danger,
        Point,
        Question,
        Target
    }
    
    /// <summary>
    /// Data for a custom marker on the minimap
    /// </summary>
    public class CustomMarker
    {
        public Vector2Int Position;
        public MarkerType Type;
        public string Label;
    }
}