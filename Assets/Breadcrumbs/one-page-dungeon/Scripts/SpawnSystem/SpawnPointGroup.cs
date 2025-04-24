using System;
using System.Collections.Generic;
using Breadcrumbs.EventSystem;
using Breadcrumbs.SpawnSystem.Events;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// Enhanced spawn point group with event system integration
    /// </summary>
    [Serializable]
    public class SpawnPointGroup : MonoBehaviour {
        [SerializeField] private string groupId;
        [SerializeField] private string groupName;
        [SerializeField] private bool isActive = true;
        [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        
        // Properties
        public string GroupId => groupId;
        public string GroupName => groupName;
        public bool IsActive => isActive;
        public List<SpawnPoint> SpawnPoints => spawnPoints;
        
        private void Awake() {
            // Generate a unique ID if none exists
            if (string.IsNullOrEmpty(groupId)) {
                groupId = System.Guid.NewGuid().ToString().Substring(0, 8);
            }
        }
        
        private void Start() {
            // Auto-collect spawn points if the list is empty
            if (spawnPoints.Count == 0) {
                spawnPoints.AddRange(GetComponentsInChildren<SpawnPoint>(true));
            }
            
            // Initialize all spawn points
            Initialize();
        }
        
        /// <summary>
        /// Initialize the group and all spawn points
        /// </summary>
        public void Initialize() {
            foreach (var spawnPoint in spawnPoints) {
                spawnPoint.SetActive(isActive);
            }
        }

        /// <summary>
        /// Set the active state of the group and all spawn points
        /// </summary>
        public void SetActive(bool active) {
            if (isActive == active) return; // No change
            
            isActive = active;
            
            foreach (var spawnPoint in spawnPoints) {
                spawnPoint.SetActive(active);
            }
            
            // Publish appropriate event
            if (active) {
                EventBehaviour.EventHandler.Dispatch(new SpawnGroupActivatedEvent(groupId));
            } else {
                EventBehaviour.EventHandler.Dispatch(new SpawnGroupDeactivatedEvent(groupId));
            }
        }
        
        /// <summary>
        /// Add a spawn point to this group
        /// </summary>
        public void AddSpawnPoint(SpawnPoint spawnPoint) {
            if (!spawnPoints.Contains(spawnPoint)) {
                spawnPoints.Add(spawnPoint);
                spawnPoint.SetActive(isActive);
                spawnPoint.transform.SetParent(transform);
            }
        }
        
        /// <summary>
        /// Remove a spawn point from this group
        /// </summary>
        public void RemoveSpawnPoint(SpawnPoint spawnPoint) {
            if (spawnPoints.Contains(spawnPoint)) {
                spawnPoints.Remove(spawnPoint);
            }
        }
        
        /// <summary>
        /// Trigger all spawn points in this group
        /// </summary>
        public void TriggerAllSpawnPoints() {
            if (!isActive) return;
            
            foreach (var spawnPoint in spawnPoints) {
                spawnPoint.TriggerSpawn();
            }
        }
        
        private void OnDrawGizmos() {
            // Visualize the group in the editor
            Gizmos.color = isActive ? new Color(0, 0.5f, 0, 0.2f) : new Color(0.5f, 0, 0, 0.2f);
            
            // Find the bounds of all spawn points
            Vector3 min = Vector3.one * float.MaxValue;
            Vector3 max = Vector3.one * float.MinValue;
            
            foreach (var spawnPoint in spawnPoints) {
                if (spawnPoint == null) continue;
                
                Vector3 pos = spawnPoint.transform.position;
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }
            
            // Draw a box around all spawn points
            if (min.x != float.MaxValue) {
                Vector3 center = (min + max) * 0.5f;
                Vector3 size = max - min + Vector3.one * 2f; // Add some padding
                Gizmos.DrawCube(center, size);
                
                // Draw the group name
                Vector3 labelPos = center + Vector3.up * (size.y * 0.5f + 1f);
                UnityEditor.Handles.Label(labelPos, $"{groupName} ({groupId})");
            }
        }
    }
}