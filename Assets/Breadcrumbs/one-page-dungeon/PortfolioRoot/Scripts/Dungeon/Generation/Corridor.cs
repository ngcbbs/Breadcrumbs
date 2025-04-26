using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace GamePortfolio.Dungeon.Generation
{
    /// <summary>
    /// Represents a corridor connecting rooms in the dungeon
    /// </summary>
    [Serializable]
    public class Corridor
    {
        /// <summary>
        /// Path points defining the corridor
        /// </summary>
        public List<Vector2Int> Path { get; private set; }
        
        /// <summary>
        /// Width of the corridor
        /// </summary>
        public int Width { get; private set; }
        
        /// <summary>
        /// Whether the corridor has doors
        /// </summary>
        public bool HasDoors { get; set; }
        
        /// <summary>
        /// Create a new corridor with the given path and width
        /// </summary>
        /// <param name="path">Path points</param>
        /// <param name="width">Width of the corridor</param>
        public Corridor(List<Vector2Int> path, int width = 1)
        {
            Path = path;
            Width = Mathf.Max(1, width);
        }
        
        /// <summary>
        /// Create a new corridor connecting two points
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="end">End point</param>
        /// <param name="width">Width of the corridor</param>
        /// <param name="useLShape">Whether to use L-shaped path (vs direct line)</param>
        public Corridor(Vector2Int start, Vector2Int end, int width = 1, bool useLShape = true)
        {
            Width = Mathf.Max(1, width);
            Path = new List<Vector2Int>();
            
            if (useLShape)
            {
                // Create L-shaped path
                CreateLShapedPath(start, end);
            }
            else
            {
                // Create straight line path
                CreateDirectPath(start, end);
            }
        }
        
        /// <summary>
        /// Create an L-shaped path between two points
        /// </summary>
        private void CreateLShapedPath(Vector2Int start, Vector2Int end)
        {
            Path.Clear();
            
            // Randomly determine whether to go horizontal first or vertical first
            bool horizontalFirst = UnityEngine.Random.value > 0.5f;
            
            if (horizontalFirst)
            {
                // Go horizontal first, then vertical
                for (int x = start.x; x != end.x; x += (start.x < end.x ? 1 : -1))
                {
                    Path.Add(new Vector2Int(x, start.y));
                }
                
                for (int y = start.y; y != end.y; y += (start.y < end.y ? 1 : -1))
                {
                    Path.Add(new Vector2Int(end.x, y));
                }
            }
            else
            {
                // Go vertical first, then horizontal
                for (int y = start.y; y != end.y; y += (start.y < end.y ? 1 : -1))
                {
                    Path.Add(new Vector2Int(start.x, y));
                }
                
                for (int x = start.x; x != end.x; x += (start.x < end.x ? 1 : -1))
                {
                    Path.Add(new Vector2Int(x, end.y));
                }
            }
            
            // Add the final endpoint
            Path.Add(end);
        }
        
        /// <summary>
        /// Create a direct path between two points (using Bresenham's line algorithm)
        /// </summary>
        private void CreateDirectPath(Vector2Int start, Vector2Int end)
        {
            Path.Clear();
            
            int x = start.x;
            int y = start.y;
            int dx = Mathf.Abs(end.x - start.x);
            int dy = Mathf.Abs(end.y - start.y);
            int sx = start.x < end.x ? 1 : -1;
            int sy = start.y < end.y ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                Path.Add(new Vector2Int(x, y));
                
                if (x == end.x && y == end.y) break;
                
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }
        }
        
        /// <summary>
        /// Get all tile positions covered by this corridor, considering its width
        /// </summary>
        public List<Vector2Int> GetAllTiles()
        {
            List<Vector2Int> tiles = new List<Vector2Int>();
            
            // Expand path to consider corridor width
            foreach (Vector2Int pathPoint in Path)
            {
                int halfWidth = Width / 2;
                
                for (int x = pathPoint.x - halfWidth; x <= pathPoint.x + halfWidth; x++)
                {
                    for (int y = pathPoint.y - halfWidth; y <= pathPoint.y + halfWidth; y++)
                    {
                        Vector2Int tile = new Vector2Int(x, y);
                        if (!tiles.Contains(tile))
                        {
                            tiles.Add(tile);
                        }
                    }
                }
            }
            
            return tiles;
        }
        
        /// <summary>
        /// Check if the corridor contains a given point
        /// </summary>
        public bool Contains(Vector2Int point)
        {
            int halfWidth = Width / 2;
            
            foreach (Vector2Int pathPoint in Path)
            {
                if (Mathf.Abs(point.x - pathPoint.x) <= halfWidth && 
                    Mathf.Abs(point.y - pathPoint.y) <= halfWidth)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Convert to corridor data for serialization
        /// </summary>
        /// <returns>Serializable corridor data</returns>
        public CorridorData ToData()
        {
            CorridorData data = new CorridorData
            {
                Path = new List<Vector2IntData>(),
                Width = Width,
                HasDoors = HasDoors
            };
            
            foreach (Vector2Int point in Path)
            {
                data.Path.Add(new Vector2IntData { X = point.x, Y = point.y });
            }
            
            return data;
        }
    }
    
    /// <summary>
    /// Serializable corridor data for network transmission and saving
    /// </summary>
    [MessagePackObject]
    [Serializable]
    public class CorridorData
    {
        [Key(0)]
        public List<Vector2IntData> Path;
        [Key(1)]
        public int Width;
        [Key(2)]
        public bool HasDoors;
    }
    
    /// <summary>
    /// Serializable Vector2Int for network transmission and saving
    /// </summary>
    [MessagePackObject]
    [Serializable]
    public class Vector2IntData
    {
        [Key(0)]
        public int X;
        [Key(1)]
        public int Y;
        
        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(X, Y);
        }
    }
}
