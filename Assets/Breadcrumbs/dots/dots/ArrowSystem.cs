using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Breadcrumbs.dots.dots {
    public struct Arrow : IComponentData {
        public float3 Velocity;
        public float3 Gravity;
    }

    public partial struct ArrowSystem  : ISystem {
        private BeginSimulationEntityCommandBufferSystem.Singleton _ecbSingleton;
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            // spawner가 여러개이면?
            foreach (var spawner in SystemAPI.Query<RefRW<Spawner>>())
            {
                SpawnArrow(ref state, spawner);
            }

            float deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (transform, arrow, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Arrow>>().WithEntityAccess()) {
                
                float3 gravityForce = new float3(0, -9.8f, 0) * Time.deltaTime;
                float3 dragForce = -math.normalize(arrow.ValueRO.Velocity) * (math.length(arrow.ValueRO.Velocity) * 0.1f * deltaTime);
                var currentVelocity = arrow.ValueRO.Velocity;
                currentVelocity += gravityForce + dragForce;
                arrow.ValueRW.Velocity = currentVelocity;

                var prePos = transform.ValueRO.Position;
                transform.ValueRW.Position += currentVelocity * deltaTime;
                var postPos = transform.ValueRO.Position;
                var dir = postPos - prePos;
                transform.ValueRW.Rotation = quaternion.LookRotation(math.normalize(dir), new float3(0, 1, 0));
                
                if (transform.ValueRO.Position.y <= 0 || transform.ValueRO.Position.y > 32)
                {
                    var ecb = _ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                    ecb.DestroyEntity(entity);
                }
            }
        }

        private void SpawnArrow(ref SystemState state, RefRW<Spawner> spawner)
        {
            
            var ecb = _ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            if (Input.GetKeyDown(KeyCode.E))
            {
                var arrowEntity = ecb.Instantiate(spawner.ValueRO.ArrowPrefab);
                
                ecb.AddComponent(arrowEntity,
                    new LocalTransform {
                        Position = spawner.ValueRO.FirePosition, 
                        Rotation = quaternion.LookRotation(math.normalize(spawner.ValueRO.FireDirection),new float3(0, 1, 0)),  
                        Scale = 1f
                    });
                
                ecb.AddComponent(arrowEntity, new Arrow {
                    Velocity = spawner.ValueRO.FireDirection * 15f /* speed */, 
                    Gravity = new float3(0, 9.8f, 0),
                });
            }
        }
    }
}