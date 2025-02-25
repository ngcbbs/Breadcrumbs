namespace Breadcrumbs.day6 {
    using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class WaveFunctionCollapseGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public List<RuleTile> possibleTiles;
    
    private Dictionary<Vector3Int, List<RuleTile>> cellPossibilities;
    private Vector2Int mapSize = new Vector2Int(20, 20);
    
    public void Generate()
    {
        // 초기화: 모든 셀에 모든 가능성 부여
        cellPossibilities = new Dictionary<Vector3Int, List<RuleTile>>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                cellPossibilities[pos] = new List<RuleTile>(possibleTiles);
            }
        }
        
        // WFC 알고리즘 반복
        while (HasUnresolvedCells())
        {
            // 엔트로피(가능성 수)가 가장 낮은 셀 찾기
            Vector3Int minEntropyPos = FindMinEntropyCell();
            
            // 해당 셀에 타일 할당 (가능한 타일 중 하나 선택)
            CollapseCell(minEntropyPos);
            
            // 제약 조건 전파
            PropagateConstraints(minEntropyPos);
        }
    }
    
    private bool HasUnresolvedCells()
    {
        foreach (var cell in cellPossibilities)
        {
            if (cell.Value.Count > 1)
                return true;
        }
        return false;
    }
    
    private Vector3Int FindMinEntropyCell()
    {
        int minEntropy = int.MaxValue;
        Vector3Int minPos = Vector3Int.zero;
        
        foreach (var cell in cellPossibilities)
        {
            if (cell.Value.Count > 1 && cell.Value.Count < minEntropy)
            {
                minEntropy = cell.Value.Count;
                minPos = cell.Key;
            }
        }
        
        return minPos;
    }
    
    private void CollapseCell(Vector3Int position)
    {
        List<RuleTile> possibilities = cellPossibilities[position];
        
        // 가능한 타일 중 하나를 랜덤하게 선택
        int randomIndex = Random.Range(0, possibilities.Count);
        RuleTile selectedTile = possibilities[randomIndex];
        
        // 해당 셀의 가능성을 선택된 하나로 줄임
        cellPossibilities[position].Clear();
        cellPossibilities[position].Add(selectedTile);
        
        // 타일맵에 적용
        tilemap.SetTile(position, selectedTile);
    }
    
    private void PropagateConstraints(Vector3Int startPos)
    {
        // 변경된 셀들을 추적하는 큐
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        frontier.Enqueue(startPos);
        
        while (frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();
            
            // 인접한 8방향 확인
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector3Int neighbor = current + new Vector3Int(dx, dy, 0);
                    
                    // 맵 범위 확인
                    if (!cellPossibilities.ContainsKey(neighbor))
                        continue;
                    
                    // 인접 셀의 가능성 업데이트
                    int beforeCount = cellPossibilities[neighbor].Count;
                    UpdateCellPossibilities(current, neighbor);
                    
                    // 가능성이 변경되었다면 전파 계속
                    if (beforeCount != cellPossibilities[neighbor].Count)
                    {
                        frontier.Enqueue(neighbor);
                    }
                }
            }
        }
    }
    
    private void UpdateCellPossibilities(Vector3Int fromCell, Vector3Int toCell)
    {
        List<RuleTile> fromPossibilities = cellPossibilities[fromCell];
        List<RuleTile> toPossibilities = cellPossibilities[toCell];
        
        // 호환되지 않는 타일 제거
        List<RuleTile> incompatibleTiles = new List<RuleTile>();
        
        foreach (RuleTile toTile in toPossibilities)
        {
            bool hasCompatibleNeighbor = false;
            
            foreach (RuleTile fromTile in fromPossibilities)
            {
                if (AreTilesCompatible(fromTile, toTile, fromCell, toCell))
                {
                    hasCompatibleNeighbor = true;
                    break;
                }
            }
            
            if (!hasCompatibleNeighbor)
            {
                incompatibleTiles.Add(toTile);
            }
        }
        
        // 호환되지 않는 타일 제거
        foreach (RuleTile tile in incompatibleTiles)
        {
            toPossibilities.Remove(tile);
        }
    }
    
    private bool AreTilesCompatible(RuleTile tileA, RuleTile tileB, Vector3Int posA, Vector3Int posB)
    {
        // RuleTile의 규칙을 검사하여 호환성 확인
        // 실제 구현은 RuleTile의 내부 규칙 구조에 따라 달라질 수 있음
        
        // 여기서는 RuleTile의 규칙을 분석하여 두 타일이 인접할 수 있는지 확인
        // 이 부분은 RuleTile의 실제 구현에 따라 달라짐
        
        Vector3Int direction = posB - posA;
        
        // RuleTile의 규칙을 분석하여 호환성 검사 로직 구현
        // ...
        
        return true; // 실제 구현에서는 규칙에 따라 호환성 결정
    }
}
}