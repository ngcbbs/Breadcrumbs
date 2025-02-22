using System;
using System.Collections.Generic;
using System.Linq;
using day3_scap;
using NUnit.Framework.Internal;
using R3;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class day3 : MonoBehaviour {
    [Header("Basic Settings")]
    public SerializableReactiveProperty<Vector2Int> worldSize;
    public SerializableReactiveProperty<Vector2Int> minRoomSize;
    public SerializableReactiveProperty<int> minRoomCount;

    [Header("Visual Settings")] 
    public SerializableReactiveProperty<Color> roomColor;
    public SerializableReactiveProperty<Color> hideRoomColor;
    public SerializableReactiveProperty<Color> wayColor;
    
    [Header("Default Settings")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Transform theWorld;
    [SerializeField] private Transform theRooms;
    [SerializeField] private Transform theWay;

    // hum..
    [SerializeField] private Button generateButton;
    
    private readonly int _colorId = Shader.PropertyToID("_Color");
    private Material[] _materials;

    public class Room {
        public Vector2Int Position { get; private set; }
        public Vector2Int Size { get; private set; }

        public bool Hide { get; set; }

        public Room(Vector2Int position, Vector2Int size) {
            Position = ToEven(position);
            Size = ToEven(size);
        }

        public bool IsOverlapping(Room other) {
            var halfSize = Size / 2;
            var min = Position - halfSize;
            var max = Position + halfSize;
            var otherHalfSize = other.Size / 2;
            var otherMin = other.Position - otherHalfSize;
            var otherMax = other.Position + otherHalfSize;
            
           // Check for overlap using AABB (Axis-Aligned Bounding Box) method
            bool isOverlapping = (min.x < otherMax.x && max.x > otherMin.x) &&
                                 (min.y < otherMax.y && max.y > otherMin.y);
            return isOverlapping;
        }
    }
    
    private List<Room> _rooms = new List<Room>();

    private void Awake() {
        SetMaterials();

        worldSize.Subscribe(_ => Generate());
        minRoomSize.Subscribe(_ => Generate());
        minRoomCount.Subscribe(_ => Generate());
        roomColor.Subscribe(_ => UpdateColors());
        hideRoomColor.Subscribe(_ => UpdateColors());
        wayColor.Subscribe(_ => UpdateColors());

        Random.InitState("Hello".GetHashCode());
        generateButton.onClick.AddListener(Generate);
    }

    private void SetMaterials() {
        _materials = new Material[4] {
            Instantiate(defaultMaterial),
            Instantiate(defaultMaterial),
            Instantiate(defaultMaterial),
            Instantiate(defaultMaterial)
        };
        
    }

    private void OnWorldRefresh() {
        Debug.Log("OnWorldRefresh");
    }

    private static Vector2Int ToEven(Vector2Int value) {
        return new Vector2Int(
            value.x % 2 > 0 ? value.x + 1 : value.x,
            value.y % 2 > 0 ? value.y + 1 : value.y);
    }

    private void Generate() {
        UpdateColors();
        
        worldSize.Value = ToEven(worldSize.CurrentValue);
        minRoomSize.Value = ToEven(minRoomSize.CurrentValue);

        var center = worldSize.Value / 2;
        theWorld.localPosition = new Vector3(center.x, -1, center.y);
        theWorld.localScale = new Vector3(worldSize.Value.x, 0.1f, worldSize.Value.y);
        var mat = theWorld.GetComponent<MeshRenderer>();
        mat.sharedMaterial = _materials[0];
        
        Cleanup();
        GenerateRooms();
    }

    private void UpdateColors() {
        _materials[0].SetColor(_colorId, Color.gray);
        _materials[1].SetColor(_colorId, roomColor.Value);
        _materials[2].SetColor(_colorId, hideRoomColor.Value);
        _materials[3].SetColor(_colorId, wayColor.Value);
    }
    
    private List<Vector2Int> _gizmoPoints = new();
    private List<MSTPrims.Edge> _gizmoEdges;
    private MSTPrims _mst = new MSTPrims();
    private day3_scap.Grid _grid;
    private day3_scap.AStar _astar;

    private void GenerateRooms() {
        GC.Collect();
        
        _gizmoPoints.Clear();

        _grid ??= new day3_scap.Grid();
        _astar ??= new AStar(_grid);
        _grid.Clear();
        for (var y = 0; y < worldSize.Value.y; y++) {
            for (var x = 0; x < worldSize.Value.x; x++)
                _grid.SetWalkable(new Vector2Int(x, y));
        }
        
        _rooms.Clear();
        for (int i = 0; i < minRoomCount.Value; i++) {
            var size = RandomRoomSize;
            var position = RandomRoomPosition(size);
            _rooms.Add(new Room(position, size));
        }

        _rooms.Sort((x, y) => x.Position.x.CompareTo(y.Position.x));
        for (var i = 0; i < minRoomCount.Value; i++) {
            var room = _rooms[i];
            if (room.Hide)
                continue;
            for (var j = i + 1; j < minRoomCount.Value; j++) {
                var other = _rooms[j];
                if (other.Hide)
                    continue;
                // X축 기준으로 일정 거리 이상이면 검사 중단 (Sweep & Prune)
                if (other.Position.x > room.Position.x + room.Size.x)
                    break;
                if (room.IsOverlapping(other))
                    other.Hide = true;
            }
        }

        foreach(var room in _rooms){
            var size = room.Size;
            var position = room.Position;
            var hide = room.Hide;

            if (!hide) {
                _gizmoPoints.Add(room.Position);
                var halfSize = room.Size / 2;
                var min = room.Position - halfSize;
                var max = room.Position + halfSize;
                for (var y = min.y; y <= max.y; y++) {
                    for (var x = min.x; x <= max.x; x++)
                        _grid.SetWalkable(new Vector2Int(x, y), true, TileType.Room);        
                }
            }

            var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.transform.SetParent(theRooms, false);
            instance.transform.localPosition = new Vector3(position.x, 0, position.y);
            instance.transform.localScale = new Vector3(size.x, hide ? 0.5f : 1, size.y);
            
            var meshRenderer = instance.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _materials[hide ? 2 : 1];
            meshRenderer.shadowCastingMode = hide ? ShadowCastingMode.Off : ShadowCastingMode.On;
        }
        
        _gizmoEdges = _mst.FindMST(_gizmoPoints);
        
        // todo: Additional edge processing is required for good path shape. but.. skip!!! :P

        var pathNodeSize = Vector3.one * 0.5f;

        foreach (var edge in _gizmoEdges) {
            var path = _astar.FindPath(edge.From, edge.To);
            if (path == null) {
                Debug.Log("길찾기 실패.");
                continue;
            }

            foreach (var node in path) {
                var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                instance.transform.SetParent(theWay, false);

                instance.transform.localPosition = new Vector3(node.Index.x, 0, node.Index.y);
                instance.transform.localScale = pathNodeSize;

                var meshRenderer = instance.GetComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = _materials[3];
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }
        
        GC.Collect();
    }

    private Vector2Int RandomRoomSize =>
        new(Random.Range(minRoomSize.Value.x, minRoomSize.Value.x * 2), 
            Random.Range(minRoomSize.Value.y, minRoomSize.Value.y * 2));
    
    private Vector2Int RandomRoomPosition(Vector2Int roomSize)  {
        var margin = roomSize / 2 + Vector2Int.one;
        var maxPosition = worldSize.Value - margin;
        return new Vector2Int(Random.Range(margin.x, maxPosition.x), Random.Range(margin.y, maxPosition.y));
    }
    
    private void Cleanup() {
        Cleanup(theRooms);
        Cleanup(theWay);
    }

    private void Cleanup(Transform target) {
        var targets = new List<GameObject>();
        for (var i = 0; i < target.childCount; ++i)
            targets.Add(target.GetChild(i).gameObject);
        foreach (var go in targets)
            Destroy(go);
        targets.Clear();
    }

    private Vector3 P(Vector2Int pt, float height = 0f) {
        return new Vector3(pt.x, height, pt.y);
    }

    private void OnDrawGizmos() {
        const float height = 0.6f;
        
        Gizmos.color = Color.yellow;
        foreach (var room in _rooms.Where(room => !room.Hide)) {
            Gizmos.DrawWireSphere(P(room.Position, height), 0.5f);
        }

        if (_gizmoEdges is { Count: > 0 }) {
            Gizmos.color = Color.red;
            foreach (var edge in _gizmoEdges) {
                Gizmos.DrawLine(P(edge.From, height), P(edge.To, height));
            }
        }

        /*
        if (_grid != null) {
            foreach (var node in _grid.Nodes) {
                switch (node.Value.TileType) {
                    case TileType.Empty:
                        Gizmos.color = Color.gray;
                        break;
                    case TileType.Room:
                        Gizmos.color = Color.yellow;
                        break;
                    case TileType.Way:
                        Gizmos.color = Color.blue;
                        break;
                }
                Gizmos.DrawWireCube(P(node.Key, height), new Vector3(0.4f, 0.4f, 0.4f));
            }
        }
        // */
    }
}
