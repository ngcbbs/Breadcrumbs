using System.Collections.Generic;
using UnityEngine;

namespace GamePortfolio.Dungeon.Generation
{
    /// <summary>
    /// Generates procedural dungeons using Binary Space Partitioning (BSP)
    /// </summary>
    public class DungeonGenerator
    {
        /// <summary>
        /// Width of the dungeon in tiles
        /// </summary>
        public int Width { get; set; } = 100;
        
        /// <summary>
        /// Height of the dungeon in tiles
        /// </summary>
        public int Height { get; set; } = 100;
        
        /// <summary>
        /// Minimum size of rooms
        /// </summary>
        public int MinRoomSize { get; set; } = 10;
        
        /// <summary>
        /// Maximum depth of BSP recursion
        /// </summary>
        public int MaxDepth { get; set; } = 5;
        
        /// <summary>
        /// Randomness factor for split position (0-1)
        /// </summary>
        public float SplitRandomness { get; set; } = 0.3f;
        
        /// <summary>
        /// Padding between room and BSP cell edge
        /// </summary>
        public int RoomPadding { get; set; } = 2;
        
        /// <summary>
        /// Width of corridors
        /// </summary>
        public int CorridorWidth { get; set; } = 2;
        
        /// <summary>
        /// Generate a dungeon using the current settings
        /// </summary>
        /// <param name="seed">Random seed (optional, 0 means use random seed)</param>
        /// <returns>Generated dungeon</returns>
        public Dungeon GenerateDungeon(int seed = 0)
        {
            // Use provided seed or generate random one
            if (seed == 0)
            {
                seed = Random.Range(1, 999999);
            }
            
            // Initialize random seed
            Random.InitState(seed);
            
            // Generate BSP tree
            BSPNode rootNode = new BSPNode(new RectInt(0, 0, Width, Height));
            SplitRecursively(rootNode, 0);
            
            // Create rooms in leaf nodes
            List<Room> rooms = CreateRooms(rootNode);
            
            // Connect rooms with corridors
            List<Corridor> corridors = ConnectRooms(rootNode);
            
            // Create and return dungeon
            return new Dungeon(rooms, corridors, Width, Height, seed);
        }
        
        /// <summary>
        /// Recursively split a BSP node
        /// </summary>
        /// <param name="node">Node to split</param>
        /// <param name="depth">Current recursion depth</param>
        private void SplitRecursively(BSPNode node, int depth)
        {
            // Stop if we've reached maximum depth
            if (depth >= MaxDepth)
                return;
                
            // Try to split the node
            if (node.Split(MinRoomSize, SplitRandomness))
            {
                // If successfully split, recursively process children
                SplitRecursively(node.Left, depth + 1);
                SplitRecursively(node.Right, depth + 1);
            }
        }
        
        /// <summary>
        /// Create rooms in the leaf nodes of the BSP tree
        /// </summary>
        /// <param name="rootNode">Root BSP node</param>
        /// <returns>List of created rooms</returns>
        private List<Room> CreateRooms(BSPNode rootNode)
        {
            // Get all leaf nodes
            List<BSPNode> leaves = new List<BSPNode>();
            rootNode.GetLeaves(leaves);
            
            List<Room> rooms = new List<Room>();
            
            // Create a room in each leaf node
            foreach (BSPNode leaf in leaves)
            {
                Room room = leaf.CreateRoom(RoomPadding);
                if (room != null)
                {
                    rooms.Add(room);
                }
            }
            
            return rooms;
        }
        
        /// <summary>
        /// Connect rooms with corridors
        /// </summary>
        /// <param name="rootNode">Root BSP node</param>
        /// <returns>List of created corridors</returns>
        private List<Corridor> ConnectRooms(BSPNode rootNode)
        {
            List<Corridor> corridors = new List<Corridor>();
            
            // Recursively connect rooms in the BSP tree
            CreateCorridorsRecursive(rootNode, corridors);
            
            return corridors;
        }
        
        /// <summary>
        /// Recursively connect rooms in the BSP tree
        /// </summary>
        /// <param name="node">Current BSP node</param>
        /// <param name="corridors">List to add corridors to</param>
        private void CreateCorridorsRecursive(BSPNode node, List<Corridor> corridors)
        {
            // Only internal nodes need corridors between their children
            if (node.Left != null && node.Right != null)
            {
                // Get rooms in left and right subtrees
                List<Room> leftRooms = new List<Room>();
                List<Room> rightRooms = new List<Room>();
                
                node.Left.GetRooms(leftRooms);
                node.Right.GetRooms(rightRooms);
                
                // If both subtrees have rooms, connect them
                if (leftRooms.Count > 0 && rightRooms.Count > 0)
                {
                    // Find closest pair of rooms to connect
                    Room leftRoom = FindBestRoomToConnect(leftRooms, rightRooms, out Room rightRoom);
                    
                    // Create corridor between them
                    Corridor corridor = CreateCorridor(leftRoom, rightRoom);
                    corridors.Add(corridor);
                }
                
                // Recursively process children
                CreateCorridorsRecursive(node.Left, corridors);
                CreateCorridorsRecursive(node.Right, corridors);
            }
        }
        
        /// <summary>
        /// Find the best pair of rooms to connect between two lists
        /// </summary>
        /// <param name="leftRooms">Rooms from left subtree</param>
        /// <param name="rightRooms">Rooms from right subtree</param>
        /// <param name="rightRoom">Output parameter for closest right room</param>
        /// <returns>Closest left room</returns>
        private Room FindBestRoomToConnect(List<Room> leftRooms, List<Room> rightRooms, out Room rightRoom)
        {
            float minDistance = float.MaxValue;
            Room closestLeft = leftRooms[0];
            rightRoom = rightRooms[0];
            
            // Find pair with minimum distance between centers
            foreach (Room left in leftRooms)
            {
                Vector2Int leftCenter = left.GetCenter();
                
                foreach (Room right in rightRooms)
                {
                    Vector2Int rightCenter = right.GetCenter();
                    float distance = Vector2Int.Distance(leftCenter, rightCenter);
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestLeft = left;
                        rightRoom = right;
                    }
                }
            }
            
            return closestLeft;
        }
        
        /// <summary>
        /// Create a corridor connecting two rooms
        /// </summary>
        /// <param name="roomA">First room</param>
        /// <param name="roomB">Second room</param>
        /// <returns>Created corridor</returns>
        private Corridor CreateCorridor(Room roomA, Room roomB)
        {
            // Get centers of rooms
            Vector2Int centerA = roomA.GetCenter();
            Vector2Int centerB = roomB.GetCenter();
            
            // 80% chance to use L-shaped corridor, 20% to use straight line
            bool useLShape = Random.value < 0.8f;
            
            return new Corridor(centerA, centerB, CorridorWidth, useLShape);
        }
    }
}
