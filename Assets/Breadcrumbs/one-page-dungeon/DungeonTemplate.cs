using System;
using Breadcrumbs.one_page_dungeon;
using UnityEngine;

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
        RightBottom
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
                Debug.Log("rotunda");
                const float step = 360f / 16f * Mathf.Deg2Rad;
                var r = 0f;
                var cx = origin.x + center.x;
                var cy = origin.y + center.y;
                for (var i = 0; i < 16; ++i, r += step) {
                    go = InstantiateBlock(BlockType.Bottom);
                    go.transform.localPosition = new Vector3(cx + Mathf.Sin(r) * center.x, 0, cy + Mathf.Cos(r) * center.y);
                    go.transform.localRotation = Quaternion.Euler(0, 360f / 16f * i + 180f, 0);
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
}