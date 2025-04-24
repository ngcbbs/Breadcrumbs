using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Breadcrumbs.SpawnSystem.Editor
{
    [CustomEditor(typeof(SpawnPointGroup))]
    public class SpawnPointGroupEditor : UnityEditor.Editor
    {
        private SerializedProperty groupIdProp;
        private SerializedProperty groupNameProp;
        private SerializedProperty isActiveProp;
        private SerializedProperty spawnPointsProp;

        private void OnEnable()
        {
            groupIdProp = serializedObject.FindProperty("groupId");
            groupNameProp = serializedObject.FindProperty("groupName");
            isActiveProp = serializedObject.FindProperty("isActive");
            spawnPointsProp = serializedObject.FindProperty("spawnPoints");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Spawn Point Group Settings", EditorStyles.boldLabel);
            
            // Basic properties
            EditorGUILayout.PropertyField(groupNameProp);
            
            EditorGUI.BeginDisabledGroup(true); // Make ID read-only
            EditorGUILayout.PropertyField(groupIdProp);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.PropertyField(isActiveProp);
            
            // Spawn Points list
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Points", EditorStyles.boldLabel);
            
            // Show count and add buttons
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Count: {spawnPointsProp.arraySize}");
            
            if (GUILayout.Button("Add All Child Points"))
            {
                AddAllChildPoints();
            }
            
            if (GUILayout.Button("Clear"))
            {
                if (EditorUtility.DisplayDialog("Clear Spawn Points", 
                    "Are you sure you want to clear the spawn points list?", 
                    "Yes", "Cancel"))
                {
                    spawnPointsProp.ClearArray();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Draw the list with reorderable capabilities
            EditorGUILayout.Space();
            for (int i = 0; i < spawnPointsProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                SerializedProperty elementProp = spawnPointsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(elementProp, new GUIContent($"Spawn Point {i + 1}"));
                
                if (GUILayout.Button("↑", GUILayout.Width(25)))
                {
                    if (i > 0)
                    {
                        spawnPointsProp.MoveArrayElement(i, i - 1);
                    }
                }
                
                if (GUILayout.Button("↓", GUILayout.Width(25)))
                {
                    if (i < spawnPointsProp.arraySize - 1)
                    {
                        spawnPointsProp.MoveArrayElement(i, i + 1);
                    }
                }
                
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    spawnPointsProp.DeleteArrayElementAtIndex(i);
                    i--; // Adjust index after deletion
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Action buttons
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Add Spawn Point"))
            {
                spawnPointsProp.arraySize++;
                SerializedProperty newElement = spawnPointsProp.GetArrayElementAtIndex(spawnPointsProp.arraySize - 1);
                newElement.objectReferenceValue = null;
            }
            
            if (GUILayout.Button("Sort by Distance"))
            {
                SortSpawnPointsByDistance();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Group actions
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Generate New ID"))
            {
                groupIdProp.stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
            }
            
            if (GUILayout.Button("Toggle Active State"))
            {
                isActiveProp.boolValue = !isActiveProp.boolValue;
                
                // Also update all spawn points if in play mode
                if (Application.isPlaying)
                {
                    SpawnPointGroup group = (SpawnPointGroup)target;
                    group.SetActive(isActiveProp.boolValue);
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Runtime actions
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Runtime Actions", EditorStyles.boldLabel);
                
                if (GUILayout.Button("Trigger All Spawn Points"))
                {
                    SpawnPointGroup group = (SpawnPointGroup)target;
                    group.TriggerAllSpawnPoints();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        private void AddAllChildPoints()
        {
            SpawnPointGroup group = (SpawnPointGroup)target;
            SpawnPoint[] childPoints = group.GetComponentsInChildren<SpawnPoint>(true);
            
            if (childPoints.Length == 0)
            {
                EditorUtility.DisplayDialog("No Spawn Points Found", 
                    "No spawn points were found in the children of this group.", 
                    "OK");
                return;
            }
            
            // Clear the existing array and add all found points
            spawnPointsProp.ClearArray();
            
            foreach (SpawnPoint point in childPoints)
            {
                spawnPointsProp.arraySize++;
                spawnPointsProp.GetArrayElementAtIndex(spawnPointsProp.arraySize - 1).objectReferenceValue = point;
            }
            
            EditorUtility.DisplayDialog("Spawn Points Added", 
                $"Added {childPoints.Length} spawn points from children.", 
                "OK");
        }
        
        private void SortSpawnPointsByDistance()
        {
            SpawnPointGroup group = (SpawnPointGroup)target;
            
            // Create a list of points with their serialized property indices
            List<(SpawnPoint point, int index)> pointsWithIndices = new List<(SpawnPoint, int)>();
            
            for (int i = 0; i < spawnPointsProp.arraySize; i++)
            {
                SerializedProperty elementProp = spawnPointsProp.GetArrayElementAtIndex(i);
                SpawnPoint point = elementProp.objectReferenceValue as SpawnPoint;
                
                if (point != null)
                {
                    pointsWithIndices.Add((point, i));
                }
            }
            
            // Sort by distance from the group's position
            Vector3 groupPosition = group.transform.position;
            pointsWithIndices.Sort((a, b) => 
                Vector3.Distance(a.point.transform.position, groupPosition)
                    .CompareTo(Vector3.Distance(b.point.transform.position, groupPosition)));
            
            // Create a new array and copy the values in sorted order
            SerializedProperty[] properties = new SerializedProperty[spawnPointsProp.arraySize];
            for (int i = 0; i < spawnPointsProp.arraySize; i++)
            {
                properties[i] = spawnPointsProp.GetArrayElementAtIndex(i).Copy();
            }
            
            // Apply the sorted order
            for (int i = 0; i < pointsWithIndices.Count; i++)
            {
                int oldIndex = pointsWithIndices[i].index;
                spawnPointsProp.GetArrayElementAtIndex(i).objectReferenceValue = properties[oldIndex].objectReferenceValue;
            }
        }
        
        private void OnSceneGUI()
        {
            SpawnPointGroup group = (SpawnPointGroup)target;
            
            // Draw connections between the group and its spawn points
            Handles.color = group.IsActive ? Color.green : Color.red;
            
            foreach (var spawnPoint in group.SpawnPoints)
            {
                if (spawnPoint != null)
                {
                    Handles.DrawDottedLine(
                        group.transform.position, 
                        spawnPoint.transform.position, 
                        2f);
                }
            }
            
            // Label with group name and ID
            Handles.Label(group.transform.position + Vector3.up * 2f, 
                $"{group.GroupName} ({group.GroupId})");
        }
    }
}
