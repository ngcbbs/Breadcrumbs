using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class DungeonCameraController : MonoBehaviour {
    [Header("Target Settings")]
    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f); // Offset from player's pivot

    [Header("Camera Position")]
    [SerializeField, Range(3f, 6f)]
    private float cameraDistance = 4f;
    [SerializeField, Range(1.5f, 3f)]
    private float cameraHeight = 2f;
    [SerializeField, Range(0.1f, 1f)]
    private float followDamping = 0.2f; // Lower = smoother follow

    [Header("Camera Rotation")]
    [SerializeField, Range(1f, 10f)]
    private float rotationSpeed = 3f;
    [SerializeField]
    private bool invertYAxis = false;

    [Header("Collision Detection")]
    [SerializeField]
    private LayerMask collisionLayers;
    [SerializeField, Range(0.1f, 2f)]
    private float collisionRadius = 0.4f;
    [SerializeField, Range(1f, 3f)]
    private float minDistanceFromPlayer = 1.5f;
    [SerializeField]
    private bool showDebugRays = false;

    [Header("Dungeon Boundaries")]
    [SerializeField]
    private Bounds dungeonBounds = new Bounds(Vector3.zero, new Vector3(100f, 20f, 100f));
    [SerializeField]
    private bool useDungeonBounds = true;
    [SerializeField]
    private bool showBoundsGizmo = true;

    // Private variables
    private Vector3 currentRotation;
    private Vector3 desiredCameraPosition;
    private Vector3 smoothPosition;
    private Vector3 adjustedPosition;
    private float currentDistance;
    private Camera cameraComponent;
    private bool isColliding = false;
    private Coroutine transitionCoroutine;

    private void Awake() {
        cameraComponent = GetComponent<Camera>();
        if (playerTransform == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogError("Player not found! Assign it manually or tag your player as 'Player'");
        }

        currentDistance = cameraDistance;
        smoothPosition = transform.position;
    }

    private void LateUpdate() {
        if (playerTransform == null) return;

        // Handle manual rotation
        HandleRotationInput();

        // Calculate desired position
        Vector3 targetPosition = playerTransform.position + targetOffset;
        CalculateCameraPosition(targetPosition);

        // Apply collision avoidance
        adjustedPosition = ApplyCollisionAvoidance(targetPosition);

        // Apply dungeon boundaries
        if (useDungeonBounds) {
            adjustedPosition = ClampToDungeonBounds(adjustedPosition);
        }

        // Apply smoothing
        smoothPosition = Vector3.Lerp(smoothPosition, adjustedPosition, (1 - followDamping) * Time.deltaTime * 10f);

        // Update camera position and rotation
        transform.position = smoothPosition;
        transform.LookAt(targetPosition);
    }

    private void HandleRotationInput() {
        // Get input from mouse or right stick
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Apply rotation speed
        currentRotation.y += mouseX * rotationSpeed;
        currentRotation.x += (invertYAxis ? 1 : -1) * mouseY * rotationSpeed;

        // Clamp vertical rotation
        currentRotation.x = Mathf.Clamp(currentRotation.x, -30f, 60f);
    }

    private void CalculateCameraPosition(Vector3 targetPosition) {
        // Convert rotation to quaternion
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

        // Calculate direction and position based on current rotation
        Vector3 direction = rotation * -Vector3.forward;
        Vector3 heightOffset = new Vector3(0, cameraHeight, 0);

        // Calculate desired position
        desiredCameraPosition = targetPosition + heightOffset + direction * cameraDistance;
    }

    private Vector3 ApplyCollisionAvoidance(Vector3 targetPosition) {
        Vector3 result = desiredCameraPosition;
        RaycastHit hit;
        Vector3 direction = (desiredCameraPosition - targetPosition).normalized;
        float targetDistance = Vector3.Distance(targetPosition, desiredCameraPosition);

        // Use SphereCast to detect obstacles
        if (Physics.SphereCast(targetPosition, collisionRadius, direction, out hit, targetDistance, collisionLayers)) {
            // Calculate adjusted distance (keeping minimum safe distance)
            float adjustedDistance = hit.distance - collisionRadius;
            currentDistance = Mathf.Max(adjustedDistance, minDistanceFromPlayer);

            // Update position using the new distance
            result = targetPosition + direction * currentDistance;
            isColliding = true;

            if (showDebugRays) {
                Debug.DrawLine(targetPosition, hit.point, Color.red);
                Debug.DrawLine(hit.point, result, Color.yellow);
            }
        } else {
            // Smoothly return to original distance when no collision
            currentDistance = Mathf.Lerp(currentDistance, cameraDistance, Time.deltaTime * 5f);
            result = targetPosition + direction * currentDistance;
            isColliding = false;

            if (showDebugRays) {
                Debug.DrawLine(targetPosition, result, Color.green);
            }
        }

        return result;
    }

    private Vector3 ClampToDungeonBounds(Vector3 position) {
        // Simple position clamping to dungeon bounds
        position.x = Mathf.Clamp(position.x, dungeonBounds.min.x, dungeonBounds.max.x);
        position.y = Mathf.Clamp(position.y, dungeonBounds.min.y, dungeonBounds.max.y);
        position.z = Mathf.Clamp(position.z, dungeonBounds.min.z, dungeonBounds.max.z);

        return position;
    }

    // Reset camera to default position behind player
    public void ResetCamera() {
        if (playerTransform == null) return;

        // Get player forward direction and set rotation accordingly
        Vector3 playerForward = playerTransform.forward;
        float yRotation = Quaternion.LookRotation(playerForward).eulerAngles.y;

        // Smoothly transition to this rotation
        StartTransition(new Vector3(currentRotation.x, yRotation, 0));
    }

    private void StartTransition(Vector3 targetRotation) {
        // Cancel existing transition if any
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        // Start new transition
        transitionCoroutine = StartCoroutine(TransitionToRotation(targetRotation));
    }

    private IEnumerator TransitionToRotation(Vector3 targetRotation) {
        Vector3 startRotation = currentRotation;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration) {
            currentRotation = Vector3.Lerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentRotation = targetRotation;
        transitionCoroutine = null;
    }

    // Auto-adjust for special terrain features
    public void AdjustForSpecialTerrain(TerrainType terrainType) {
        switch (terrainType) {
            case TerrainType.NarrowCorridor:
                // Temporarily move camera closer and higher
                StartCoroutine(TemporaryAdjustment(2f, 2.5f, 1.5f));
                break;
            case TerrainType.Stairs:
                // Adjust camera to look slightly downward
                StartCoroutine(TemporaryRotationAdjustment(new Vector3(30f, currentRotation.y, 0), 2f));
                break;
            case TerrainType.Corner:
                // Widen FOV temporarily
                StartCoroutine(TemporaryFOVAdjustment(cameraComponent.fieldOfView + 10f, 1.5f));
                break;
        }
    }

    private IEnumerator TemporaryAdjustment(float tempDistance, float tempHeight, float duration) {
        float originalDistance = cameraDistance;
        float originalHeight = cameraHeight;
        float elapsed = 0f;

        while (elapsed < duration) {
            float t = elapsed / duration;

            // Apply adjustment
            if (t < 0.5f) {
                // Transition to temporary settings
                cameraDistance = Mathf.Lerp(originalDistance, tempDistance, t * 2f);
                cameraHeight = Mathf.Lerp(originalHeight, tempHeight, t * 2f);
            } else {
                // Transition back to original settings
                cameraDistance = Mathf.Lerp(tempDistance, originalDistance, (t - 0.5f) * 2f);
                cameraHeight = Mathf.Lerp(tempHeight, originalHeight, (t - 0.5f) * 2f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we're back to original values
        cameraDistance = originalDistance;
        cameraHeight = originalHeight;
    }

    private IEnumerator TemporaryRotationAdjustment(Vector3 tempRotation, float duration) {
        Vector3 originalRotation = currentRotation;
        float elapsed = 0f;

        while (elapsed < duration) {
            float t = elapsed / duration;

            // Apply adjustment
            if (t < 0.5f) {
                // Transition to temporary rotation
                currentRotation = Vector3.Lerp(originalRotation, tempRotation, t * 2f);
            } else {
                // Transition back to original rotation
                currentRotation = Vector3.Lerp(tempRotation, originalRotation, (t - 0.5f) * 2f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we're back to original rotation
        currentRotation = originalRotation;
    }

    private IEnumerator TemporaryFOVAdjustment(float tempFOV, float duration) {
        float originalFOV = cameraComponent.fieldOfView;
        float elapsed = 0f;

        while (elapsed < duration) {
            float t = elapsed / duration;

            // Apply adjustment
            if (t < 0.5f) {
                // Transition to temporary FOV
                cameraComponent.fieldOfView = Mathf.Lerp(originalFOV, tempFOV, t * 2f);
            } else {
                // Transition back to original FOV
                cameraComponent.fieldOfView = Mathf.Lerp(tempFOV, originalFOV, (t - 0.5f) * 2f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we're back to original FOV
        cameraComponent.fieldOfView = originalFOV;
    }

    public enum TerrainType {
        NarrowCorridor,
        Stairs,
        Corner
    }

    private void OnDrawGizmos() {
        if (!showBoundsGizmo || !useDungeonBounds) return;

        // Draw dungeon bounds
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        Gizmos.DrawCube(dungeonBounds.center, dungeonBounds.size);

        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.8f);
        Gizmos.DrawWireCube(dungeonBounds.center, dungeonBounds.size);
    }

    // Public method to set dungeon bounds at runtime
    public void SetDungeonBounds(Bounds newBounds) {
        dungeonBounds = newBounds;
    }
}