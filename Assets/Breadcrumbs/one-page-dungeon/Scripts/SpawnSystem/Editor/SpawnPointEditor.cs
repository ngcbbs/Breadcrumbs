using UnityEditor;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// SpawnPoint 컴포넌트의 커스텀 에디터
    /// </summary>
    [CustomEditor(typeof(SpawnPoint))]
    public class SpawnPointEditor : Editor {
        private SerializedProperty spawnType;
        private SerializedProperty spawnPrefab;
        private SerializedProperty spawnTrigger;
        private SerializedProperty spawnDelay;
        private SerializedProperty requiredDifficulty;
        private SerializedProperty respawnAfterDeath;
        private SerializedProperty respawnTime;
        private SerializedProperty initialRotation;
        private SerializedProperty positionRandomRange;
        private SerializedProperty triggerArea;
        private SerializedProperty maxSpawnCount;
        private SerializedProperty isActive;

        private void OnEnable() {
            spawnType = serializedObject.FindProperty("SpawnType");
            spawnPrefab = serializedObject.FindProperty("SpawnPrefab");
            spawnTrigger = serializedObject.FindProperty("SpawnTrigger");
            spawnDelay = serializedObject.FindProperty("SpawnDelay");
            requiredDifficulty = serializedObject.FindProperty("RequiredDifficulty");
            respawnAfterDeath = serializedObject.FindProperty("respawnAfterDeath");
            respawnTime = serializedObject.FindProperty("respawnTime");
            initialRotation = serializedObject.FindProperty("InitialRotation");
            positionRandomRange = serializedObject.FindProperty("PositionRandomRange");
            triggerArea = serializedObject.FindProperty("TriggerArea");
            maxSpawnCount = serializedObject.FindProperty("maxSpawnCount");
            isActive = serializedObject.FindProperty("isActive");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.LabelField("SpawnPoint 설정", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(isActive);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("기본 설정", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(spawnType);

            EditorGUILayout.PropertyField(spawnPrefab);
            if (spawnPrefab.objectReferenceValue == null) {
                EditorGUILayout.HelpBox("스폰할 프리팹을 지정해주세요.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("스폰 조건", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(spawnTrigger);

            // SpawnTrigger에 따라 추가 UI 표시
            SpawnTriggerType triggerType = (SpawnTriggerType)spawnTrigger.enumValueIndex;

            if (triggerType == SpawnTriggerType.Timer || triggerType == SpawnTriggerType.PlayerEnter ||
                triggerType == SpawnTriggerType.Event) {
                EditorGUILayout.PropertyField(spawnDelay, new GUIContent("스폰 딜레이 (초)"));
            }

            EditorGUILayout.PropertyField(requiredDifficulty);

            if (triggerType == SpawnTriggerType.PlayerEnter) {
                EditorGUILayout.PropertyField(triggerArea);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("재스폰 설정", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(respawnAfterDeath);

            if (respawnAfterDeath.boolValue) {
                EditorGUILayout.PropertyField(respawnTime, new GUIContent("재스폰 시간 (초)"));
            }

            EditorGUILayout.PropertyField(maxSpawnCount);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("스폰 속성", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(initialRotation);
            EditorGUILayout.PropertyField(positionRandomRange, new GUIContent("랜덤 위치 범위"));

            serializedObject.ApplyModifiedProperties();

            // 테스트 버튼
            EditorGUILayout.Space();
            if (Application.isPlaying && GUILayout.Button("지금 스폰 테스트")) {
                ((SpawnPoint)target).TriggerSpawn();
            }
        }
    }
}