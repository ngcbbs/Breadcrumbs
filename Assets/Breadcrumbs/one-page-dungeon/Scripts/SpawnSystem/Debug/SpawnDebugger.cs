using System.Collections.Generic;
using Breadcrumbs.EventSystem;
using Breadcrumbs.SpawnSystem.Events;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Breadcrumbs.SpawnSystem.Debugger {
    /// <summary>
    /// Utility class for debugging spawn-related activities
    /// </summary>
    public class SpawnDebugger : MonoBehaviour {
        [Header("Debug Settings")]
        [SerializeField]
        private bool enableLogging = true;
        [SerializeField]
        private bool visualizeSpawnPoints = true;
        [SerializeField]
        private bool trackSpawnEvents = true;
        [SerializeField]
        private Color spawnEventColor = Color.green;
        [SerializeField]
        private Color despawnEventColor = Color.red;
        [SerializeField]
        private float eventMarkerDuration = 3f;
        [SerializeField]
        private int maxTrackedEvents = 100;

        [Header("Performance Monitoring")]
        [SerializeField]
        private bool monitorPerformance = true;
        [SerializeField]
        private float warningThreshold = 10;

        // Event tracking
        private class EventMarker {
            public Vector3 Position;
            public string Label;
            public Color Color;
            public float Time;
            public float Duration;
        }

        private List<EventMarker> _eventMarkers = new List<EventMarker>();

        // Performance tracking
        private Dictionary<string, int> _spawnCounts = new Dictionary<string, int>();
        private Dictionary<string, float> _spawnTimes = new Dictionary<string, float>();

        private void OnEnable() {
            // Subscribe to events
            if (trackSpawnEvents) {
                EventBehaviour.EventHandler.Register(typeof(SpawnEvent), OnSpawnEvent);
                EventBehaviour.EventHandler.Register(typeof(DespawnEvent), OnDespawnEvent);
            }
        }

        private void OnDisable() {
            // Unsubscribe from events
            if (trackSpawnEvents) {
                EventBehaviour.EventHandler.Unregister(typeof(SpawnEvent), OnSpawnEvent);
                EventBehaviour.EventHandler.Unregister(typeof(DespawnEvent), OnDespawnEvent);
            }
        }

        private void OnSpawnEvent(IEvent evt) {
            if (!enableLogging && !visualizeSpawnPoints) return;

            var spawnEvent = evt as SpawnEvent;
            if (spawnEvent == null) return;

            string objectName = spawnEvent.SpawnedObject.name;
            Vector3 position = spawnEvent.SpawnedObject.transform.position;

            // Log the event
            if (enableLogging) {
                UnityEngine.Debug.Log($"[Spawn] {objectName} spawned at {position}");
            }

            // Track performance
            if (monitorPerformance) {
                // Track spawn count by type
                string prefabName = objectName.Split('_')[0]; // Get base name without instance IDs
                if (!_spawnCounts.ContainsKey(prefabName)) {
                    _spawnCounts[prefabName] = 0;
                }

                _spawnCounts[prefabName]++;

                // Check if we're spawning too many objects
                if (_spawnCounts[prefabName] > warningThreshold) {
                    UnityEngine.Debug.LogWarning(
                        $"[SpawnDebugger] High spawn count for {prefabName}: {_spawnCounts[prefabName]}");
                }
            }

            // Add visual marker
            if (visualizeSpawnPoints) {
                _eventMarkers.Add(new EventMarker {
                    Position = position,
                    Label = $"Spawn: {objectName}",
                    Color = spawnEventColor,
                    Time = Time.time,
                    Duration = eventMarkerDuration
                });

                // Limit the number of markers
                if (_eventMarkers.Count > maxTrackedEvents) {
                    _eventMarkers.RemoveAt(0);
                }
            }
        }

        private void OnDespawnEvent(IEvent evt) {
            if (!enableLogging && !visualizeSpawnPoints) return;

            var despawnEvent = evt as DespawnEvent;
            if (despawnEvent == null) return;

            string objectName = despawnEvent.DespawnedObject.name;
            Vector3 position = despawnEvent.DespawnedObject.transform.position;

            // Log the event
            if (enableLogging) {
                UnityEngine.Debug.Log($"[Despawn] {objectName} despawned at {position}");
            }

            // Track performance
            if (monitorPerformance) {
                // Track despawn by type
                string prefabName = objectName.Split('_')[0]; // Get base name without instance IDs
                if (_spawnCounts.ContainsKey(prefabName)) {
                    _spawnCounts[prefabName]--;
                }
            }

            // Add visual marker
            if (visualizeSpawnPoints) {
                _eventMarkers.Add(new EventMarker {
                    Position = position,
                    Label = $"Despawn: {objectName}",
                    Color = despawnEventColor,
                    Time = Time.time,
                    Duration = eventMarkerDuration
                });

                // Limit the number of markers
                if (_eventMarkers.Count > maxTrackedEvents) {
                    _eventMarkers.RemoveAt(0);
                }
            }
        }

        private void Update() {
            // Update the event markers list by removing expired ones
            if (visualizeSpawnPoints) {
                for (int i = _eventMarkers.Count - 1; i >= 0; i--) {
                    if (Time.time - _eventMarkers[i].Time > _eventMarkers[i].Duration) {
                        _eventMarkers.RemoveAt(i);
                    }
                }
            }
        }

        private void OnDrawGizmos() {
            if (!visualizeSpawnPoints) return;

            // Draw event markers
            foreach (var marker in _eventMarkers) {
                float remainingTime = marker.Duration - (Time.time - marker.Time);
                if (remainingTime <= 0) continue;

                // Fade out as time passes
                float alpha = remainingTime / marker.Duration;
                Gizmos.color = new Color(marker.Color.r, marker.Color.g, marker.Color.b, alpha);

                // Draw a sphere at the event position
                Gizmos.DrawSphere(marker.Position, 0.3f);

                // Draw a line from the ground up to the event position
                Gizmos.DrawLine(new Vector3(marker.Position.x, 0, marker.Position.z), marker.Position);

#if UNITY_EDITOR
                // Draw label
                Handles.color = new Color(marker.Color.r, marker.Color.g, marker.Color.b, alpha);
                Handles.Label(marker.Position + Vector3.up * 0.5f, marker.Label);
#endif
            }
        }

        private void OnGUI() {
            if (!monitorPerformance) return;

            // Draw performance monitor in the top-right corner
            GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 290, 300));
            GUILayout.BeginVertical("box");

            GUILayout.Label("Spawn System Performance", EditorStyles.boldLabel);

            // Show active counts by type
            foreach (var kvp in _spawnCounts) {
                if (kvp.Value <= 0) continue; // Skip zero counts

                Color textColor = kvp.Value > warningThreshold ? Color.red : Color.white;
                GUI.color = textColor;
                GUILayout.Label($"{kvp.Key}: {kvp.Value} active");
            }

            // Reset color
            GUI.color = Color.white;

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
