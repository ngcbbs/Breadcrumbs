using UnityEngine;
using System.Collections.Generic;

public class SDFMeshGenerator : MonoBehaviour
{
    private class VertexKey
    {
        public Vector3 position;
        private readonly float precision = 1000f; // 소수점 3자리까지 비교

        public VertexKey(Vector3 position)
        {
            this.position = position;
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexKey other)
            {
                return Mathf.Round(position.x * precision) == Mathf.Round(other.position.x * precision) &&
                       Mathf.Round(position.y * precision) == Mathf.Round(other.position.y * precision) &&
                       Mathf.Round(position.z * precision) == Mathf.Round(other.position.z * precision);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Mathf.RoundToInt(position.x * precision).GetHashCode() ^
                   (Mathf.RoundToInt(position.y * precision).GetHashCode() << 2) ^
                   (Mathf.RoundToInt(position.z * precision).GetHashCode() >> 2);
        }
    }

    private Dictionary<VertexKey, int> vertexIndices = new Dictionary<VertexKey, int>();
    
    private int GetOrAddVertex(Vector3 vertex, List<Vector3> vertices)
    {
        var key = new VertexKey(vertex);
        if (vertexIndices.TryGetValue(key, out int index))
        {
            return index;
        }
        
        index = vertices.Count;
        vertices.Add(vertex);
        vertexIndices.Add(key, index);
        return index;
    }

    private void ProcessCellWithTable(VoxelGrid grid, int x, int y, int z, 
                                    List<Vector3> vertices, List<int> triangles)
    {
        float[] cornerValues = new float[8];
        Vector3[] cornerPositions = new Vector3[8];
        int cubeIndex = 0;

        // 코너 값과 위치 수집
        for (int i = 0; i < 8; i++)
        {
            int dx = i & 1;
            int dy = (i & 2) >> 1;
            int dz = (i & 4) >> 2;

            cornerValues[i] = grid.voxels[x + dx, y + dy, z + dz].value;
            cornerPositions[i] = grid.voxels[x + dx, y + dy, z + dz].position;

            if (cornerValues[i] < 0)
            {
                cubeIndex |= 1 << i;
            }
        }

        if (cubeIndex == 0 || cubeIndex == 255) return; // 셀이 완전히 안쪽이나 바깥쪽인 경우 스킵

        // 엣지 교차점 계산
        Vector3[] edgeVertices = new Vector3[12];
        bool[] edgeComputed = new bool[12];

        var length = MarchingCubesTables.TriangleTable.Length;

        // 실제로 필요한 엣지만 계산
        for (int i = 0; MarchingCubesTables.TriangleTable[(cubeIndex * 16 + i) % length] != -1; i++)
        {
            int edgeIndex = MarchingCubesTables.TriangleTable[(cubeIndex * 16 + i) % length];
            if (!edgeComputed[edgeIndex])
            {
                int v1 = MarchingCubesTables.EdgeVertexIndices[edgeIndex, 0];
                int v2 = MarchingCubesTables.EdgeVertexIndices[edgeIndex, 1];

                edgeVertices[edgeIndex] = InterpolateVertex(
                    cornerPositions[v1],
                    cornerPositions[v2],
                    cornerValues[v1],
                    cornerValues[v2]
                );
                edgeComputed[edgeIndex] = true;
            }
        }

        // 삼각형 생성
        for (int i = 0; MarchingCubesTables.TriangleTable[(cubeIndex * 16 + i) % length] != -1; i += 3)
        {
            int a = MarchingCubesTables.TriangleTable[(cubeIndex * 16 + i) % length];
            int b = MarchingCubesTables.TriangleTable[(cubeIndex * 16 + i + 1) % length];
            int c = MarchingCubesTables.TriangleTable[(cubeIndex * 16 + i + 2) % length];

            // 정점 인덱스 얻기 (중복 제거)
            int indexA = GetOrAddVertex(edgeVertices[a], vertices);
            int indexB = GetOrAddVertex(edgeVertices[b], vertices);
            int indexC = GetOrAddVertex(edgeVertices[c], vertices);

            // 삼각형이 올바른 방향을 가리키도록 법선 확인
            Vector3 normal = Vector3.Cross(
                vertices[indexB] - vertices[indexA],
                vertices[indexC] - vertices[indexA]
            );

            // SDF의 그래디언트와 법선이 같은 방향을 가리키도록 함
            Vector3 center = (vertices[indexA] + vertices[indexB] + vertices[indexC]) / 3f;
            Vector3 gradient = CalculateGradient(center);
            
            if (Vector3.Dot(normal, gradient) > 0)
            {
                // 법선 방향이 반대면 인덱스 순서를 뒤집음
                triangles.Add(indexA);
                triangles.Add(indexC);
                triangles.Add(indexB);
            }
            else
            {
                triangles.Add(indexA);
                triangles.Add(indexB);
                triangles.Add(indexC);
            }
        }
    }

