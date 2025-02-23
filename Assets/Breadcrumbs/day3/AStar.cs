using System;
using System.Collections.Generic;
using UnityEngine;

namespace day3_scrap {

    public enum TileType {
        Empty,
        Room,
        Way,
    }
    
    public class Node : IComparable<Node> {
        public Vector2Int Index;
        public bool IsWalkable;
        public TileType TileType;
        public Node Parent;
        public float G, H; // G: 시작점부터 거리, H: 휴리스틱(목표점 거리)
        public float F => G + H; // F = G + H

        public Node(Vector2Int index, bool walkable, TileType type = TileType.Empty) {
            Index = index;
            IsWalkable = walkable;
            TileType = type;
        }

        public int CompareTo(Node other) {
            if (other == null)
                throw new ArgumentException("Object is null");
            return F.CompareTo(other.F);
        }
    }
    
    public class Grid {
        private readonly Dictionary<Vector2Int, Node> _nodes = new(64 * 64);

        public void Clear() {
            _nodes.Clear();
        }
        
        public Dictionary<Vector2Int, Node> Nodes => _nodes;

        public Node GetNode(Vector2Int index) {
            return _nodes.GetValueOrDefault(index);
        }

        public void SetWalkable(Vector2Int index, bool isWalkable = true, TileType type = TileType.Empty) {
            if (_nodes.TryGetValue(index, out var node)) {
                node.IsWalkable = isWalkable;
                node.TileType = type;
                return;
            }

            _nodes.Add(index, new Node(index, isWalkable, type));
        }
        
        public void SetWay(Vector2Int index) {
            if (_nodes.TryGetValue(index, out var node)) {
                node.IsWalkable = false;
                node.TileType = node.TileType == TileType.Empty ? TileType.Way : node.TileType;
                return;
            }
            _nodes.Add(index, new Node(index, false, TileType.Way));
        }
    }
    
    public class AStar {
        private readonly Grid _grid;

        private static readonly Vector2Int[] Directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        };

        public AStar(Grid grid) {
            _grid = grid;
        }

        public List<Node> FindPath(Vector2Int start, Vector2Int goal) {
            Node startNode = _grid.GetNode(start);
            Node goalNode = _grid.GetNode(goal);

            if (startNode == null || goalNode == null || !goalNode.IsWalkable)
                return null;

            List<Node> openSet = new List<Node> { startNode };
            HashSet<Node> closedSet = new HashSet<Node>();

            foreach (var pair in _grid.Nodes)
                pair.Value.Parent = null;

            while (openSet.Count > 0) {
                Node current = openSet[0];
                foreach (var node in openSet)
                    if (node.F < current.F)
                        current = node;

                openSet.Remove(current);
                closedSet.Add(current);

                if (current.Index == goalNode.Index)
                    return ReconstructPath(goalNode);

                foreach (var neighbor in GetNeighbors(current)) {
                    if (closedSet.Contains(neighbor))
                        continue;

                    float gCost = current.G + Vector2Int.Distance(current.Index, neighbor.Index);
                    switch (current.TileType) {
                        case TileType.Room:
                            gCost *= 1.8f;
                            break;
                        case TileType.Way:
                            gCost *= 1.2f;
                            break;
                    }
                    if (!openSet.Contains(neighbor) || gCost < neighbor.G) {
                        neighbor.G = gCost;
                        neighbor.H = Vector2Int.Distance(neighbor.Index, goalNode.Index);
                        neighbor.Parent = current;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private List<Node> GetNeighbors(Node node) {
            List<Node> neighbors = new List<Node>();

            foreach (var dir in Directions) {
                Node neighbor = _grid.GetNode(node.Index + dir);
                if (neighbor != null && neighbor.IsWalkable)
                    neighbors.Add(neighbor);
            }

            return neighbors;
        }

        private List<Node> ReconstructPath(Node goalNode) {
            List<Node> path = new List<Node>();
            Node current = goalNode;
            while (current != null) {
                path.Add(current);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}