using Unity.Entities;
using UnityEngine;

namespace Breadcrumbs.dots.dots {
    class SpawnerAuthoring : MonoBehaviour {
        public Transform firePoint;
        public GameObject Prefab;
        public GameObject ArraowPrefab;
        public float SpawnRate;
    }

    class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Spawner
            {
                // By default, each authoring GameObject turns into an Entity.
                // Given a GameObject (or authoring component), GetEntity looks up the resulting Entity.
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                ArrowPrefab = GetEntity(authoring.ArraowPrefab, TransformUsageFlags.Dynamic),
                SpawnPosition = authoring.transform.position,
                NextSpawnTime = 0.0f,
                SpawnRate = authoring.SpawnRate,
                // 화살 발사 방향 (고정만 되나?)
                FirePosition = authoring.firePoint.position,
                FireDirection = authoring.firePoint.forward, 
            });
            // 플레이 모드 시작/종료시 각각 호출됨...
        }
    }
}