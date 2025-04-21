using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰 포인트 그룹을 위한 에디터 창
    /// </summary>
    public class SpawnPointGroupWindow : EditorWindow {
        private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        private string newGroupId = "New_Group";
        private string newGroupName = "새 그룹";

        [MenuItem("Tools/Dungeon Game/Spawn Point Group Manager")]
        public static void ShowWindow() {
            GetWindow<SpawnPointGroupWindow>("스폰 포인트 그룹 관리자");
        }

        private void OnGUI() {
            EditorGUILayout.LabelField("스폰 포인트 그룹 생성", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("그룹 ID:", GUILayout.Width(80));
            newGroupId = EditorGUILayout.TextField(newGroupId);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("그룹 이름:", GUILayout.Width(80));
            newGroupName = EditorGUILayout.TextField(newGroupName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("선택된 스폰 포인트:", EditorStyles.boldLabel);

            // 현재 씬의 모든 스폰 포인트 검색
            if (GUILayout.Button("현재 씬의 스폰 포인트 불러오기")) {
                spawnPoints.Clear();
                SpawnPoint[] points = FindObjectsOfType<SpawnPoint>();
                spawnPoints.AddRange(points);
            }

            // 스폰 포인트 리스트 표시
            for (int i = 0; i < spawnPoints.Count; i++) {
                EditorGUILayout.BeginHorizontal();

                if (spawnPoints[i] != null) {
                    bool isSelected = EditorGUILayout.Toggle(false, GUILayout.Width(20));
                    EditorGUILayout.ObjectField(spawnPoints[i], typeof(SpawnPoint), true);

                    if (isSelected) {
                        // 선택된 오브젝트는 나중에 그룹에 추가할 때 사용
                        Selection.activeGameObject = spawnPoints[i].gameObject;
                    }
                } else {
                    EditorGUILayout.LabelField("(Missing SpawnPoint)");
                    if (GUILayout.Button("제거", GUILayout.Width(60))) {
                        spawnPoints.RemoveAt(i);
                        i--;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("선택된 스폰 포인트로 그룹 생성")) {
                CreateSpawnPointGroup();
            }
        }

        private void CreateSpawnPointGroup() {
            // 현재 씬의 SpawnManager 찾기
            SpawnManager manager = FindObjectOfType<SpawnManager>();
            if (manager == null) {
                EditorUtility.DisplayDialog("오류", "씬에 SpawnManager가 없습니다.", "확인");
                return;
            }

            // 선택된 스폰 포인트 가져오기
            var selectedSpawnPoints = new List<SpawnPoint>();
            foreach (var obj in Selection.gameObjects) {
                SpawnPoint sp = obj.GetComponent<SpawnPoint>();
                if (sp != null) {
                    selectedSpawnPoints.Add(sp);
                }
            }

            if (selectedSpawnPoints.Count == 0) {
                EditorUtility.DisplayDialog("오류", "선택된 스폰 포인트가 없습니다.", "확인");
                return;
            }

            // SpawnPointGroup 생성 및 SpawnManager에 추가
            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty groupsProperty = serializedManager.FindProperty("spawnPointGroups");

            int newIndex = groupsProperty.arraySize;
            groupsProperty.arraySize++;

            SerializedProperty newGroup = groupsProperty.GetArrayElementAtIndex(newIndex);
            newGroup.FindPropertyRelative("GroupId").stringValue = newGroupId;
            newGroup.FindPropertyRelative("GroupName").stringValue = newGroupName;
            newGroup.FindPropertyRelative("IsActive").boolValue = true;

            SerializedProperty spawnPointsProperty = newGroup.FindPropertyRelative("SpawnPoints");
            spawnPointsProperty.arraySize = selectedSpawnPoints.Count;

            for (int i = 0; i < selectedSpawnPoints.Count; i++) {
                spawnPointsProperty.GetArrayElementAtIndex(i).objectReferenceValue = selectedSpawnPoints[i];
            }

            serializedManager.ApplyModifiedProperties();

            EditorUtility.DisplayDialog("완료", $"{selectedSpawnPoints.Count}개의 스폰 포인트를 가진 '{newGroupName}' 그룹이 생성되었습니다.", "확인");
        }
    }
}