using System;
using UnityEngine;

namespace GamePortfolio.Dungeon.Generation
{
    /// <summary>
    /// Represents a room in the dungeon
    /// </summary>
    [Serializable]
    public class Room
    {
        /// <summary>
        /// The rectangular area of the room
        /// </summary>
        public RectInt Bounds { get; private set; }
        
        /// <summary>
        /// The type of room (normal, entrance, exit, etc.)
        /// </summary>
        public RoomType Type { get; set; } = RoomType.Normal;
        
        /// <summary>
        /// Whether the room has been discovered by the player
        /// </summary>
        public bool IsDiscovered { get; set; } = false;
        
        /// <summary>
        /// Additional data/properties for the room
        /// </summary>
        public RoomData Properties { get; set; }
        
        /// <summary>
        /// Create a new room with the given bounds
        /// </summary>
        /// <param name="bounds">Rectangular area of the room</param>
        public Room(RectInt bounds)
        {
            Bounds = bounds;
            Properties = new RoomData();
        }
        
        /// <summary>
        /// Get the center point of the room
        /// </summary>
        /// <returns>Center position as Vector2Int</returns>
        public Vector2Int GetCenter()
        {
            return new Vector2Int(
                Bounds.x + Bounds.width / 2,
                Bounds.y + Bounds.height / 2
            );
        }
        
        /// <summary>
        /// Check if a point is inside the room
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns>True if the point is inside the room</returns>
        public bool Contains(Vector2Int point)
        {
            return Bounds.Contains(point);
        }
        
        /// <summary>
        /// Get a random position inside the room
        /// </summary>
        /// <param name="padding">Padding from the room edges</param>
        /// <returns>Random position inside the room</returns>
        public Vector2Int GetRandomPosition(int padding = 1)
        {
            int x = UnityEngine.Random.Range(
                Bounds.x + padding, 
                Bounds.x + Bounds.width - padding
            );
            
            int y = UnityEngine.Random.Range(
                Bounds.y + padding, 
                Bounds.y + Bounds.height - padding
            );
            
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Calculate the distance to another room (center to center)
        /// </summary>
        /// <param name="other">The other room</param>
        /// <returns>Distance between room centers</returns>
        public float DistanceTo(Room other)
        {
            return Vector2Int.Distance(GetCenter(), other.GetCenter());
        }
        
        /// <summary>
        /// Convert to data format for serialization
        /// </summary>
        /// <returns>Serializable room data</returns>
        public RoomData ToData()
        {
            RoomData data = Properties ?? new RoomData();
            data.X = Bounds.x;
            data.Y = Bounds.y;
            data.Width = Bounds.width;
            data.Height = Bounds.height;
            data.Type = Type;
            data.IsDiscovered = IsDiscovered;
            
            return data;
        }
    }
    
    /// <summary>
    /// Room types in the dungeon
    /// </summary>
    public enum RoomType
    {
        Normal,
        Entrance,
        Exit,
        Treasure,
        Shop,
        Boss,
        Secret,
        Challenge
    }
    
    /// <summary>
    /// Serializable room data for network transmission and saving
    /// </summary>
    [Serializable]
    public class RoomData
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public RoomType Type;
        public bool IsDiscovered;
        public string CustomProperties;
        public int EnemyCount;
        public bool IsCleared;
        public int TreasureLevel;
        
        /// <summary>
        /// Get rectangle bounds from this data
        /// </summary>
        public RectInt GetBounds()
        {
            return new RectInt(X, Y, Width, Height);
        }
    }
}
