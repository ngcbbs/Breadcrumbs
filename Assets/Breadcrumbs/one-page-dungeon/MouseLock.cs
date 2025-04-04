using UnityEngine;

public class MouseLock : MonoBehaviour {
    private void OnEnable() {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void OnDisable() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
