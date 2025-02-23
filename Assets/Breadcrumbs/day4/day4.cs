using System;
using UnityEngine;

public class day4 : MonoBehaviour {
    private day3 _day3;

    public GameObject player;

    private void Awake() {
        _day3 = GetComponent<day3>();
        _day3.OnGenerateDone += (position) => {
            // :P
            var groundId = LayerMask.NameToLayer("Ground");
            SetLayerRecursively(_day3.RoomRoot, groundId);
            SetLayerRecursively(_day3.WayRoot, groundId);
            player.transform.position = position;
        };
    }

    private void Start() {
        // note: 에디터에서는... 안되네..
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private static void SetLayerRecursively(Transform root, int layerId) {
        var children = root.GetComponentsInChildren<Transform>();
        foreach (var node in children)
            node.gameObject.layer = layerId;
    }
}
