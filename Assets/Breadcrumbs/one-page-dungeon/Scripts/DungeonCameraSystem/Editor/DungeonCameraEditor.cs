using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonCameraController))]
public class DungeonCameraEditor : Editor {
    private SerializedProperty playerTransform;
    private SerializedProperty targetOffset;
    private SerializedProperty cameraDistance;
    private SerializedProperty cameraHeight;
    private SerializedProperty followDamping;
    private SerializedProperty rotationSpeed;
    private SerializedProperty invertYAxis;
    private SerializedProperty collisionLayers;
    private SerializedProperty collisionRadius;
    private SerializedProperty minDistanceFromPlayer;
    private SerializedProperty showDebugRays;
    private SerializedProperty dungeonBounds;
    private SerializedProperty useDungeonBounds;
    private SerializedProperty showBoundsGizmo;

    private bool showBasicSettings = true;
    private bool showCollisionSettings = true;
    private bool showBoundarySettings = true;

    private void OnEnable() {
        // Find all serialized properties
        playerTransform = serializedObject.FindProperty("playerTransform");
        targetOffset = serializedObject.FindProperty("targetOffset");
        cameraDistance = serializedObject.FindProperty("cameraDistance");
        cameraHeight = serializedObject.FindProperty("cameraHeight");
        followDamping = serializedObject.FindProperty("followDamping");
        rotationSpeed = serializedObject.FindProperty("rotationSpeed");
        invertYAxis = serializedObject.FindProperty("invertYAxis");
        collisionLayers = serializedObject.FindProperty("collisionLayers");
        collisionRadius = serializedObject.FindProperty("collisionRadius");
        minDistanceFromPlayer = serializedObject.FindProperty("minDistanceFromPlayer");
        showDebugRays = serializedObject.FindProperty("showDebugRays");
        dungeonBounds = serializedObject.FindProperty("dungeonBounds");
        useDungeonBounds = serializedObject.FindProperty("useDungeonBounds");
        showBoundsGizmo = serializedObject.FindProperty("showBoundsGizmo");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.Space();
        DrawLogo();

        EditorGUILayout.Space();
        DrawQuickSetupButtons();

        EditorGUILayout.Space();
        DrawBasicSettings();

        EditorGUILayout.Space();
        DrawCollisionSettings();

        EditorGUILayout.Space();
        DrawBoundarySettings();

        EditorGUILayout.Space();
        DrawHelpBox();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLogo() {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Dungeon Camera System", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawQuickSetupButtons() {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Auto-Find Player")) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                playerTransform.objectReferenceValue = player.transform;
            } else {
                EditorUtility.DisplayDialog("Player Not Found",
                    "No GameObject with the tag 'Player' was found in the scene. Please tag your player character or assign it manually.",
                    "OK");
            }
        }

        if (GUILayout.Button("Set Default Collision")) {
            // Set up common collision layers (everything except player, UI, etc.)
            int layerMask = ~(LayerMask.GetMask("Player", "UI", "Ignore Raycast"));
            collisionLayers.intValue = layerMask;
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawBasicSettings() {
        showBasicSettings = EditorGUILayout.Foldout(showBasicSettings, "Basic Camera Settings", true, EditorStyles.foldoutHeader);

        if (showBasicSettings) {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(playerTransform);
            EditorGUILayout.PropertyField(targetOffset);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Position and Movement", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(cameraDistance);
            EditorGUILayout.PropertyField(cameraHeight);
            EditorGUILayout.PropertyField(followDamping);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotation Controls", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(rotationSpeed);
            EditorGUILayout.PropertyField(invertYAxis);

            EditorGUI.indentLevel--;
        }
    }

    private void DrawCollisionSettings() {
        showCollisionSettings =
            EditorGUILayout.Foldout(showCollisionSettings, "Collision Settings", true, EditorStyles.foldoutHeader);

        if (showCollisionSettings) {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(collisionLayers);
            EditorGUILayout.PropertyField(collisionRadius);
            EditorGUILayout.PropertyField(minDistanceFromPlayer);
            EditorGUILayout.PropertyField(showDebugRays);

            EditorGUI.indentLevel--;
        }
    }

    private void DrawBoundarySettings() {
        showBoundarySettings =
            EditorGUILayout.Foldout(showBoundarySettings, "Dungeon Boundary Settings", true, EditorStyles.foldoutHeader);

        if (showBoundarySettings) {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(useDungeonBounds);

            if (useDungeonBounds.boolValue) {
                EditorGUILayout.PropertyField(dungeonBounds);
                EditorGUILayout.PropertyField(showBoundsGizmo);

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Quick Setup");

                if (GUILayout.Button("From Scene Bounds")) {
                    // Find a renderer or collider in the scene to estimate bounds
                    Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
                    Bounds combinedBounds = new Bounds();
                    bool foundBounds = false;

                    foreach (Renderer renderer in renderers) {
                        // Skip UI elements and other non-relevant objects
                        if (renderer.gameObject.layer == LayerMask.NameToLayer("UI"))
                            continue;

                        if (!foundBounds) {
                            combinedBounds = renderer.bounds;
                            foundBounds = true;
                        } else {
                            combinedBounds.Encapsulate(renderer.bounds);
                        }
                    }

                    if (foundBounds) {
                        // Add some padding
                        combinedBounds.Expand(5f);
                        dungeonBounds.boundsValue = combinedBounds;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }
    }

    private void DrawHelpBox() {
        EditorGUILayout.HelpBox(
            "Setup Tips:\n" +
            "1. Assign your player character to 'Player Transform'\n" +
            "2. Set up collision layers to include walls and obstacles\n" +
            "3. For special terrain features, use the DungeonTerrainDetector component on your player\n" +
            "4. Adjust dungeon bounds to prevent the camera from leaving the playable area",
            MessageType.Info);
    }
}