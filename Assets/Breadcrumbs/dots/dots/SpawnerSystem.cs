using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Breadcrumbs.dots.dots {
    [BurstCompile]
    public partial struct SpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Queries for all Spawner components. Uses RefRW because this system wants
            // to read from and write to the component. If the system only needed read-only
            // access, it would use RefRO instead.
            foreach (RefRW<Spawner> spawner in SystemAPI.Query<RefRW<Spawner>>())
            {
                ProcessSpawner(ref state, spawner);
            }
        }

        private void ProcessSpawner(ref SystemState state, RefRW<Spawner> spawner)
        {
            /*
            // If the next spawn time has passed.
            if (spawner.ValueRO.NextSpawnTime < SystemAPI.Time.ElapsedTime) {
                // Spawns a new entity and positions it at the spawner.
                Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);
                // LocalPosition.FromPosition returns a Transform initialized with the given position.
                state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition));

                state.EntityManager.AddComponent<Movement>(newEntity);

                // Resets the next spawn time.
                spawner.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
            }
            // */
            
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var newEntity = ecb.Instantiate(spawner.ValueRO.Prefab);
                ecb.AddComponent(newEntity,
                    new LocalTransform { Position = float3.zero, Rotation = quaternion.identity, Scale = 1f });
                ecb.AddComponent(newEntity, new Movement { Speed = 1f });
                //ecb.AddComponent(projectile, new Translation { Value = new float3(0, 0, 0) });
                //ecb.AddComponent(projectile, new MoveSpeed { Value = 10f });
            }

            /*
            // 투사체 수명 관리
            foreach (var (projectile, entity) in SystemAPI.Query<RefRW<Projectile>>().WithEntityAccess())
            {
                projectile.ValueRW.LifeTime -= SystemAPI.Time.DeltaTime;
                if (projectile.ValueRO.LifeTime <= 0)
                {
                    ecb.DestroyEntity(entity);
                }
            }
            // */
        }
    }
}