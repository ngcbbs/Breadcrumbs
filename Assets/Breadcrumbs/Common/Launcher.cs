using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Breadcrumbs.Common {
    // um.. LauncherTypes?
    public enum LauncherType {
        Direction,
    }
    public class Launcher : MonoBehaviour {
        [SerializeField] private Vector2 size = Vector2.one;

        public (Vector3 position, Quaternion rotation, Vector3 forward) GetLauncherInfo() {
            var circlePos = Random.insideUnitCircle * size;
            return (transform.TransformPoint(circlePos), transform.rotation, transform.forward);
        }
    
        private void OnDrawGizmos() {
            var center = transform.position;
            var direction = transform.forward;
            var right = transform.right;
            
            Gizmos.color = new Color(1f, 1f, 0, 0.5f);
            var lines = new List<Vector3>();
            var radius = 360f / 32f * Mathf.Deg2Rad;
            for (int i = 0; i < 32; ++i) {
                lines.Add(transform.TransformPoint(new Vector3(
                    Mathf.Sin(radius * i) * size.x,
                    Mathf.Cos(radius * i) * size.y,
                    0f
                )));
            }
            Gizmos.DrawLineStrip(lines.ToArray(), true);
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
            
            Gizmos.DrawLine(center, center + direction);
            Gizmos.DrawLine(center + direction, center + direction * 0.5f + right * 0.5f);
            Gizmos.DrawLine(center + direction, center + direction * 0.5f - right * 0.5f);
        }
    }
}
