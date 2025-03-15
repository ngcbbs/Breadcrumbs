using UnityEngine;

namespace Breadcrumbs.Common {
    public static class ComponentExtensions {
        public static T GetOrAdd<T>(this Component component) where T : Component {
            var go = component.gameObject;
            var result = go.GetComponent<T>();
            if (result == null)
                result = go.AddComponent<T>();
            return result;
        }
        
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component {
            var result = gameObject.GetComponent<T>();
            if (result == null)
                result = gameObject.AddComponent<T>();
            return result;
        }
    }
}