using System.Collections.Generic;
using UnityEngine;

namespace day3_scap {
    public class MSTPrims {
        public class Edge {
            public Vector2Int From { get; set; }
            public Vector2Int To { get; set; }
            public float Weight { get; set; }

            public Edge(Vector2Int from, Vector2Int to, float weight = 0f) {
                From = from;
                To = to;
                Weight = weight;
            }
        }

        // 두 노드 간의 거리를 계산하는 함수
        private float CalculateDistance(Vector2Int a, Vector2Int b) {
            return Vector2Int.Distance(a, b);
        }

        public List<Edge> FindMST(List<Vector2Int> nodes) {
            if (nodes == null || nodes.Count < 2)
                return new List<Edge>();

            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            List<Edge> mstEdges = new List<Edge>();

            // 우선순위 큐 대신 List를 사용하여 구현
            List<Edge> priorityQueue = new List<Edge>();

            // 시작 노드 선택 (첫 번째 노드)
            Vector2Int startNode = nodes[0];
            visited.Add(startNode);

            // 시작 노드에서 모든 다른 노드로의 엣지 추가
            foreach (var node in nodes) {
                if (node != startNode) {
                    priorityQueue.Add(new Edge(startNode, node, CalculateDistance(startNode, node)));
                }
            }

            // Prim's 알고리즘 실행
            while (priorityQueue.Count > 0 && visited.Count < nodes.Count) {
                // 가장 가중치가 낮은 엣지 찾기
                Edge minEdge = null;
                float minWeight = float.MaxValue;
                int minIndex = -1;

                for (int i = 0; i < priorityQueue.Count; i++) {
                    if (priorityQueue[i].Weight < minWeight) {
                        minEdge = priorityQueue[i];
                        minWeight = priorityQueue[i].Weight;
                        minIndex = i;
                    }
                }

                if (minEdge == null) break;
                priorityQueue.RemoveAt(minIndex);

                // 이미 방문한 노드라면 스킵
                if (visited.Contains(minEdge.To))
                    continue;

                // 새로운 노드 방문 처리
                visited.Add(minEdge.To);
                mstEdges.Add(new Edge(minEdge.From, minEdge.To));

                // 새로 방문한 노드에서 갈 수 있는 모든 엣지 추가
                foreach (var node in nodes) {
                    if (!visited.Contains(node)) {
                        priorityQueue.Add(new Edge(minEdge.To, node, CalculateDistance(minEdge.To, node)));
                    }
                }
            }

            return mstEdges;
        }
    }
}