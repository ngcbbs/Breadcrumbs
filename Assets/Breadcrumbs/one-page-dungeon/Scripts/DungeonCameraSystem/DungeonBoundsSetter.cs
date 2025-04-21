using UnityEngine;

[ExecuteInEditMode]
public class DungeonBoundsSetter : MonoBehaviour {
    [Header("Bounds Settings")]
    [SerializeField]
    private Vector3 boundsSize = new Vector3(100f, 20f, 100f);
    [SerializeField]
    private Vector3 boundsOffset = Vector3.zero;
    [SerializeField]
    private bool autoUpdateBounds = true;
    [SerializeField]
    private bool drawGizmos = true;

    [Header("Camera Reference")]
    [SerializeField]
    private DungeonCameraController cameraController;

    private Bounds calculatedBounds;

    private void Awake() {
        if (cameraController == null) {
            cameraController = FindObjectOfType<DungeonCameraController>();
        }

        if (autoUpdateBounds) {
            UpdateBounds();
        }
    }

    private void OnValidate() {
        if (autoUpdateBounds) {
            UpdateBounds();
        }
    }

    public void UpdateBounds() {
        calculatedBounds = new Bounds(transform.position + boundsOffset, boundsSize);

        if (cameraController != null) {
            cameraController.SetDungeonBounds(calculatedBounds);
        }
    }

    // Alternative method to set bounds from collider
    public void SetBoundsFromCollider(Collider collider) {
        if (collider != null) {
            calculatedBounds = collider.bounds;

            if (cameraController != null) {
                cameraController.SetDungeonBounds(calculatedBounds);
            }
        }
    }

    private void OnDrawGizmos() {
        if (!drawGizmos) return;

        Gizmos.color = new Color(1f, 0.5f, 0.5f, 0.3f);
        Gizmos.DrawCube(transform.position + boundsOffset, boundsSize);

        Gizmos.color = new Color(1f, 0.5f, 0.5f, 0.8f);
        Gizmos.DrawWireCube(transform.position + boundsOffset, boundsSize);
    }
}