    private Vector3 CalculateGradient(Vector3 p)
    {
        float epsilon = 0.001f;
        return new Vector3(
            EvaluateSDF(p + new Vector3(epsilon, 0, 0)) - EvaluateSDF(p - new Vector3(epsilon, 0, 0)),
            EvaluateSDF(p + new Vector3(0, epsilon, 0)) - EvaluateSDF(p - new Vector3(0, epsilon, 0)),
            EvaluateSDF(p + new Vector3(0, 0, epsilon)) - EvaluateSDF(p - new Vector3(0, 0, epsilon))
        ).normalized;
    }

    private Vector3 InterpolateVertex(Vector3 v1, Vector3 v2, float val1, float val2)
    {
        float t = (0 - val1) / (val2 - val1);
        return Vector3.Lerp(v1, v2, t);
    }

    // GenerateMesh 메서드 수정
    public Mesh GenerateMesh(Vector3 center, Vector3 size, float cellSize)
    {
        VoxelGrid grid = CreateVoxelGrid(center, size, cellSize);
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        vertexIndices.Clear(); // 새 메시 생성 전에 캐시 초기화

        for (int x = 0; x < grid.width - 1; x++)
        {
            for (int y = 0; y < grid.height - 1; y++)
            {
                for (int z = 0; z < grid.depth - 1; z++)
                {
                    ProcessCellWithTable(grid, x, y, z, vertices, triangles);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
    
    public struct Voxel
    {
        public Vector3 position;
        public float value;
    }

    public class VoxelGrid
    {
        public Voxel[,,] voxels;
        public int width, height, depth;
        public float cellSize;

        public VoxelGrid(int width, int height, int depth, float cellSize)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.cellSize = cellSize;
            voxels = new Voxel[width, height, depth];
        }
    }

    // SDF 함수들
    private float SphereSDF(Vector3 p, float radius)
    {
        return p.magnitude - radius;
    }

    private float BoxSDF(Vector3 p, Vector3 size)
    {
        Vector3 d = new Vector3(
            Mathf.Abs(p.x) - size.x * 0.5f,
            Mathf.Abs(p.y) - size.y * 0.5f,
            Mathf.Abs(p.z) - size.z * 0.5f
        );
        
        return Mathf.Min(Mathf.Max(d.x, Mathf.Max(d.y, d.z)), 0.0f) + 
               new Vector3(Mathf.Max(d.x, 0.0f), Mathf.Max(d.y, 0.0f), Mathf.Max(d.z, 0.0f)).magnitude;
    }

    // 복합 SDF 연산
    private float UnionSDF(float d1, float d2)
    {
        return Mathf.Min(d1, d2);
    }

    private float SubtractSDF(float d1, float d2)
    {
        return Mathf.Max(d1, -d2);
    }

    private float IntersectSDF(float d1, float d2)
    {
        return Mathf.Max(d1, d2);
    }

    // 복셀 그리드 생성
    private VoxelGrid CreateVoxelGrid(Vector3 center, Vector3 size, float cellSize)
    {
        int width = Mathf.CeilToInt(size.x / cellSize);
        int height = Mathf.CeilToInt(size.y / cellSize);
        int depth = Mathf.CeilToInt(size.z / cellSize);

        VoxelGrid grid = new VoxelGrid(width, height, depth, cellSize);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    Vector3 pos = new Vector3(
                        center.x - size.x * 0.5f + x * cellSize,
                        center.y - size.y * 0.5f + y * cellSize,
                        center.z - size.z * 0.5f + z * cellSize
                    );

                    grid.voxels[x, y, z] = new Voxel
                    {
                        position = pos,
                        value = EvaluateSDF(pos) // SDF 값 계산
                    };
                }
            }
        }

        return grid;
    }

