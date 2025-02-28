using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Breadcrumbs.day9.Editor {
    [FilePath("ProjectSettings/Day9Settings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class Day9Settings : ScriptableSingleton<Day9Settings> {
        [SerializeField] public bool day9Flag;

        public bool Day9Flag {
            get => day9Flag;
            set {
                day9Flag = value;
                Save(true);
            }
        }
    }

    public class Day9SettingProvider : SettingsProvider {
        public Day9SettingProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes,
            keywords) {
            
        }

        public override void OnGUI(string searchContext) {
            base.OnGUI(searchContext);
            
            GUILayout.Space(10f);
            
            bool flag = Day9Settings.instance.day9Flag;
            bool value = EditorGUILayout.Toggle("Day9 flag", flag, GUILayout.ExpandWidth(false));
            if (value != flag)
                Day9Settings.instance.day9Flag = value;
        }

        [SettingsProvider]
        public static SettingsProvider CreateDay9SettingsProvider() {
            return new Day9SettingProvider("Tool/Day9 Setting", SettingsScope.User);
        }
    }
}