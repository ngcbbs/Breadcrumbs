using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Breadcrumbs.day12;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DetectTargets", story: "[Agent] detect [Target]", category: "Action", id: "b8d4a93c2eca6058ff51fef13e9df245")]
public partial class DetectTargetsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    NavMeshAgent agent;
    Sensor sensor;

    protected override Status OnStart()
    {
        agent = Agent.Value.GetComponent<NavMeshAgent>();
        sensor = Agent.Value.GetComponent<Sensor>();

        if (sensor == null)
        {
            Debug.LogError($"Sensor component not found on Agent {Agent.Value.name}");
            return Status.Failure;
        }
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var target = sensor.GetNearestTarget();
        if (target == null)
           return Status.Running;

        Target.Value = target;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

