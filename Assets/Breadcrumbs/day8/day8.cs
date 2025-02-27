using System;
using Breadcrumbs.day8;
using RVO;
using UnityEngine;

public class day8 : MonoBehaviour
{
    public Transform target;
    public float radius = 5f;
    
    public UnitController[] unitControllers;

    private void OnDestroy() {
        Simulator.Instance.Clear();
        Debug.Log("Simulator.Instance.Clear()");
    }

    void Update() {
        if (unitControllers == null || unitControllers.Length == 0)
            return;
        target.transform.position = new Vector3(Mathf.Sin(Time.time) * radius, 0, Mathf.Cos(Time.time) * radius);
        foreach (var unit in unitControllers) {
            unit.goal = new Vector3(target.transform.position.x, 0, target.transform.position.z);
        }
    }
}
