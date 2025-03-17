using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Breadcrumbs.dots.dots {
    public struct Spawner : IComponentData
    {
        public Entity Prefab;
        public float3 SpawnPosition;
        public float NextSpawnTime;
        public float SpawnRate;
        public double Time;
        public float3 Position;
    }

    // 이동 시스템
}
