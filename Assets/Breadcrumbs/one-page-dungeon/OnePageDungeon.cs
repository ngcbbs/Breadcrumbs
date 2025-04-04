using Breadcrumbs.one_page_dungeon;
using UnityEditor;
using UnityEngine;

public class OnePageDungeon : MonoBehaviour {
    public TextAsset jsonData;

    private OnePageDungeonData data;
    private void Start() {
        if (jsonData == null)
            return;
        var json = jsonData.text;
        data = OnePageDungeonData.FromJson(json);
    }

    private void OnDrawGizmos() {
        if (data == null)
            return;
        Gizmos.color = Color.gray;
        foreach (var rect in data.Rects) {
            var center = new Vector3(rect.X + rect.W * 0.5f, 0f, rect.Y + rect.H * 0.5f);
            var size = new Vector3(rect.W, 0, rect.H);
            Gizmos.DrawWireCube(center, size);            
        }
        
        Gizmos.color = Color.red;
        var r = Quaternion.AngleAxis(90f, new Vector3(0, 0f, 1f));
        foreach (var door in data.Doors) {
            var origin = new Vector3(door.X, 0f, door.Y);
            var dir = new Vector3(door.Dir.X, 0f, door.Dir.Y);
            
            var dd = r * dir;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, 0.2f);
            Gizmos.DrawRay(origin, dir);
            Handles.Label(origin, $"{door.Type}");
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(origin, dd);
        }
        
        Gizmos.color = Color.blue;
        foreach (var water in data.Water) {
            var center = new Vector3(water.X, 0f, water.Y);
            var size = new Vector3(0.8f, 0f, 0.8f);
            Gizmos.DrawWireCube(center, size);            
        }
    }
}
