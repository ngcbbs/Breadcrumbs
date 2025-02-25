using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.day6 {
    [CreateAssetMenu]
    public class WFCRuleTile : RuleTile
    {
        // 다른 타일과의 호환성 정보 저장
        public List<TileCompatibility> compatibleNeighbors;
    
        [System.Serializable]
        public class TileCompatibility
        {
            public WFCRuleTile otherTile;
            public bool north;
            public bool east;
            public bool south;
            public bool west;
            public bool northEast;
            public bool southEast;
            public bool southWest;
            public bool northWest;
        }
    
        // 특정 방향으로 다른 타일과 호환되는지 확인하는 메서드
        public bool IsCompatibleWith(WFCRuleTile other, Vector3Int direction)
        {
            TileCompatibility compatibility = compatibleNeighbors.Find(c => c.otherTile == other);
        
            if (compatibility == null)
                return false;
            
            if (direction == Vector3Int.up) return compatibility.north;
            if (direction == Vector3Int.right) return compatibility.east;
            if (direction == Vector3Int.down) return compatibility.south;
            if (direction == Vector3Int.left) return compatibility.west;
            if (direction == new Vector3Int(1, 1, 0)) return compatibility.northEast;
            if (direction == new Vector3Int(1, -1, 0)) return compatibility.southEast;
            if (direction == new Vector3Int(-1, -1, 0)) return compatibility.southWest;
            if (direction == new Vector3Int(-1, 1, 0)) return compatibility.northWest;
        
            return false;
        }
    }
}