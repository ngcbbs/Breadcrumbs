using UnityEngine;
using UnityEditor;
using Breadcrumbs.SpawnSystem.Strategies;

namespace Breadcrumbs.SpawnSystem.Editor
{
    [CustomEditor(typeof(SpawnPoint))]
    public class SpawnPointEditor : UnityEditor.Editor
    {
        private SerializedProperty spawnTypeProp;
        private SerializedProperty spawnPrefabProp;
        private SerializedProperty spawnTriggerProp;
        private SerializedProperty spawnDelayProp;
        private SerializedProperty minimumDifficultyProp;
        private SerializedProperty respawnAfterDeathProp;
        private SerializedProperty respawnTimeProp;
        private SerializedProperty initialRotationProp;
        private SerializedProperty positionRandomRangeProp;
        private SerializedProperty triggerAreaProp;
        private SerializedProperty maxSpawnCountProp;
        private SerializedProperty isActiveProp;
        private SerializedProperty strategyTypeProp;
        
        // Wave strategy properties
        private SerializedProperty enemiesPerWaveProp;
        private SerializedProperty timeBetweenSpawnsProp;
        private SerializedProperty timeBetweenWavesProp;
        private SerializedProperty maxWavesProp;

        private void OnEnable()
        {
            spawnTypeProp = serializedObject.FindProperty("spawnType");
            spawnPrefabProp = serializedObject.FindProperty("spawnPrefab");
            spawnTriggerProp = serializedObject.FindProperty("spawnTrigger");
            spawnDelayProp = serializedObject.FindProperty("spawnDelay");
            minimumDifficultyProp = serializedObject.FindProperty("minimumDifficulty");
            respawnAfterDeathProp = serializedObject.FindProperty("respawnAfterDeath");
            respawnTimeProp = serializedObject.FindProperty("respawnTime");
            initialRotationProp = serializedObject.FindProperty("initialRotation");
            positionRandomRangeProp = serializedObject.FindProperty("positionRandomRange");
            triggerAreaProp = serializedObject.FindProperty("triggerArea");
            maxSpawnCountProp = serializedObject.FindProperty("maxSpawnCount");
            isActiveProp = serializedObject.FindProperty("isActive");
            strategyTypeProp = serializedObject.FindProperty("strategyType");
            
            // Wave strategy properties
            enemiesPerWaveProp = serializedObject.FindProperty("enemiesPerWave");
            timeBetweenSpawnsProp = serializedObject.FindProperty("timeBetweenSpawns");
            timeBetweenWavesProp = serializedObject.FindProperty("timeBetweenWaves");
            maxWavesProp = serializedObject.FindProperty("maxWaves");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Spawn Point Settings", EditorStyles.boldLabel);
            
            // Basic Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spawnTypeProp);
            EditorGUILayout.PropertyField(spawnPrefabProp);
            
            // Spawn Conditions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Conditions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spawnTriggerProp);
            EditorGUILayout.PropertyField(spawnDelayProp);
            EditorGUILayout.PropertyField(minimumDifficultyProp);
            EditorGUILayout.PropertyField(respawnAfterDeathProp);
            
            if (respawnAfterDeathProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(respawnTimeProp);
                EditorGUI.indentLevel--;
            }
            
            // Spawn Properties
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(initialRotationProp);
            EditorGUILayout.PropertyField(positionRandomRangeProp);
            EditorGUILayout.PropertyField(triggerAreaProp);
            
            // Spawn Limits
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Limits", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(maxSpawnCountProp);
            EditorGUILayout.PropertyField(isActiveProp);
            
            // Spawn Strategy
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Strategy", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(strategyTypeProp);
            
            // Show strategy-specific properties
            var strategyType = (SpawnStrategyType)strategyTypeProp.enumValueIndex;
            
            if (strategyType == SpawnStrategyType.Wave)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(enemiesPerWaveProp);
                EditorGUILayout.PropertyField(timeBetweenSpawnsProp);
                EditorGUILayout.PropertyField(timeBetweenWavesProp);
                EditorGUILayout.PropertyField(maxWavesProp);
                EditorGUI.indentLevel--;
            }
            else if (strategyType == SpawnStrategyType.RandomSelection)
            {
                EditorGUILayout.HelpBox("For random selection strategy, configure prefab options in the script or use a custom editor.", MessageType.Info);
            }
            
            // Buttons for quick actions
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Test Spawn"))
            {
                if (Application.isPlaying)
                {
                    SpawnPoint spawnPoint = (SpawnPoint)target;
                    spawnPoint.TriggerSpawn();
                }
                else
                {
                    EditorUtility.DisplayDialog("Cannot Test Spawn", "Enter Play Mode to test spawning.", "OK");
                }
            }
            
            if (GUILayout.Button("Create Group"))
            {
                CreateSpawnGroup();
            }
            
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
        
        private void CreateSpawnGroup()
        {
            GameObject target = ((SpawnPoint)this.target).gameObject;
            
            // Check if already in a group
            if (target.transform.parent != null && target.transform.parent.GetComponent<SpawnPointGroup>() != null)
            {
                EditorUtility.DisplayDialog("Already In Group", 
                    "This spawn point is already in the group: " + target.transform.parent.name, 
                    "OK");
                return;
            }
            
            // Create a new group
            GameObject groupObj = new GameObject("SpawnPointGroup");
            SpawnPointGroup group = groupObj.AddComponent<SpawnPointGroup>();
            
            // Position the group at the spawn point's position
            groupObj.transform.position = target.transform.position;
            
            // Make the spawn point a child of the group
            Undo.SetTransformParent(target.transform, groupObj.transform, "Add to Spawn Group");
            
            // Add the spawn point to the group's list
            Undo.RecordObject(group, "Add to Spawn Group List");
            
            // Select the new group
            Selection.activeGameObject = groupObj;
            
            EditorUtility.DisplayDialog("Group Created", 
                "Created a new spawn point group with this spawn point.", 
                "OK");
        }
        
        private void OnSceneGUI()
        {
            SpawnPoint spawnPoint = (SpawnPoint)target;
            Transform transform = spawnPoint.transform;
            
            // Draw the trigger area
            Handles.color = new Color(0, 1, 0, 0.2f);
            Bounds triggerArea = spawnPoint.GetTriggerArea();
            
            // Draw a box for the trigger area
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                transform.position + triggerArea.center,
                transform.rotation,
                Vector3.one
            );
            
            using (new Handles.DrawingScope(rotationMatrix))
            {
                Handles.DrawWireCube(Vector3.zero, triggerArea.size);
            }
            
            // Draw spawn direction handle
            Handles.color = Color.blue;
            Vector3 position = transform.position;
            Quaternion rotation = spawnPoint.SpawnRotation;
            
            rotation = Handles.RotationHandle(rotation, position);
            
            // Apply the rotation if it changed
            if (rotation != spawnPoint.SpawnRotation)
            {
                Undo.RecordObject(spawnPoint, "Change Spawn Rotation");
                spawnPoint.SetSpawnRotation(rotation);
            }
        }
    }
}
