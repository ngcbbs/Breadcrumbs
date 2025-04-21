using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class DungeonTerrainDetector : MonoBehaviour {
    [SerializeField]
    private DungeonCameraController cameraController;

    [Header("Detection Settings")]
    [SerializeField]
    private LayerMask terrainLayers;
    [SerializeField]
    private float checkRadius = 1.5f;
    [SerializeField]
    private float checkInterval = 0.2f;

    [Header("Terrain Tags")]
    [SerializeField]
    private string narrowCorridorTag = "NarrowCorridor";
    [SerializeField]
    private string stairsTag = "Stairs";
    [SerializeField]
    private string cornerTag = "Corner";

    private float lastCheckTime;
    private Collider myCollider;
    private string currentTerrainTag = "";

    private void Awake() {
        myCollider = GetComponent<Collider>();

        if (cameraController == null) {
            cameraController = FindObjectOfType<DungeonCameraController>();
            if (cameraController == null) {
                Debug.LogError("DungeonCameraController not found! Please assign it manually.");
            }
        }
    }

    private void Update() {
        // Check at regular intervals to avoid performance issues
        if (Time.time - lastCheckTime >= checkInterval) {
            CheckSurroundingTerrain();
            lastCheckTime = Time.time;
        }
    }

    private void CheckSurroundingTerrain() {
        // Get all colliders in the surrounding area
        Collider[] surroundingColliders = Physics.OverlapSphere(transform.position, checkRadius, terrainLayers);

        if (surroundingColliders.Length == 0) {
            // Reset terrain type if no special terrain is detected
            currentTerrainTag = "";
            return;
        }

        // Check for special terrain types
        foreach (Collider collider in surroundingColliders) {
            // Skip self
            if (collider == myCollider) continue;

            // Check for tags
            if (collider.CompareTag(narrowCorridorTag) && currentTerrainTag != narrowCorridorTag) {
                currentTerrainTag = narrowCorridorTag;
                cameraController.AdjustForSpecialTerrain(DungeonCameraController.TerrainType.NarrowCorridor);
                return;
            } else if (collider.CompareTag(stairsTag) && currentTerrainTag != stairsTag) {
                currentTerrainTag = stairsTag;
                cameraController.AdjustForSpecialTerrain(DungeonCameraController.TerrainType.Stairs);
                return;
            } else if (collider.CompareTag(cornerTag) && currentTerrainTag != cornerTag) {
                currentTerrainTag = cornerTag;
                cameraController.AdjustForSpecialTerrain(DungeonCameraController.TerrainType.Corner);
                return;
            }
        }
    }

    private void OnDrawGizmosSelected() {
        // Visualize detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}