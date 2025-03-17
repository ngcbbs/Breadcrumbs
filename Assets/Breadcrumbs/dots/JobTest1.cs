using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Breadcrumbs.dots {
    public class JobTest1 : MonoBehaviour {
        // Create a native array of a single float to store the result. Using a
        // NativeArray is the only way you can get the results of the job, whether
        // you're getting one value or an array of values.
        NativeArray<float> result;

        // Create a JobHandle for the job
        JobHandle handle;

        // Set up the job
        [BurstCompile]
        public struct MyJob : IJob {
            public float a;
            public float b;
            public NativeArray<float> result;

            public void Execute() {
                result[0] = a + b;
            }
        }
        
        public struct VelocityJob : IJobParallelForTransform
        {
            // Jobs declare all data that will be accessed in the job
            // By declaring it as read only, multiple jobs are allowed to access the data in parallel
            [ReadOnly]
            public NativeArray<Vector3> velocity;

            // Delta time must be copied to the job since jobs generally don't have a concept of a frame.
            // The main thread waits for the job same frame or next frame, but the job should do work deterministically
            // independent on when the job happens to run on the worker threads.
            public float deltaTime;

            // The code actually running on the job
            public void Execute(int index, TransformAccess transform)
            {
                // Move the transforms based on delta time and velocity
                var pos = transform.position;
                pos += velocity[index] * deltaTime;
                transform.position = pos;
            }
        }
        
        // Assign transforms in the inspector to be acted on by the job
        [SerializeField] public Transform[] m_Transforms;
        TransformAccessArray m_AccessArray;
        [SerializeField] private float moveSpeed = 1f;
        NativeArray<Vector3> velocity;
        JobHandle velocityHandle;

        void Awake() {
            // Store the transforms inside a TransformAccessArray instance,
            // so that the transforms can be accessed inside a job.
            m_AccessArray = new TransformAccessArray(m_Transforms);
        }

        // Update is called once per frame
        void Update() {
            // VelocityJob
            velocity = new NativeArray<Vector3>(m_Transforms.Length, Allocator.TempJob);

            var radius = 0f;
            var radiusStep = 360f / velocity.Length * Mathf.Deg2Rad;
            for (var i = 0; i < velocity.Length; ++i, radius += radiusStep) {
                velocity[i] = new Vector3(
                    Mathf.Sin(radius) * Time.deltaTime,
                    0f,
                    Mathf.Cos(radius) * Time.deltaTime) * moveSpeed;
            }

            // Initialize the job data
            var job = new VelocityJob()
            {
                deltaTime = Time.deltaTime,
                velocity = velocity
            };

            // Schedule a parallel-for-transform job.
            // The method takes a TransformAccessArray which contains the Transforms that will be acted on in the job.
            velocityHandle = job.Schedule(m_AccessArray);
            
            // Set up the job data
            result = new NativeArray<float>(1, Allocator.TempJob);

            MyJob jobData = new MyJob {
                a = 10,
                b = 10,
                result = result
            };

            // Schedule the job
            handle = jobData.Schedule(velocityHandle);
        }

        private void LateUpdate() {
            // Ensure the job has completed.
            // It is not recommended to Complete a job immediately,
            // since that reduces the chance of having other jobs run in parallel with this one.
            // You optimally want to schedule a job early in a frame and then wait for it later in the frame.
            velocityHandle.Complete();

            //Debug.Log(m_Transforms[0].position);

            // Native arrays must be disposed manually.
            velocity.Dispose();
            
            // Sometime later in the frame, wait for the job to complete before accessing the results.
            handle.Complete();

            // All copies of the NativeArray point to the same memory, you can access the result in "your" copy of the NativeArray
            // float aPlusB = result[0];
            var value = result[0];
            //Debug.Log("a + b result = " + value);

            // Free the memory allocated by the result array
            result.Dispose();
        }
    }
}