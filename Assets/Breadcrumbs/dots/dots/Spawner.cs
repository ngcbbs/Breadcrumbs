using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace Breadcrumbs.dots.dots {
    public struct Spawner : IComponentData
    {
        public Entity Prefab;
        public Entity ArrowPrefab;
        public float3 SpawnPosition;
        public float NextSpawnTime;
        public float SpawnRate;
        
        // 화살 발사 방향 정보
        public float3 FirePosition;
        public float3 FireDirection;
    }
}
