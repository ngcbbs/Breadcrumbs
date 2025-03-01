using System.Linq;
using UnityEditor;
using UnityEngine;

public class day9 : MonoBehaviour {
    void Start() {
#if UNITY_EDITOR
        // 에디터용 기능인듯! 런타임에 실행안됨.
        var types = TypeCache.GetTypesDerivedFrom<day8>().ToList();
        foreach (var type in types) {
            Debug.Log(type.Name);
        }
#endif
    }
}
