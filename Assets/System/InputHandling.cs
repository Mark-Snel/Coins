using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHandling
{
    public enum KeyState
    {
        Released,
        None,
        Held,
        Pressed
    }
    public static void UpdateKeyState(InputAction action, ref KeyState state)
    {
        if (action.WasCompletedThisFrame()) {
            state = KeyState.Released;
        } else if (action.WasPerformedThisFrame()) {
            state = KeyState.Pressed;
        } else if (action.ReadValue<float>() == 1 && state != KeyState.Pressed) {
            state = KeyState.Held;
        }
    }
}
