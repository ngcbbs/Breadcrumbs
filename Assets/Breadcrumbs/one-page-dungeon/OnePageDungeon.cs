using Breadcrumbs.one_page_dungeon;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

public class OnePageDungeon : MonoBehaviour {
    public TextAsset jsonData;

    private OnePageDungeonData _data;

    [SerializeField] private DungeonTemplate dungeonTemplate;
    [SerializeField] private Transform dungeonRoot;

    private NavMeshSurface _navMeshSurface;

    private void Start() {
        if (jsonData == null)
            return;
        var json = jsonData.text;
        _data = OnePageDungeonData.FromJson(json);
        if (dungeonTemplate == null) {
            Debug.Log("dungeonTemplate is null");
            return;
        }
        
        // new
        var edgeTiles = dungeonTemplate.GetEdgeTiles(_data);
        EdgeBuilder.Build(edgeTiles, dungeonTemplate, dungeonRoot);
        dungeonTemplate.gameObject.SetActive(false);
        
        /*
        // old test
        dungeonTemplate.InstantiateRooms(_data, dungeonRoot);
        dungeonTemplate.gameObject.SetActive(false);
        // */

        if (dungeonRoot != null)
            dungeonRoot.localScale = new Vector3(1, 1, -1); // hum..

        // /*
        if (_navMeshSurface == null)
            _navMeshSurface = GetComponent<NavMeshSurface>();
        _navMeshSurface?.BuildNavMesh();
        // */
    }

    private void OnDrawGizmos() {
        if (_data == null)
            return;
        
        const float kTileUnits = 4f;

        var mat = Gizmos.matrix;
        Gizmos.matrix = Handles.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(1, 1, -1));

        foreach (var note in _data.Notes) {
            var origin = new Vector3(note.Pos.X, 0, note.Pos.Y) * kTileUnits;
            Handles.Label(origin, $"({note.Ref}):{note.Text}");
        }

        foreach (var rect in _data.Rects) {
            var origin = new Vector3(rect.X, 0, rect.Y) * kTileUnits;
            var size = new Vector3(rect.W, 0, rect.H) * kTileUnits;
            var center = size * 0.5f;

            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(origin, 0.1f);
            Gizmos.DrawWireCube(origin + center, size);
            if (rect.Ending == true)
                Gizmos.DrawWireCube(origin + center, size * 0.98f);
        }

        Handles.color = Color.magenta;
        foreach (var door in _data.Doors) {
            var origin = new Vector3(door.X, 0f, door.Y) * kTileUnits;
            var size = new Vector3(1, 0f, 1) * kTileUnits;
            var center = size * 0.5f;

            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireCube(origin + center, size);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(origin + center, new Vector3(door.Dir.X, door.Dir.Y));
            Handles.Label(origin + center, door.Type.ToString());
        }

        Gizmos.color = Color.blue;
        foreach (var water in _data.Water) {
            var center = new Vector3(water.X, 0, water.Y) * kTileUnits;
            var size = new Vector3(0.8f, 0f, 0.8f) * kTileUnits;
            Gizmos.DrawWireCube(center, size);
        }
    }

    private static Vector3 GetPerpendicular3D(Vector3 direction, Vector3 upHint) {
        // 방향 벡터와 평행한 upHint가 들어오면 안 됨 (외적 결과가 0)
        if (Vector3.Cross(direction, upHint).sqrMagnitude < 1e-6f) {
            // 방향 벡터와 수직인 보조 벡터가 필요함
            upHint = Vector3.right; // 다른 축으로 대체
        }

        // 외적으로 수직 벡터 구하고 정규화
        return Vector3.Cross(direction, upHint).normalized;
    }

    private static Vector3[] LineExtrude(Vector3 p1, Vector3 p2, float thickness) {
        // 방향 벡터
        var dir = (p2 - p1).normalized;

        // 수직 방향 벡터 (법선)
        var normal = GetPerpendicular3D(dir, Vector3.up);

        // 절반 두께 벡터
        var offset = normal * (thickness * 0.5f);

        // 4개의 꼭짓점 계산 (시계방향)
        var v0 = p1 + offset; // 왼쪽 위
        var v1 = p2 + offset; // 오른쪽 위
        var v2 = p2 - offset; // 오른쪽 아래
        var v3 = p1 - offset; // 왼쪽 아래
        return new [] { v0, v1, v2, v3 };
    }
}
