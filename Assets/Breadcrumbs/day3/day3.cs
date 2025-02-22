using System;
using System.Collections.Generic;
using day3_scap;
using R3;
using UnityEngine;
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
    
    private DelaunayTriangulation _delaunayTriangulation = new(); 
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
        theWorld.localScale = new Vector3(worldSize.Value.x, -1, worldSize.Value.y);
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

    private void GenerateRooms() {
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
            
            var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.transform.SetParent(theRooms, false);
            instance.transform.localPosition = new Vector3(position.x, 0, position.y);
            instance.transform.localScale = new Vector3(size.x, 1, size.y);
            
            var meshRenderer = instance.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _materials[hide ? 2 : 1];
        }
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

    private Vector3 P(Vector2Int pt) {
        return new Vector3(pt.x, 0, pt.y);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        var points = new List<Vector2Int>();
        foreach (var room in _rooms) {
            var position = new Vector3(room.Position.x, 0, room.Position.y);
            if (!room.Hide) {
                points.Add(room.Position);
                continue;
            }
            Gizmos.DrawWireSphere(position, 0.5f);
        }

        // todo: 중복 제거.
        var triangles = _delaunayTriangulation.Triangulation(points);
        foreach (var triangle in triangles) {
            Gizmos.DrawLine(P(triangle.a), P(triangle.b));
            Gizmos.DrawLine(P(triangle.b), P(triangle.c));
            Gizmos.DrawLine(P(triangle.c), P(triangle.a));
        }
    }
}
