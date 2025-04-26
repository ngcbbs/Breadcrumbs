# 프로시저럴 던전 생성 알고리즘 예제

BSP(Binary Space Partitioning) 알고리즘을 활용한 던전 생성 시스템의 핵심 클래스들입니다.

## BSP 노드 클래스

```csharp
// BSP 노드 클래스 예시
public class BSPNode
{
    public RectInt Space { get; private set; }
    public BSPNode Left { get; private set; }
    public BSPNode Right { get; private set; }
    public Room Room { get; private set; }
    
    public bool IsLeaf => Left == null && Right == null;
    
    public BSPNode(RectInt space)
    {
        Space = space;
    }
    
    // 공간 분할 메서드
    public bool Split(int minSize, float splitRandomness)
    {
        // 이미 분할된 경우 더 이상 분할하지 않음
        if (!IsLeaf) return false;
        
        // 공간이 너무 작으면 분할하지 않음
        if (Space.width < minSize * 2 || Space.height < minSize * 2)
            return false;
            
        // 수평 또는 수직 분할 결정
        bool horizontalSplit;
        
        if (Space.width > Space.height * 1.25f)
            horizontalSplit = false; // 너비가 높이보다 훨씬 크면 수직 분할
        else if (Space.height > Space.width * 1.25f)
            horizontalSplit = true; // 높이가 너비보다 훨씬 크면 수평 분할
        else
            horizontalSplit = Random.value > 0.5f; // 그 외의 경우 무작위 분할
            
        // 분할 위치 계산 (임의성 추가)
        float splitRatio = Mathf.Clamp(0.5f + (Random.value - 0.5f) * splitRandomness, 0.4f, 0.6f);
        
        if (horizontalSplit)
        {
            // 수평 분할
            int splitPoint = Mathf.FloorToInt(Space.height * splitRatio);
            
            if (splitPoint < minSize || Space.height - splitPoint < minSize)
                return false;
                
            Left = new BSPNode(new RectInt(Space.x, Space.y, Space.width, splitPoint));
            Right = new BSPNode(new RectInt(Space.x, Space.y + splitPoint, Space.width, Space.height - splitPoint));
        }
        else
        {
            // 수직 분할
            int splitPoint = Mathf.FloorToInt(Space.width * splitRatio);
            
            if (splitPoint < minSize || Space.width - splitPoint < minSize)
                return false;
                
            Left = new BSPNode(new RectInt(Space.x, Space.y, splitPoint, Space.height));
            Right = new BSPNode(new RectInt(Space.x + splitPoint, Space.y, Space.width - splitPoint, Space.height));
        }
        
        return true;
    }
    
    // 방 생성 메서드
    public Room CreateRoom(int padding)
    {
        if (!IsLeaf || Room != null)
            return Room;
            
        // 패딩을 적용한 방 크기 계산
        int paddingX = Mathf.Min(padding, Space.width / 4);
        int paddingY = Mathf.Min(padding, Space.height / 4);
        
        // 방 위치와 크기 계산 (패딩 적용)
        int x = Space.x + Random.Range(paddingX, paddingX * 2);
        int y = Space.y + Random.Range(paddingY, paddingY * 2);
        int width = Space.width - (x - Space.x) - Random.Range(paddingX, paddingX * 2);
        int height = Space.height - (y - Space.y) - Random.Range(paddingY, paddingY * 2);
        
        // 최소 크기 확인
        if (width < 3 || height < 3)
            return null;
            
        // 방 객체 생성
        Room = new Room(new RectInt(x, y, width, height));
        return Room;
    }
    
    // 모든 방 목록 가져오기
    public void GetRooms(List<Room> rooms)
    {
        if (Room != null)
            rooms.Add(Room);
            
        if (Left != null)
            Left.GetRooms(rooms);
            
        if (Right != null)
            Right.GetRooms(rooms);
    }
    
    // 모든 리프 노드 가져오기
    public void GetLeaves(List<BSPNode> leaves)
    {
        if (IsLeaf)
            leaves.Add(this);
        else
        {
            Left?.GetLeaves(leaves);
            Right?.GetLeaves(leaves);
        }
    }
}
```

