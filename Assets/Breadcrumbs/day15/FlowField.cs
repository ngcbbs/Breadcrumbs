using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.day15 {
    public class FlowField {
        private Vector2[,] flowVectors;
        private float[,] costField; // 코스트 필드 저장
        private int width;
        private int height;
        private float cellSize;
        private Vector2 origin;
        private bool[,] obstaclesMap;

        // 목표 위치 저장
        private Vector2Int targetCell;

        // Flow field 생성을 위한 기본 생성자
        public FlowField(int width, int height, float cellSize, Vector2 origin) {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.origin = origin;
            flowVectors = new Vector2[width, height];
            obstaclesMap = new bool[width, height];
            costField = new float[width, height];
            targetCell = new Vector2Int(0, 0);
        }

        // Grid의 특정 위치에 있는 flow vector를 얻는 메서드
        public Vector2 GetFlowVector(int x, int y) {
            if (x >= 0 && x < width && y >= 0 && y < height) {
                return flowVectors[x, y];
            }

            return Vector2.zero;
        }

        // 월드 위치에서 flow vector를 얻는 메서드 - 더 스마트한 방법으로 구현
        public Vector2 GetFlowVectorFromWorldPosition(Vector2 worldPosition) {
            int x, y;
            WorldToGrid(worldPosition, out x, out y);

            // 그리드 범위를 벗어난 경우 목표 방향으로 이동
            if (x < 0 || x >= width || y < 0 || y >= height) {
                return GetDirectionToTarget(worldPosition);
            }

            // 장애물이면 더 스마트한 회피 알고리즘 사용
            if (IsObstacle(x, y)) {
                return GetSmartAvoidanceVector(x, y, worldPosition);
            }

            // 일반적인 경우 flow vector 반환
            Vector2 flowVector = GetFlowVector(x, y);

            // 좌표 보간을 통한 더 부드러운 이동
            return SmoothFlowVector(x, y, worldPosition);
        }

        // 보간을 통한 부드러운 flow vector 계산
        private Vector2 SmoothFlowVector(int gridX, int gridY, Vector2 worldPosition) {
            // 셀 내에서의 상대적 위치 (0~1 사이)
            float cellX = (worldPosition.x - (origin.x + gridX * cellSize)) / cellSize;
            float cellY = (worldPosition.y - (origin.y + gridY * cellSize)) / cellSize;

            // 이웃한 4개 셀의 flow vector를 가져와 보간
            Vector2 bottomLeft = GetFlowVector(gridX, gridY);
            Vector2 bottomRight = GetFlowVector(Mathf.Min(gridX + 1, width - 1), gridY);
            Vector2 topLeft = GetFlowVector(gridX, Mathf.Min(gridY + 1, height - 1));
            Vector2 topRight = GetFlowVector(Mathf.Min(gridX + 1, width - 1), Mathf.Min(gridY + 1, height - 1));

            // 보간
            Vector2 bottom = Vector2.Lerp(bottomLeft, bottomRight, cellX);
            Vector2 top = Vector2.Lerp(topLeft, topRight, cellX);
            Vector2 result = Vector2.Lerp(bottom, top, cellY);

            return result.normalized;
        }

        // 목표 방향으로 직접 이동하는 벡터 계산
        private Vector2 GetDirectionToTarget(Vector2 worldPosition) {
            Vector2 targetWorldPos = new Vector2(
                origin.x + targetCell.x * cellSize + cellSize / 2,
                origin.y + targetCell.y * cellSize + cellSize / 2
            );

            return (targetWorldPos - worldPosition).normalized;
        }

        // 스마트한 장애물 회피 벡터 계산
        private Vector2 GetSmartAvoidanceVector(int x, int y, Vector2 worldPosition) {
            // 1. 가장 비용이 낮은 인접 셀 찾기
            Vector2Int bestCell = FindLowestCostNeighbor(x, y);

            // 2. 해당 셀로 향하는 방향 계산
            Vector2 bestCellWorldPos = new Vector2(
                origin.x + bestCell.x * cellSize + cellSize / 2,
                origin.y + bestCell.y * cellSize + cellSize / 2
            );

            Vector2 directionToBestCell = (bestCellWorldPos - worldPosition).normalized;

            // 3. 장애물 경계에서 약간 떨어지도록 벡터 조정
            Vector2 cellCenter = new Vector2(
                origin.x + x * cellSize + cellSize / 2,
                origin.y + y * cellSize + cellSize / 2
            );

            Vector2 fromCellCenter = (worldPosition - cellCenter).normalized;

            // 최종 방향 = 최적 셀 방향 + 약간의 셀 중심에서 멀어지는 힘
            Vector2 finalDirection = (directionToBestCell * 0.8f + fromCellCenter * 0.2f).normalized;
            return finalDirection;
        }

        // 가장 낮은 코스트를 가진 인접 셀 찾기
        private Vector2Int FindLowestCostNeighbor(int x, int y) {
            float lowestCost = float.MaxValue;
            Vector2Int bestCell = new Vector2Int(x, y);

            // 인접한 8개 방향을 탐색
            for (int offsetX = -1; offsetX <= 1; offsetX++) {
                for (int offsetY = -1; offsetY <= 1; offsetY++) {
                    if (offsetX == 0 && offsetY == 0) continue;

                    int neighborX = x + offsetX;
                    int neighborY = y + offsetY;

                    if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height
                        && !IsObstacle(neighborX, neighborY)) {
                        if (costField[neighborX, neighborY] < lowestCost) {
                            lowestCost = costField[neighborX, neighborY];
                            bestCell = new Vector2Int(neighborX, neighborY);
                        }
                    }
                }
            }

            return bestCell;
        }

        // 장애물인지 확인하는 메서드
        public bool IsObstacle(int x, int y) {
            if (x >= 0 && x < width && y >= 0 && y < height) {
                return obstaclesMap[x, y];
            }

            return false;
        }

        // 월드 위치를 grid 좌표로 변환하는 메서드
        private void WorldToGrid(Vector2 worldPosition, out int x, out int y) {
            x = Mathf.FloorToInt((worldPosition.x - origin.x) / cellSize);
            y = Mathf.FloorToInt((worldPosition.y - origin.y) / cellSize);
        }

        // 장애물이 있는 경우를 위한 flow field 생성 메서드
        public void GenerateFlowFieldWithObstacles(Vector2 targetPosition, bool[,] obstacles) {
            // 장애물 맵 저장
            this.obstaclesMap = obstacles;

            // 목표 지점의 grid 좌표
            int targetX, targetY;
            WorldToGrid(targetPosition, out targetX, out targetY);
            targetCell = new Vector2Int(targetX, targetY);

            // 각 셀의 코스트 계산을 위한 배열
            costField = new float[width, height];
            bool[,] visited = new bool[width, height];

            // 모든 셀을 최대값으로 초기화
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    costField[x, y] = float.MaxValue;
                    visited[x, y] = false;
                }
            }

            // 목표 지점의 코스트는 0
            costField[targetX, targetY] = 0;

            // A* 알고리즘을 사용하여 코스트 필드 계산 (Dijkstra보다 효율적)
            PriorityQueue<Vector2Int> openSet = new PriorityQueue<Vector2Int>();
            openSet.Enqueue(new Vector2Int(targetX, targetY), 0);

            // 인접한 8개 방향을 탐색하기 위한 방향 배열
            Vector2Int[] directions = new Vector2Int[] {
                new Vector2Int(0, 1), // 북
                new Vector2Int(1, 1), // 북동
                new Vector2Int(1, 0), // 동
                new Vector2Int(1, -1), // 남동
                new Vector2Int(0, -1), // 남
                new Vector2Int(-1, -1), // 남서
                new Vector2Int(-1, 0), // 서
                new Vector2Int(-1, 1) // 북서
            };

            while (openSet.Count > 0) {
                Vector2Int current = openSet.Dequeue();

                if (visited[current.x, current.y])
                    continue;

                visited[current.x, current.y] = true;

                // 모든 인접 셀 탐색
                foreach (Vector2Int dir in directions) {
                    int neighborX = current.x + dir.x;
                    int neighborY = current.y + dir.y;

                    // 그리드 범위 내에 있고 장애물이 아닌지 확인
                    if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height
                        && !visited[neighborX, neighborY] && !obstacles[neighborX, neighborY]) {
                        // 대각선 이동의 경우 코스트를 1.414(루트 2)로 설정, 직선 이동은 1
                        float moveCost = (dir.x != 0 && dir.y != 0) ? 1.414f : 1f;

                        // 이웃 셀의 총 코스트 계산
                        float totalCost = costField[current.x, current.y] + moveCost;

                        // 더 낮은 코스트를 발견하면 업데이트
                        if (totalCost < costField[neighborX, neighborY]) {
                            costField[neighborX, neighborY] = totalCost;

                            // 우선순위를 사용하여 다음에 처리할 셀 선택
                            // 휴리스틱으로 목표까지의 맨해튼 거리 사용
                            float priority = totalCost;
                            openSet.Enqueue(new Vector2Int(neighborX, neighborY), priority);
                        }
                    }
                }
            }

            // 코스트 필드를 기반으로 flow field 계산 - 개선된 방식으로
            CalculateFlowVectors();

            // 장애물 주변에 특별한 회피 벡터 계산
            CalculateObstacleAvoidanceVectors();
        }

        // 장애물 주변의 회피 벡터 계산
        private void CalculateObstacleAvoidanceVectors() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (obstaclesMap[x, y]) continue; // 장애물 자체는 스킵

                    // 장애물 주변에 있는지 확인
                    bool nearObstacle = false;
                    for (int offsetX = -1; offsetX <= 1; offsetX++) {
                        for (int offsetY = -1; offsetY <= 1; offsetY++) {
                            if (offsetX == 0 && offsetY == 0) continue;

                            int neighborX = x + offsetX;
                            int neighborY = y + offsetY;

                            if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height
                                && obstaclesMap[neighborX, neighborY]) {
                                nearObstacle = true;
                                break;
                            }
                        }

                        if (nearObstacle) break;
                    }

                    // 장애물 근처가 아니면 계속
                    if (!nearObstacle) continue;

                    // 장애물 회피 벡터 계산 - 가장 좋은 경로 방향과 장애물 반대 방향의 조합
                    Vector2 avoidVector = Vector2.zero;

                    // 1. 장애물로부터 멀어지는 방향
                    for (int offsetX = -1; offsetX <= 1; offsetX++) {
                        for (int offsetY = -1; offsetY <= 1; offsetY++) {
                            if (offsetX == 0 && offsetY == 0) continue;

                            int neighborX = x + offsetX;
                            int neighborY = y + offsetY;

                            if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height
                                && obstaclesMap[neighborX, neighborY]) {
                                // 장애물 반대 방향으로 힘 추가
                                avoidVector -= new Vector2(offsetX, offsetY).normalized;
                            }
                        }
                    }

                    if (avoidVector != Vector2.zero) {
                        avoidVector.Normalize();

                        // 2. 원래 flow 방향과 장애물 회피 방향을 혼합
                        flowVectors[x, y] = (flowVectors[x, y] + avoidVector * 0.3f).normalized;
                    }
                }
            }
        }

        // 코스트 필드를 기반으로 flow vector 계산
        private void CalculateFlowVectors() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (obstaclesMap[x, y]) {
                        flowVectors[x, y] = Vector2.zero;
                        continue;
                    }

                    // 현재 셀의 코스트가 무한대인 경우 (도달할 수 없는 셀)
                    if (costField[x, y] == float.MaxValue) {
                        // 목표 지점을 향한 직접적인 방향 사용
                        Vector2 directDirection = new Vector2(targetCell.x - x, targetCell.y - y).normalized;
                        flowVectors[x, y] = directDirection;
                        continue;
                    }

                    // 가장 낮은 코스트를 가진 인접 셀 찾기
                    float lowestCost = costField[x, y];
                    Vector2 bestDirection = Vector2.zero;

                    bool foundBetterNeighbor = false;

                    // 인접한 8개 방향을 탐색
                    for (int offsetX = -1; offsetX <= 1; offsetX++) {
                        for (int offsetY = -1; offsetY <= 1; offsetY++) {
                            if (offsetX == 0 && offsetY == 0) continue;

                            int neighborX = x + offsetX;
                            int neighborY = y + offsetY;

                            if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height) {
                                if (costField[neighborX, neighborY] < lowestCost) {
                                    lowestCost = costField[neighborX, neighborY];
                                    bestDirection = new Vector2(offsetX, offsetY).normalized;
                                    foundBetterNeighbor = true;
                                }
                            }
                        }
                    }

                    // 더 좋은 이웃을 찾지 못한 경우 (지역 최소값에 도달)
                    if (!foundBetterNeighbor) {
                        // 목표 방향 직접 사용
                        Vector2 directDirection = new Vector2(targetCell.x - x, targetCell.y - y).normalized;
                        flowVectors[x, y] = directDirection;
                    }
                    else {
                        // 가장 좋은 방향 설정
                        flowVectors[x, y] = bestDirection;
                    }
                }
            }
        }

        // Flow field 시각화를 위한 메서드 (디버깅 용도)
        public void DebugDrawFlowField() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Vector2 start = new Vector2(origin.x + x * cellSize + cellSize / 2, origin.y + y * cellSize + cellSize / 2);
                    Vector2 direction = flowVectors[x, y];
                    Vector2 end = start + direction * (cellSize / 2);

                    // 장애물 셀은 빨간색으로 표시
                    Color color = obstaclesMap[x, y] ? Color.red : Color.blue;
                    if (obstaclesMap[x, y]) {
                        var dir = (new Vector2(x, y) - new Vector2(targetCell.x, targetCell.y)).normalized * 0.25f;
                        end += dir;
                        start -= dir;
                    }

                    // Unity의 Debug.DrawLine 사용하여 화살표 표시
                    Debug.DrawLine(start, end, color, 0.1f);

                    // 코스트에 따라 색상 표시
                    if (!obstaclesMap[x, y] && !Mathf.Approximately(costField[x, y], float.MaxValue)) {
                        float normalizedCost = costField[x, y] / 50f; // 코스트 범위에 따라 조정 필요
                        normalizedCost = Mathf.Clamp01(normalizedCost);
                        Color costColor = Color.Lerp(Color.green, Color.yellow, normalizedCost);
                        Debug.DrawLine(start, start + Vector2.up * 0.1f, costColor, 0.1f);
                    }
                }
            }
        }
    }

    // 우선순위 큐 구현 (A* 알고리즘에 필요)
    public class PriorityQueue<T> {
        private List<KeyValuePair<T, float>> elements = new List<KeyValuePair<T, float>>();

        public int Count {
            get { return elements.Count; }
        }

        public void Enqueue(T item, float priority) {
            elements.Add(new KeyValuePair<T, float>(item, priority));
        }

        public T Dequeue() {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++) {
                if (elements[i].Value < elements[bestIndex].Value) {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Key;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}
