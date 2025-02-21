using UnityEngine;

public class SDFMeshTest : MonoBehaviour
{
    void Start()
    {
        var generator = gameObject.AddComponent<SDFMeshGenerator>();
        var mesh = generator.GenerateMesh(
            center: Vector3.zero,
            size: new Vector3(5f, 5f, 5f),
            cellSize: 0.1f
        );

        // 메시 적용
        var meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
    }
}
