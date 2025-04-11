using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Breadcrumbs.one_page_dungeon {
    public static class EdgeBuilder {
        private static readonly Vector2Int[] Directions = {
            new Vector2Int(0, 1), // 북
            new Vector2Int(1, 0), // 동
            new Vector2Int(0, -1), // 남
            new Vector2Int(-1, 0) // 서
        };

        private static readonly Wall[] WallTypes = { Wall.North, Wall.East, Wall.South, Wall.West };

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private static void SetMeshColor(GameObject go, Color color) {
            if (go == null)
                return;
            var meshRenderer = go.GetComponent<MeshRenderer>();
            var material = meshRenderer.material;
            material.SetColor(ColorId, color);
            meshRenderer.sharedMaterial = material;
        }

        public static void Build(List<Vector2Int> edgeTiles, DungeonTemplate dungeonTemplate, Transform root) {
            // make wall edge info
            var edges = new Dictionary<Vector2Int, Edge>();
            foreach (var tile in edgeTiles) {
                var wallType = Wall.None;
                for (var i = 0; i < 4; i++) {
                    if (!edgeTiles.Contains(tile + Directions[i]))
                        wallType |= WallTypes[i];
                }

                if (!edges.ContainsKey(tile))
                    edges.Add(tile, new Edge() { pos = tile, wall = wallType });
                else {
                    Debug.Log("<color=red>tile 중복</color>");
                }
            }

            // make edges.
            foreach (var edge in edges.Values) {
                var wall = edge.wall;
                var pos = new Vector3(edge.pos.x, 0, edge.pos.y) * 4f;
                GameObject go;
                if ((wall & Wall.North) != 0) {
                    go = Object.Instantiate(dungeonTemplate.wallTop, root);
                    go.transform.position = pos + new Vector3(2, 0, 4f);
                    SetMeshColor(go, Color.red);
                }

                if ((wall & Wall.East) != 0) {
                    go = Object.Instantiate(dungeonTemplate.wallRight, root);
                    go.transform.position = pos + new Vector3(4f, 0, 2f);
                    SetMeshColor(go, Color.green);
                }

                if ((wall & Wall.South) != 0) {
                    go = Object.Instantiate(dungeonTemplate.wallBottom, root);
                    go.transform.position = pos + new Vector3(2f, 0, 0);
                    SetMeshColor(go, Color.blue);
                }

                if ((wall & Wall.West) != 0) {
                    go = Object.Instantiate(dungeonTemplate.wallLeft, root);
                    go.transform.position = pos + new Vector3(0, 0, 2f);
                }
            }
        }
    }
}
