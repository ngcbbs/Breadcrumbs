using System.Collections.Generic;
using UnityEngine;

namespace GamePortfolio.Dungeon.Generation
{
    /// <summary>
    /// Represents a node in a Binary Space Partitioning tree for dungeon generation
    /// </summary>
    public class BSPNode
    {
        /// <summary>
        /// The rectangular space this node represents
        /// </summary>
        public RectInt Space { get; private set; }
        
        /// <summary>
        /// Left (or top) child node after splitting
        /// </summary>
        public BSPNode Left { get; private set; }
        
        /// <summary>
        /// Right (or bottom) child node after splitting
        /// </summary>
        public BSPNode Right { get; private set; }
        
        /// <summary>
        /// Room contained in this node (if leaf node)
        /// </summary>
        public Room Room { get; private set; }
        
        /// <summary>
        /// Whether this node is a leaf node (has no children)
        /// </summary>
        public bool IsLeaf => Left == null && Right == null;
        
        /// <summary>
        /// Create a new BSP node with the given space
        /// </summary>
        /// <param name="space">Rectangular space for this node</param>
        public BSPNode(RectInt space)
        {
            Space = space;
        }
        
        /// <summary>
        /// Split this node into two child nodes
        /// </summary>
        /// <param name="minSize">Minimum size of a node</param>
        /// <param name="splitRandomness">Randomness factor for split position (0-1)</param>
        /// <returns>True if successfully split, false otherwise</returns>
        public bool Split(int minSize, float splitRandomness)
        {
            // Already split
            if (!IsLeaf) return false;
            
            // Too small to split further
            if (Space.width < minSize * 2 || Space.height < minSize * 2)
                return false;
                
            // Determine split direction (horizontal or vertical)
            bool horizontalSplit;
            
            if (Space.width > Space.height * 1.25f)
                horizontalSplit = false; // Vertical split for wide spaces
            else if (Space.height > Space.width * 1.25f)
                horizontalSplit = true; // Horizontal split for tall spaces
            else
                horizontalSplit = Random.value > 0.5f; // Random for nearly square spaces
                
            // Calculate split position with randomness
            float splitRatio = Mathf.Clamp(0.5f + (Random.value - 0.5f) * splitRandomness, 0.4f, 0.6f);
            
            if (horizontalSplit)
            {
                // Horizontal split (along height)
                int splitPoint = Mathf.FloorToInt(Space.height * splitRatio);
                
                // Ensure minimum size is maintained
                if (splitPoint < minSize || Space.height - splitPoint < minSize)
                    return false;
                    
                // Create child nodes
                Left = new BSPNode(new RectInt(Space.x, Space.y, Space.width, splitPoint));
                Right = new BSPNode(new RectInt(Space.x, Space.y + splitPoint, Space.width, Space.height - splitPoint));
            }
            else
            {
                // Vertical split (along width)
                int splitPoint = Mathf.FloorToInt(Space.width * splitRatio);
                
                // Ensure minimum size is maintained
                if (splitPoint < minSize || Space.width - splitPoint < minSize)
                    return false;
                    
                // Create child nodes
                Left = new BSPNode(new RectInt(Space.x, Space.y, splitPoint, Space.height));
                Right = new BSPNode(new RectInt(Space.x + splitPoint, Space.y, Space.width - splitPoint, Space.height));
            }
            
            return true;
        }
        
        /// <summary>
        /// Create a room within this node
        /// </summary>
        /// <param name="padding">Padding between room and node edges</param>
        /// <returns>The created room, or null if creation failed</returns>
        public Room CreateRoom(int padding)
        {
            // Skip if not a leaf node or room already exists
            if (!IsLeaf || Room != null)
                return Room;
                
            // Calculate padding (not exceeding 1/4 of space dimensions)
            int paddingX = Mathf.Min(padding, Space.width / 4);
            int paddingY = Mathf.Min(padding, Space.height / 4);
            
            // Calculate room position and size with padding
            int x = Space.x + Random.Range(paddingX, paddingX * 2);
            int y = Space.y + Random.Range(paddingY, paddingY * 2);
            int width = Space.width - (x - Space.x) - Random.Range(paddingX, paddingX * 2);
            int height = Space.height - (y - Space.y) - Random.Range(paddingY, paddingY * 2);
            
            // Ensure minimum room size
            if (width < 3 || height < 3)
                return null;
                
            // Create the room
            Room = new Room(new RectInt(x, y, width, height));
            return Room;
        }
        
        /// <summary>
        /// Get all rooms in this node and its children
        /// </summary>
        /// <param name="rooms">List to collect rooms</param>
        public void GetRooms(List<Room> rooms)
        {
            if (Room != null)
                rooms.Add(Room);
                
            // Recursively get rooms from children
            if (Left != null)
                Left.GetRooms(rooms);
                
            if (Right != null)
                Right.GetRooms(rooms);
        }
        
        /// <summary>
        /// Get all leaf nodes in this node and its children
        /// </summary>
        /// <param name="leaves">List to collect leaf nodes</param>
        public void GetLeaves(List<BSPNode> leaves)
        {
            if (IsLeaf)
                leaves.Add(this);
            else
            {
                if (Left != null)
                    Left.GetLeaves(leaves);
                    
                if (Right != null)
                    Right.GetLeaves(leaves);
            }
        }
    }
}
