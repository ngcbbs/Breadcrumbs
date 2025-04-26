using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePortfolio.Dungeon.Generation
{
    /// <summary>
    /// Represents a complete dungeon with rooms, corridors, and other elements
    /// </summary>
    public class Dungeon
    {
        /// <summary>
        /// Rooms in the dungeon
        /// </summary>
        public List<Room> Rooms { get; private set; }
        
        /// <summary>
        /// Corridors connecting rooms
        /// </summary>
        public List<Corridor> Corridors { get; private set; }
        
        /// <summary>
        /// Width of the dungeon in tiles
        /// </summary>
        public int Width { get; private set; }
        
        /// <summary>
        /// Height of the dungeon in tiles
        /// </summary>
        public int Height { get; private set; }
        
        /// <summary>
        /// Seed used to generate the dungeon
        /// </summary>
        public int Seed { get; private set; }
        
        /// <summary>
        /// Grid of tile types in the dungeon
        /// </summary>
        private TileType[,] tileGrid;
        
        /// <summary>
        /// Create a new dungeon with the given rooms and corridors
        /// </summary>
        /// <param name="rooms">List of rooms</param>
        /// <param name="corridors">List of corridors</param>
        /// <param name="width">Width of the dungeon</param>
        /// <param name="height">Height of the dungeon</param>
        /// <param name="seed">Seed used for generation</param>
        public Dungeon(List<Room> rooms, List<Corridor> corridors, int width, int height, int seed)
        {
            Rooms = rooms;
            Corridors = corridors;
            Width = width;
            Height = height;
            Seed = seed;
            
            // Create tile grid
            tileGrid = CreateTileGrid();
        }
        
        /// <summary>
        /// Create a dungeon from serialized data
        /// </summary>
        /// <param name="data">Dungeon data</param>
        public Dungeon(DungeonData data)
        {
            Width = data.Width;
            Height = data.Height;
            Seed = data.Seed;
            
            // Create rooms from data
            Rooms = new List<Room>();
            if (data.Rooms != null)
            {
                foreach (RoomData roomData in data.Rooms)
                {
                    Room room = new Room(new RectInt(roomData.X, roomData.Y, roomData.Width, roomData.Height))
                    {
                        Type = roomData.Type,
                        IsDiscovered = roomData.IsDiscovered,
                        Properties = roomData
                    };
                    Rooms.Add(room);
                }
            }
            
            // Create corridors from data
            Corridors = new List<Corridor>();
            if (data.Corridors != null)
            {
                foreach (CorridorData corridorData in data.Corridors)
                {
                    List<Vector2Int> path = new List<Vector2Int>();
                    foreach (Vector2IntData point in corridorData.Path)
                    {
                        path.Add(new Vector2Int(point.X, point.Y));
                    }
                    
                    Corridor corridor = new Corridor(path, corridorData.Width)
                    {
                        HasDoors = corridorData.HasDoors
                    };
                    Corridors.Add(corridor);
                }
            }
            
            // Create tile grid
            tileGrid = CreateTileGrid();
        }
        
        /// <summary>
        /// Create a grid of tile types based on rooms and corridors
        /// </summary>
        /// <returns>2D array of tile types</returns>
        public TileType[,] CreateTileGrid()
        {
            TileType[,] grid = new TileType[Width, Height];
            
            // Initialize all tiles as walls
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    grid[x, y] = TileType.Wall;
                }
            }
            
            // Set room tiles
            foreach (Room room in Rooms)
            {
                for (int x = room.Bounds.x; x < room.Bounds.x + room.Bounds.width; x++)
                {
                    for (int y = room.Bounds.y; y < room.Bounds.y + room.Bounds.height; y++)
                    {
                        // Boundary is wall, interior is floor
                        if (x == room.Bounds.x || x == room.Bounds.x + room.Bounds.width - 1 ||
                            y == room.Bounds.y || y == room.Bounds.y + room.Bounds.height - 1)
                        {
                            grid[x, y] = TileType.Wall;
                        }
                        else
                        {
                            grid[x, y] = TileType.Floor;
                        }
                    }
                }
                
                // Set special tiles based on room type
                Vector2Int center = room.GetCenter();
                switch (room.Type)
                {
                    case RoomType.Entrance:
                        grid[center.x, center.y] = TileType.Entrance;
                        break;
                    case RoomType.Exit:
                        grid[center.x, center.y] = TileType.Exit;
                        break;
                    case RoomType.Shop:
                        grid[center.x, center.y] = TileType.Shop;
                        break;
                    // Handle other special room types...
                }
            }
            
            // Set corridor tiles
            foreach (Corridor corridor in Corridors)
            {
                foreach (Vector2Int point in corridor.Path)
                {
                    int halfWidth = corridor.Width / 2;
                    
                    for (int x = point.x - halfWidth; x <= point.x + halfWidth; x++)
                    {
                        for (int y = point.y - halfWidth; y <= point.y + halfWidth; y++)
                        {
                            if (x >= 0 && x < Width && y >= 0 && y < Height)
                            {
                                grid[x, y] = TileType.Floor;
                            }
                        }
                    }
                }
            }
            
            // Add doors between rooms and corridors
            AddDoors(grid);
            
            return grid;
        }
        
        /// <summary>
        /// Add doors at the intersections of rooms and corridors
        /// </summary>
        /// <param name="grid">Tile grid to modify</param>
        private void AddDoors(TileType[,] grid)
        {
            // For each room
            foreach (Room room in Rooms)
            {
                // Check room perimeter for potential door locations
                for (int x = room.Bounds.x + 1; x < room.Bounds.x + room.Bounds.width - 1; x++)
                {
                    // Check top wall
                    CheckForDoor(grid, x, room.Bounds.y, 0, -1);
                    
                    // Check bottom wall
                    CheckForDoor(grid, x, room.Bounds.y + room.Bounds.height - 1, 0, 1);
                }
                
                for (int y = room.Bounds.y + 1; y < room.Bounds.y + room.Bounds.height - 1; y++)
                {
                    // Check left wall
                    CheckForDoor(grid, room.Bounds.x, y, -1, 0);
                    
                    // Check right wall
                    CheckForDoor(grid, room.Bounds.x + room.Bounds.width - 1, y, 1, 0);
                }
            }
        }
        
        /// <summary>
        /// Check if a door should be placed at a specific wall position
        /// </summary>
        /// <param name="grid">Tile grid</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="dx">X direction to check</param>
        /// <param name="dy">Y direction to check</param>
        private void CheckForDoor(TileType[,] grid, int x, int y, int dx, int dy)
        {
            // Skip if out of bounds
            if (x < 0 || x >= Width || y < 0 || y >= Height ||
                x + dx < 0 || x + dx >= Width || y + dy < 0 || y + dy >= Height)
            {
                return;
            }
            
            // If there's a corridor outside the wall, place a door
            if (grid[x, y] == TileType.Wall && grid[x + dx, y + dy] == TileType.Floor)
            {
                bool isDoorway = false;
                
                // Check if this is actually a doorway between a room and corridor
                for (int checkX = -1; checkX <= 1; checkX++)
                {
                    for (int checkY = -1; checkY <= 1; checkY++)
                    {
                        int nextX = x + dx + checkX;
                        int nextY = y + dy + checkY;
                        
                        if (nextX >= 0 && nextX < Width && nextY >= 0 && nextY < Height &&
                            grid[nextX, nextY] == TileType.Floor)
                        {
                            isDoorway = true;
                            break;
                        }
                    }
                    
                    if (isDoorway) break;
                }
                
                if (isDoorway)
                {
                    // 40% chance to actually place a door
                    if (UnityEngine.Random.value < 0.4f)
                    {
                        grid[x, y] = TileType.Door;
                    }
                    else
                    {
                        // Otherwise, make it an open doorway
                        grid[x, y] = TileType.Floor;
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the tile type at a specific position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Tile type or Wall if out of bounds</returns>
        public TileType GetTileAt(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return TileType.Wall;
            }
            
            return tileGrid[x, y];
        }
        
        /// <summary>
        /// Find the room that contains a given point
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Room containing the position, or null if none</returns>
        public Room GetRoomAt(Vector2Int position)
        {
            foreach (Room room in Rooms)
            {
                if (room.Contains(position))
                {
                    return room;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Find a room by type
        /// </summary>
        /// <param name="type">Room type to find</param>
        /// <returns>First room of the specified type, or null if none</returns>
        public Room GetRoomByType(RoomType type)
        {
            foreach (Room room in Rooms)
            {
                if (room.Type == type)
                {
                    return room;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Find all rooms of a specific type
        /// </summary>
        /// <param name="type">Room type to find</param>
        /// <returns>List of rooms matching the type</returns>
        public List<Room> GetRoomsByType(RoomType type)
        {
            List<Room> result = new List<Room>();
            
            foreach (Room room in Rooms)
            {
                if (room.Type == type)
                {
                    result.Add(room);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get a random room, optionally of a specific type
        /// </summary>
        /// <param name="type">Room type to filter by, or null for any type</param>
        /// <returns>Random room matching criteria, or null if none</returns>
        public Room GetRandomRoom(RoomType? type = null)
        {
            if (Rooms.Count == 0) return null;
            
            if (type.HasValue)
            {
                List<Room> matchingRooms = GetRoomsByType(type.Value);
                if (matchingRooms.Count == 0) return null;
                
                return matchingRooms[UnityEngine.Random.Range(0, matchingRooms.Count)];
            }
            else
            {
                return Rooms[UnityEngine.Random.Range(0, Rooms.Count)];
            }
        }
        
        /// <summary>
        /// Mark a room as discovered
        /// </summary>
        /// <param name="room">Room to mark as discovered</param>
        public void DiscoverRoom(Room room)
        {
            room.IsDiscovered = true;
        }
        
        /// <summary>
        /// Convert to serializable data format
        /// </summary>
        /// <returns>Dungeon data for serialization</returns>
        public DungeonData ToData()
        {
            DungeonData data = new DungeonData
            {
                Width = Width,
                Height = Height,
                Seed = Seed,
                Rooms = new List<RoomData>(),
                Corridors = new List<CorridorData>()
            };
            
            // Convert rooms to data
            foreach (Room room in Rooms)
            {
                data.Rooms.Add(room.ToData());
            }
            
            // Convert corridors to data
            foreach (Corridor corridor in Corridors)
            {
                data.Corridors.Add(corridor.ToData());
            }
            
            return data;
        }
    }
    
    /// <summary>
    /// Tile types in the dungeon
    /// </summary>
    public enum TileType
    {
        Wall,
        Floor,
        Door,
        Entrance,
        Exit,
        Water,
        Lava,
        Trap,
        Shop,
        Chest
    }
    
    /// <summary>
    /// Serializable dungeon data for network transmission and saving
    /// </summary>
    [Serializable]
    public class DungeonData
    {
        public int Width;
        public int Height;
        public int Seed;
        public List<RoomData> Rooms;
        public List<CorridorData> Corridors;
    }
}
