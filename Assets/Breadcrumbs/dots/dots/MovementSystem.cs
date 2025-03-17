using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Breadcrumbs.dots.dots {
    [BurstCompile]
    public partial struct MovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (transform, moveSpeed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Movement>>())
            {
                transform.ValueRW.Position += new float3(0, moveSpeed.ValueRO.Speed * deltaTime, 0);
            }
        }
    }
}