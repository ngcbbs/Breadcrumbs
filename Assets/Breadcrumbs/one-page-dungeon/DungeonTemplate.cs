using System;
using System.Collections.Generic;
using Breadcrumbs.one_page_dungeon;
using UnityEngine;

// note: 아이디어 노트
// 1) edge 기준으로 필요한 타일을 생성하도록 하는게 좋을까?
// 2) left, right, top, bottom, 보다는 동서남북으로 하면 편할까? north, south, east, west?

public class DungeonTemplate : MonoBehaviour {
    // for test
    public GameObject cube;
    [Header("Set")] 
    // side
    public GameObject wallTop;
    public GameObject wallBottom;
    public GameObject wallLeft;
    public GameObject wallRight;
    // corner
    public GameObject wallLeftTop;
    public GameObject wallLeftBottom;
    public GameObject wallRightTop;
    public GameObject wallRightBottom;

    public GameObject cornerLeftTop;
    public GameObject cornerLeftBottom;
    public GameObject cornerRightTop;
    public GameObject cornerRightBottom;

    public GameObject doorLeft;
    public GameObject doorRight;
    public GameObject doorTop;
    public GameObject doorBottom;

    private const float kTileUnits = 4f;

    private enum BlockType {
        Cube,
        Top,
        Bottom,
        Left,
        Right,
        LeftTop,
        LeftBottom,
        RightTop,
        RightBottom,
        CornerLeftTop,
        CornerLeftBottom,
        CornerRightTop,
        CornerRightBottom,
        DoorLeft,
        DoorRight,
        DoorTop,
        DoorBottom
    }
    
    private GameObject InstantiateBlock(BlockType type = BlockType.Cube) {
        return type switch {
            BlockType.Cube => Instantiate(cube, _root),
            BlockType.Top => Instantiate(wallTop, _root),
            BlockType.Bottom => Instantiate(wallBottom, _root),
            BlockType.Left => Instantiate(wallLeft, _root),
            BlockType.Right => Instantiate(wallRight, _root),
            BlockType.LeftTop => Instantiate(wallLeftTop, _root),
            BlockType.LeftBottom => Instantiate(wallLeftBottom, _root),
            BlockType.RightTop => Instantiate(wallRightTop, _root),
            BlockType.RightBottom => Instantiate(wallRightBottom, _root),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private Transform _root;
    
    public void InstantiateRooms(OnePageDungeonData data, Transform root = null) {
        if (data == null) {
            Debug.Log("data is null");
            return;
        }

        _root = root;

        foreach(var rect in data.Rects) {
            var origin = new Vector3(rect.X, rect.Y) * kTileUnits;
            var size = new Vector3(rect.W, rect.H) * kTileUnits;
            var center = size * 0.5f;

            GameObject go;
            if (rect.Rotunda == true) {
                const float segments = 16f; // todo: 방 사이즈에 따라서 필요한 세그먼트 수를 계산 할 수 있어야함. (통로와 연결될 Segment 방향도 신경써야함)
                const float step = 360f / segments * Mathf.Deg2Rad;
                var r = 0f;
                var cx = origin.x + center.x;
                var cy = origin.y + center.y;
                for (var i = 0; i < 16; ++i, r += step) {
                    go = InstantiateBlock(BlockType.Bottom);
                    go.transform.localPosition = new Vector3(cx + Mathf.Sin(r) * center.x, 0, cy + Mathf.Cos(r) * center.y);
                    go.transform.localRotation = Quaternion.Euler(0, 360f / segments * i + 180f, 0);
                }

                continue; // :)
            }

            if (rect.W == 1 && rect.H == 1) {
                continue;
            }

            // side
            var sx = origin.x + 2f;
            for (var i = 0; i < rect.W; ++i) {
                go = InstantiateBlock(BlockType.Bottom);
                go.transform.localPosition = new Vector3(sx, 0, origin.y);

                go = InstantiateBlock(BlockType.Top);
                go.transform.localPosition = new Vector3(sx, 0, origin.y + size.y);

                sx += kTileUnits;
            }

            var sy = origin.y + 2f;
            for (var i = 0; i < rect.H; ++i) {
                go = InstantiateBlock(BlockType.Left);
                go.transform.localPosition = new Vector3(origin.x, 0, sy);

                go = InstantiateBlock(BlockType.Right);
                go.transform.localPosition = new Vector3(origin.x + size.x, 0, sy);

                sy += kTileUnits;
            }

            // corner
            go = InstantiateBlock(BlockType.LeftBottom);
            go.transform.localPosition = new Vector3(origin.x, 0, origin.y);

            go = InstantiateBlock(BlockType.LeftTop);
            go.transform.localPosition = new Vector3(origin.x, 0, origin.y + size.y);

            go = InstantiateBlock(BlockType.RightBottom);
            go.transform.localPosition = new Vector3(origin.x + size.x, 0, origin.y);
            
            go = InstantiateBlock(BlockType.RightTop);
            go.transform.localPosition = new Vector3(origin.x + size.x, 0, origin.y + size.y);
        }
    }

    private static bool IsInsideEllipse(Vector2 point, Vector2 ellipseCenter, Vector2 ellipseSize) {
        if (ellipseSize.x <= 0 || ellipseSize.y <= 0) {
            Debug.LogError("타원의 가로 또는 세로 크기는 0보다 커야 합니다.");
            return false;
        }

        // 직사각형 내부에 꽉 찬 타원의 반지름 계산
        float horizontalRadius = ellipseSize.x / 2f;
        float verticalRadius = ellipseSize.y / 2f;

        // 타원 방정식: ((x - centerX) / horizontalRadius)^2 + ((y - centerY) / verticalRadius)^2 <= 1
        // 여기서 centerX, centerY는 타원의 중심이며, 직사각형의 중심과 동일합니다.
        double normalizedX = (point.x - ellipseCenter.x) / horizontalRadius;
        double normalizedY = (point.y - ellipseCenter.y) / verticalRadius;

        return (normalizedX * normalizedX + normalizedY * normalizedY) <= 1;
    }

    public List<Vector2Int> GetEdgeTiles(OnePageDungeonData data) {
        var result = new List<Vector2Int>();

        if (data == null) {
            Debug.Log("data is null");
            return result;
        }

        foreach (var rect in data.Rects) {
            var origin = new Vector2Int((int)rect.X, (int)rect.Y);

            for (var y = 0; y < rect.H; ++y) {
                for (var x = 0; x < rect.W; ++x) {
                    if (rect.Rotunda == true) {
                        // rotunda
                        var cx = rect.X + rect.W * 0.5f;
                        var cy = rect.Y + rect.H * 0.5f;
                        if (!IsInsideEllipse(new Vector2(origin.x + x, origin.y + y), new Vector2(cx, cy),
                                new Vector2(rect.W, rect.H)))
                            continue;
                    }

                    AddPosition(origin, x, y, result);
                }
            }
        }

        return result;
    }

    private static void AddPosition(Vector2Int origin, int x, int y, List<Vector2Int> result) {
        var position = origin + new Vector2Int(x, y);
        if (result.Contains(position))
            return;    
        result.Add(position);
    }
}