## 던전 생성기 클래스

```csharp
// 던전 생성기 클래스 예시
public class DungeonGenerator
{
    // 생성 파라미터
    public int Width { get; set; } = 100;
    public int Height { get; set; } = 100;
    public int MinRoomSize { get; set; } = 10;
    public int MaxDepth { get; set; } = 5;
    public float SplitRandomness { get; set; } = 0.3f;
    public int RoomPadding { get; set; } = 2;
    public int CorridorWidth { get; set; } = 2;
    
    // 던전 생성 메서드
    public Dungeon GenerateDungeon(int seed)
    {
        Random.InitState(seed);
        
        // BSP 트리 생성 및 방 배치
        BSPNode rootNode = new BSPNode(new RectInt(0, 0, Width, Height));
        SplitRecursively(rootNode, 0);
        
        // 방 생성
        List<Room> rooms = CreateRooms(rootNode);
        
        // 복도 연결
        List<Corridor> corridors = ConnectRooms(rootNode, rooms);
        
        // 던전 객체 생성 및 반환
        return new Dungeon(rooms, corridors, Width, Height, seed);
    }
    
    private void SplitRecursively(BSPNode node, int depth)
    {
        // 최대 깊이에 도달하면 분할 중지
        if (depth >= MaxDepth)
            return;
            
        // 노드 분할 시도
        if (node.Split(MinRoomSize, SplitRandomness))
        {
            // 자식 노드가 생성되면 재귀적으로 분할
            SplitRecursively(node.Left, depth + 1);
            SplitRecursively(node.Right, depth + 1);
        }
    }
    
    private List<Room> CreateRooms(BSPNode node)
    {
        List<BSPNode> leaves = new List<BSPNode>();
        node.GetLeaves(leaves);
        
        List<Room> rooms = new List<Room>();
        
        foreach (var leaf in leaves)
        {
            Room room = leaf.CreateRoom(RoomPadding);
            if (room != null)
                rooms.Add(room);
        }
        
        return rooms;
    }
    
    private List<Corridor> ConnectRooms(BSPNode node, List<Room> rooms)
    {
        List<Corridor> corridors = new List<Corridor>();
        
        // 재귀적으로 연결 복도 생성
        CreateCorridorsRecursive(node, corridors);
        
        return corridors;
    }
    
    private void CreateCorridorsRecursive(BSPNode node, List<Corridor> corridors)
    {
        // 리프 노드가 아닌 경우에만 처리
        if (node.Left != null && node.Right != null)
        {
            // 왼쪽 자식과 오른쪽 자식의 방 목록 가져오기
            List<Room> leftRooms = new List<Room>();
            List<Room> rightRooms = new List<Room>();
            
            node.Left.GetRooms(leftRooms);
            node.Right.GetRooms(rightRooms);
            
            if (leftRooms.Count > 0 && rightRooms.Count > 0)
            {
                // 연결할 두 방 선택
                Room leftRoom = leftRooms[Random.Range(0, leftRooms.Count)];
                Room rightRoom = rightRooms[Random.Range(0, rightRooms.Count)];
                
                // 두 방 사이에 복도 생성
                corridors.Add(CreateCorridor(leftRoom, rightRoom));
            }
            
            // 자식 노드들도 재귀적으로 처리
            CreateCorridorsRecursive(node.Left, corridors);
            CreateCorridorsRecursive(node.Right, corridors);
        }
    }
    
    private Corridor CreateCorridor(Room roomA, Room roomB)
    {
        // 두 방의 중심점 계산
        Vector2Int centerA = new Vector2Int(
            roomA.Bounds.x + roomA.Bounds.width / 2,
            roomA.Bounds.y + roomA.Bounds.height / 2
        );
        
        Vector2Int centerB = new Vector2Int(
            roomB.Bounds.x + roomB.Bounds.width / 2,
            roomB.Bounds.y + roomB.Bounds.height / 2
        );
        
        List<Vector2Int> path = new List<Vector2Int>();
        
        // L자형 복도 생성
        if (Random.value > 0.5f)
        {
            // 먼저 수평으로 이동
            CreateHorizontalCorridor(centerA.x, centerB.x, centerA.y, path);
            // 그 다음 수직으로 이동
            CreateVerticalCorridor(centerA.y, centerB.y, centerB.x, path);
        }
        else
        {
            // 먼저 수직으로 이동
            CreateVerticalCorridor(centerA.y, centerB.y, centerA.x, path);
            // 그 다음 수평으로 이동
            CreateHorizontalCorridor(centerA.x, centerB.x, centerB.y, path);
        }
        
        return new Corridor(path, CorridorWidth);
    }
    
    private void CreateHorizontalCorridor(int startX, int endX, int y, List<Vector2Int> path)
    {
        int start = Mathf.Min(startX, endX);
        int end = Mathf.Max(startX, endX);
        
        for (int x = start; x <= end; x++)
        {
            path.Add(new Vector2Int(x, y));
        }
    }
    
    private void CreateVerticalCorridor(int startY, int endY, int x, List<Vector2Int> path)
    {
        int start = Mathf.Min(startY, endY);
        int end = Mathf.Max(startY, endY);
        
        for (int y = start; y <= end; y++)
        {
            path.Add(new Vector2Int(x, y));
        }
    }
}
```