    // SDF 값 계산 (예시: 구체와 상자의 결합)
    private float EvaluateSDF(Vector3 point)
    {
        float sphereDist = SphereSDF(point - new Vector3(0, 0, 0), 1f);
        float boxDist = BoxSDF(point - new Vector3(1.0f, 0, 0), new Vector3(1f, 1f, 1f));
        return UnionSDF(sphereDist, boxDist);
    }

    // 단일 셀 처리 (error)
    private void ProcessCell(VoxelGrid grid, int x, int y, int z, List<Vector3> vertices, List<int> triangles)
    {
        float[] cornerValues = new float[8];
        Vector3[] cornerPositions = new Vector3[8];

        // 코너 값들 수집
        for (int i = 0; i < 8; i++)
        {
            int dx = i & 1;
            int dy = (i & 2) >> 1;
            int dz = (i & 4) >> 2;

            cornerValues[i] = grid.voxels[x + dx, y + dy, z + dz].value;
            cornerPositions[i] = grid.voxels[x + dx, y + dy, z + dz].position;
        }

        // 마칭 큐브 룩업 테이블을 사용하여 삼각형 생성
        int cubeIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cornerValues[i] < 0)
            {
                cubeIndex |= 1 << i;
            }
        }

        // 여기서 실제 삼각형을 생성
        // (간단한 구현을 위해 일부 생략됨 - 실제 구현시 마칭 큐브 룩업 테이블 필요)
        if (cubeIndex != 0 && cubeIndex != 255)
        {
            Vector3 vertex = Vector3.zero;
            for (int i = 0; i < 8; i++)
            {
                vertex += cornerPositions[i];
            }
            vertex /= 8;

            int vertexIndex = vertices.Count;
            vertices.Add(vertex);

            // 임시로 단순한 삼각형 추가
            if (vertexIndex > 2)
            {
                triangles.Add(vertexIndex - 2);
                triangles.Add(vertexIndex - 1);
                triangles.Add(vertexIndex);
            }
        }
    }
    
    public static class MarchingCubesTables
    {
        // 엣지 정점 인덱스 쌍
        public static readonly int[,] EdgeVertexIndices = new int[12, 2]
        {
            {0, 1}, {1, 2}, {2, 3}, {3, 0},  // 아래쪽 엣지
            {4, 5}, {5, 6}, {6, 7}, {7, 4},  // 위쪽 엣지
            {0, 4}, {1, 5}, {2, 6}, {3, 7}   // 수직 엣지
        };

        // 정점 상대 위치 (큐브 내에서)
        public static readonly Vector3[] VertexOffsets = new Vector3[]
        {
            new Vector3(0, 0, 0), // 0
            new Vector3(1, 0, 0), // 1
            new Vector3(1, 0, 1), // 2
            new Vector3(0, 0, 1), // 3
            new Vector3(0, 1, 0), // 4
            new Vector3(1, 1, 0), // 5
            new Vector3(1, 1, 1), // 6
            new Vector3(0, 1, 1)  // 7
        };

        // 각 케이스(256가지)에 대한 엣지 인덱스 리스트
        // -1은 리스트의 끝을 표시
        public static readonly int[] TriangleTable = new int[]
{
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1,
    3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1,
    3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1,
    3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1,
    9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1,
    1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1,
    9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1,
    2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1,
    8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1,
    9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1,
    4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1,
    3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1,
    1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1,
    4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1,
    4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1,
    9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1,
    1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1,
    5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1,
    2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1,
    9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1,
    0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1,
    2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1,
    10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1,
    4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1,
    5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1,
    5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1,
    9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1,
    0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1,
    1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1,
    10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1,
    8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1,
    2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1,
    7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1,
    9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1,
    2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1,
    11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1,
    9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1,
    5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1,
    11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1,
    11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1,
    1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1,
    9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1,
    5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1,
    2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1,
    0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1,
    5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1,
    6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1,
    0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1,
    3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1,
    6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1,
    5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    // 아 짤렸네.
        };

        // 엣지 보간을 위한 헬퍼 함수
        public static Vector3 InterpolateVertex(Vector3 v1, Vector3 v2, float val1, float val2)
        {
            float t = (0 - val1) / (val2 - val1);
            return Vector3.Lerp(v1, v2, t);
        }
    }
}