using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class day6 : MonoBehaviour {
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TilemapRenderer tilemapRenderer;

    private List<bool> _tilemapLayers = new List<bool>();

    void Awake() {
        if (grid == null)
            grid = GetComponent<Grid>();
        if (tilemap == null)
            tilemap = GetComponentInChildren<Tilemap>();
        if (tilemapRenderer == null)
            tilemapRenderer = GetComponentInChildren<TilemapRenderer>();
        GetTilemapLayers();
        //tilemap.GetTilesBlock() // get tiles from bound.


    }

    void GetTilemapLayers() {
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;
        // 카메라 뷰포트를 월드 좌표로 변환
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // 화면에 보이는 영역의 경계 계산
        Bounds visibleBounds = new Bounds();
        visibleBounds.SetMinMax(bottomLeft, topRight);

        // Tilemap의 로컬 좌표계로 변환
        Matrix4x4 worldToLocal = tilemap.transform.worldToLocalMatrix;
        Vector3 localMin = worldToLocal.MultiplyPoint3x4(visibleBounds.min);
        Vector3 localMax = worldToLocal.MultiplyPoint3x4(visibleBounds.max);

        // 타일맵의 셀 좌표로 변환
        Vector3Int minCell = tilemap.WorldToCell(tilemap.transform.TransformPoint(localMin));
        Vector3Int maxCell = tilemap.WorldToCell(tilemap.transform.TransformPoint(localMax));

        // 보이는 영역의 모든 타일 순회
        for (int x = minCell.x; x <= maxCell.x; x++) {
            for (int y = minCell.y; y <= maxCell.y; y++) {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // 해당 위치에 타일이 있는지 확인
                TileBase tile = tilemap.GetTile(cellPosition);

                if (tile != null) {
                    // 타일 데이터 처리
                    Debug.Log($"화면에 보이는 타일: {tile.name}, 위치: {cellPosition}");
                }
            }
        }
    }

    Tile GetTileInfo(Vector3 screenPosition, Camera useCamera = null) {
        var mainCamera = (useCamera == null) ? Camera.main : useCamera;
        if (mainCamera == null)
            return null;
        var worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition = new Vector3(worldPosition.x, worldPosition.y, 0); // z - remove depth value.
        var cellIndex = tilemap.WorldToCell(worldPosition);
        var result = tilemap.GetTile<Tile>(cellIndex);
        if (result == null) {
            //Debug.Log($"sp: {screenPosition} wp: {worldPosition}, ci {cellIndex}");
            return null;
        }

        return result;
    }

    private void OnGUI() {
        int line = 0;

        var mousePosition = Event.current.mousePosition;
        GUI.Label(new Rect(10, 10 + line * 22, 200, 20), $"OnGUI: {mousePosition.x}, {mousePosition.y}");
        line++;

        if (tilemap != null) {
            var tileInfo = GetTileInfo(mousePosition);
            GUI.Label(new Rect(10, 10 + line * 22, 200, 20), $"TileInfo: {tileInfo}");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (tilemap == null ||
            (SceneView.currentDrawingSceneView != null && Camera.current != SceneView.currentDrawingSceneView.camera))
            return;
        try {
            var mousePosition = Event.current.mousePosition;
            var sceneViewCamera = Camera.current;
            var tileInfo = GetTileInfo(mousePosition, sceneViewCamera);
            Handles.Label(tilemap.transform.position,
                $"inSceneView {mousePosition.x}, {mousePosition.y} // TileInfo: {tileInfo}");
        }
        catch (Exception ex) {
            Debug.Log(ex.Message);
        }
    }
#endif
}