## 던전, 방, 복도 클래스

```csharp
// 던전 클래스
public class Dungeon
{
    public List<Room> Rooms { get; private set; }
    public List<Corridor> Corridors { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Seed { get; private set; }
    
    public Dungeon(List<Room> rooms, List<Corridor> corridors, int width, int height, int seed)
    {
        Rooms = rooms;
        Corridors = corridors;
        Width = width;
        Height = height;
        Seed = seed;
    }
    
    // 던전 타일 그리드 생성
    public TileType[,] CreateTileGrid()
    {
        TileType[,] grid = new TileType[Width, Height];
        
        // 초기화 - 모든 타일을 벽으로 설정
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                grid[x, y] = TileType.Wall;
            }
        }
        
        // 방 타일 설정
        foreach (var room in Rooms)
        {
            for (int x = room.Bounds.x; x < room.Bounds.x + room.Bounds.width; x++)
            {
                for (int y = room.Bounds.y; y < room.Bounds.y + room.Bounds.height; y++)
                {
                    if (x == room.Bounds.x || x == room.Bounds.x + room.Bounds.width - 1 ||
                        y == room.Bounds.y || y == room.Bounds.y + room.Bounds.height - 1)
                    {
                        // 방의 가장자리는 벽으로 유지
                        continue;
                    }
                    
                    grid[x, y] = TileType.Floor;
                }
            }
        }
        
        // 복도 타일 설정
        foreach (var corridor in Corridors)
        {
            foreach (var point in corridor.Path)
            {
                int halfWidth = corridor.Width / 2;
                
                for (int x = point.x - halfWidth; x <= point.x + halfWidth; x++)
                {
                    for (int y = point.y - halfWidth; y <= point.y + halfWidth; y++)
                    {
                        if (x >= 0 && x < Width && y >= 0 && y < Height)
                        {
                            grid[x, y] = TileType.Floor;
                        }
                    }
                }
            }
        }
        
        return grid;
    }
    
    // 데이터 직렬화 메서드
    public DungeonData ToData()
    {
        DungeonData data = new DungeonData
        {
            Width = Width,
            Height = Height,
            Seed = Seed,
            Rooms = new List<RoomData>(),
            Corridors = new List<CorridorData>()
        };
        
        // 방 데이터 변환
        foreach (var room in Rooms)
        {
            data.Rooms.Add(new RoomData
            {
                X = room.Bounds.x,
                Y = room.Bounds.y,
                Width = room.Bounds.width,
                Height = room.Bounds.height,
                Type = room.Type
            });
        }
        
        // 복도 데이터 변환
        foreach (var corridor in Corridors)
        {
            List<Vector2Int> pathPoints = new List<Vector2Int>();
            pathPoints.AddRange(corridor.Path);
            
            data.Corridors.Add(new CorridorData
            {
                Path = pathPoints.Select(p => new Vector2IntData { X = p.x, Y = p.y }).ToList(),
                Width = corridor.Width
            });
        }
        
        return data;
    }
}

// 방 클래스
public class Room
{
    public RectInt Bounds { get; private set; }
    public RoomType Type { get; set; } = RoomType.Normal;
    
    public Room(RectInt bounds)
    {
        Bounds = bounds;
    }
    
    public Vector2Int GetCenter()
    {
        return new Vector2Int(
            Bounds.x + Bounds.width / 2,
            Bounds.y + Bounds.height / 2
        );
    }
    
    // 방 유형 열거형
    public enum RoomType
    {
        Normal,
        Entrance,
        Exit,
        Treasure,
        Shop,
        Boss
    }
}

// 복도 클래스
public class Corridor
{
    public List<Vector2Int> Path { get; private set; }
    public int Width { get; private set; }
    
    public Corridor(List<Vector2Int> path, int width = 1)
    {
        Path = path;
        Width = width;
    }
}

// 타일 유형 열거형
public enum TileType
{
    Wall,
    Floor,
    Door,
    Stairs,
    Water,
    Lava,
    Trap
}

// 직렬화를 위한 데이터 클래스들
[MessagePackObject]
public class DungeonData
{
    [Key(0)]
    public int Width { get; set; }
    
    [Key(1)]
    public int Height { get; set; }
    
    [Key(2)]
    public int Seed { get; set; }
    
    [Key(3)]
    public List<RoomData> Rooms { get; set; }
    
    [Key(4)]
    public List<CorridorData> Corridors { get; set; }
}

[MessagePackObject]
public class RoomData
{
    [Key(0)]
    public int X { get; set; }
    
    [Key(1)]
    public int Y { get; set; }
    
    [Key(2)]
    public int Width { get; set; }
    
    [Key(3)]
    public int Height { get; set; }
    
    [Key(4)]
    public Room.RoomType Type { get; set; }
}

[MessagePackObject]
public class CorridorData
{
    [Key(0)]
    public List<Vector2IntData> Path { get; set; }
    
    [Key(1)]
    public int Width { get; set; }
}

[MessagePackObject]
public class Vector2IntData
{
    [Key(0)]
    public int X { get; set; }
    
    [Key(1)]
    public int Y { get; set; }
}
```

