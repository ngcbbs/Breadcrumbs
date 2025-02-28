using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Breadcrumbs.day9.Editor {
    public static class day9Test
    {
        [MenuItem("Tools/Day9/Reflection")]
        public static void WithReflection() {
            int count = 0;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    if (type.Name.StartsWith("day"))
                        count++;
                    /*
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                 BindingFlags.Static | BindingFlags.Instance);
                    foreach (var method in methods) {
                    }
                    // */
                }
            }
            Debug.Log($"Found: {count}");
        }

        [MenuItem("Tools/Day9/TypeCache")]
        public static void WithTypeCache() {
            int count = 0;
            // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/TypeCache.html
            // GetFieldsWithAttribute,
            // GetMethodsWithAttribute,
            // GetTypesDerivedFrom
            // GetTypesWithAttribute
            foreach (var type in TypeCache.GetTypesDerivedFrom<MonoBehaviour>()) {
                if (type.Name.StartsWith("day"))
                    count++;
            }
            Debug.Log($"Found: {count}");
        }
    }
}
