using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHandling
{
    public enum KeyState {
        Released,
        None,
        Held,
        Pressed
    }
    public static void UpdateKeyState(InputAction action, ref KeyState state) {
        if (action.WasCompletedThisFrame()) {
            state = KeyState.Released;
        } else if (action.WasPerformedThisFrame()) {
            state = KeyState.Pressed;
        } else if (action.ReadValue<float>() == 1 && state != KeyState.Pressed) {
            state = KeyState.Held;
        }
    }
    public static KeyState GetKeyState(InputAction action) {
        if (action.WasCompletedThisFrame()) {
            return KeyState.Released;
        } else if (action.WasPerformedThisFrame()) {
            return KeyState.Pressed;
        } else if (action.ReadValue<float>() == 1) {
            return KeyState.Held;
        }
        return KeyState.None;
    }
    public static float GetRotationToCursor(Vector3 fromPosition, InputAction aimAction, Camera cam) {
        //Read the Aim position (which should be a Vector2 screen position)
        Vector2 screenPosition = aimAction.ReadValue<Vector2>();

        //Convert screen position to world space
        Vector3 worldPosition = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, cam.nearClipPlane));

        //Calculate direction from player to cursor
        Vector3 direction = (worldPosition - fromPosition).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle;
    }
}