## 던전 시각화 유틸리티

```csharp
// 던전 시각화 클래스
public class DungeonVisualizer : MonoBehaviour
{
    [SerializeField] private Transform floorParent;
    [SerializeField] private Transform wallParent;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject stairsPrefab;
    
    [Header("Generation Settings")]
    [SerializeField] private DungeonGenerationSettings settings;
    
    private DungeonGenerator generator;
    private Dungeon currentDungeon;
    
    private void Awake()
    {
        generator = new DungeonGenerator
        {
            Width = settings.Width,
            Height = settings.Height,
            MinRoomSize = settings.MinRoomSize,
            MaxDepth = settings.MaxDepth,
            SplitRandomness = settings.SplitRandomness,
            RoomPadding = settings.RoomPadding,
            CorridorWidth = settings.CorridorWidth
        };
    }
    
    public void GenerateAndVisualize(int? seed = null)
    {
        ClearDungeon();
        
        int dungeonSeed = seed ?? Random.Range(0, 999999);
        currentDungeon = generator.GenerateDungeon(dungeonSeed);
        
        StartCoroutine(VisualizeDungeonRoutine(currentDungeon));
    }
    
    private IEnumerator VisualizeDungeonRoutine(Dungeon dungeon)
    {
        TileType[,] grid = dungeon.CreateTileGrid();
        
        // 타일 그리드 시각화
        for (int x = 0; x < dungeon.Width; x++)
        {
            for (int y = 0; y < dungeon.Height; y++)
            {
                if (grid[x, y] == TileType.Floor)
                {
                    // 바닥 타일 생성
                    GameObject floor = Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity, floorParent);
                    floor.name = $"Floor_{x}_{y}";
                }
                else if (grid[x, y] == TileType.Wall)
                {
                    // 벽 타일 생성 (주변 타일이 바닥인 경우에만)
                    bool createWall = false;
                    
                    for (int nx = x - 1; nx <= x + 1; nx++)
                    {
                        for (int ny = y - 1; ny <= y + 1; ny++)
                        {
                            if (nx >= 0 && nx < dungeon.Width &&
                                ny >= 0 && ny < dungeon.Height &&
                                grid[nx, ny] == TileType.Floor)
                            {
                                createWall = true;
                                break;
                            }
                        }
                        
                        if (createWall) break;
                    }
                    
                    if (createWall)
                    {
                        GameObject wall = Instantiate(wallPrefab, new Vector3(x, 0, y), Quaternion.identity, wallParent);
                        wall.name = $"Wall_{x}_{y}";
                    }
                }
                
                // 타일 100개마다 프레임 양보
                if ((x * dungeon.Height + y) % 100 == 0)
                {
                    yield return null;
                }
            }
        }
        
        // 특수 방 표시 (입구, 출구, 보스 등)
        foreach (var room in dungeon.Rooms)
        {
            if (room.Type != Room.RoomType.Normal)
            {
                Vector3 center = new Vector3(
                    room.GetCenter().x,
                    0.1f,
                    room.GetCenter().y
                );
                
                switch (room.Type)
                {
                    case Room.RoomType.Entrance:
                        Instantiate(stairsPrefab, center, Quaternion.identity, floorParent);
                        break;
                    case Room.RoomType.Exit:
                        Instantiate(stairsPrefab, center, Quaternion.Euler(0, 180, 0), floorParent);
                        break;
                    // 기타 특수 방 타입 처리...
                }
            }
        }
        
        Debug.Log($"던전 생성 완료: {dungeon.Width}x{dungeon.Height}, 방 {dungeon.Rooms.Count}개, 복도 {dungeon.Corridors.Count}개");
    }
    
    private void ClearDungeon()
    {
        // 기존 던전 오브젝트 제거
        foreach (Transform child in floorParent)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Transform child in wallParent)
        {
            Destroy(child.gameObject);
        }
    }
}

// 던전 생성 설정 ScriptableObject
[CreateAssetMenu(fileName = "DungeonGenerationSettings", menuName = "Dungeon/Generation Settings")]
public class DungeonGenerationSettings : ScriptableObject
{
    [Header("Dungeon Size")]
    [Range(20, 200)]
    public int Width = 100;
    
    [Range(20, 200)]
    public int Height = 100;
    
    [Header("BSP Settings")]
    [Range(3, 20)]
    public int MinRoomSize = 10;
    
    [Range(2, 10)]
    public int MaxDepth = 5;
    
    [Range(0f, 0.5f)]
    public float SplitRandomness = 0.3f;
    
    [Header("Room Settings")]
    [Range(0, 5)]
    public int RoomPadding = 2;
    
    [Header("Corridor Settings")]
    [Range(1, 5)]
    public int CorridorWidth = 2;
    
    [Header("Special Rooms")]
    public bool GenerateSpecialRooms = true;
    public bool ForceEntranceAndExit = true;
    
    [Range(0f, 1f)]
    public float SpecialRoomChance = 0.2f;
}
```