using UnityEditor;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// SpawnManager 컴포넌트의 커스텀 에디터
    /// </summary>
    [CustomEditor(typeof(SpawnManager))]
    public class SpawnManagerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            SpawnManager manager = (SpawnManager)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("관리 도구", EditorStyles.boldLabel);

            // 테스트 버튼들
            if (Application.isPlaying) {
                if (GUILayout.Button("이벤트 트리거: 보스 처치")) {
                    manager.TriggerEvent("BossDefeated_Default");
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("모든 스폰 포인트 그룹 활성화")) {
                    var groups = serializedObject.FindProperty("spawnPointGroups");
                    for (int i = 0; i < groups.arraySize; i++) {
                        var group = groups.GetArrayElementAtIndex(i);
                        string groupId = group.FindPropertyRelative("GroupId").stringValue;
                        manager.ActivateSpawnPointGroup(groupId);
                    }
                }

                if (GUILayout.Button("모든 스폰 포인트 그룹 비활성화")) {
                    var groups = serializedObject.FindProperty("spawnPointGroups");
                    for (int i = 0; i < groups.arraySize; i++) {
                        var group = groups.GetArrayElementAtIndex(i);
                        string groupId = group.FindPropertyRelative("GroupId").stringValue;
                        manager.DeactivateSpawnPointGroup(groupId);
                    }
                }
            }
        }
    }
}