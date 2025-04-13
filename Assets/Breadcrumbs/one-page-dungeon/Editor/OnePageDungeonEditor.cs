using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Breadcrumbs.one_page_dungeon.Editor {
    [CustomEditor(typeof(OnePageDungeon))]
    public class OnePageDungeonEditor : UnityEditor.Editor {
        private SerializedProperty _jsonData;
        private SerializedProperty _dungeonTemplate;
        private SerializedProperty _dungeonRoot;

        private OnePageDungeon _onePageDungeon;

        void OnEnable() {
            _onePageDungeon = (OnePageDungeon)target;
            _jsonData = serializedObject.FindProperty("jsonData");
            _dungeonTemplate = serializedObject.FindProperty("dungeonTemplate");
            _dungeonRoot = serializedObject.FindProperty("dungeonRoot");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            serializedObject.Update();
            if (GUILayout.Button("생 성")) {
                var buildMethod = _onePageDungeon
                    .GetType()
                    .GetMethod("Build", BindingFlags.NonPublic | BindingFlags.Instance);
                buildMethod?.Invoke(_onePageDungeon, null);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
