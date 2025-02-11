using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public enum KeyState
    {
        Released = -1, // Key was just released this tick
        None = 0,   // Key is not pressed
        Held = 1,   // Key is currently being held
        Pressed = 2, // Key was just pressed this tick
    }
    public enum Action
    {
        Jump,
        Left,
        Right,
        Primary
    }

    public static InputManager Inputs { get; private set; }

    private Dictionary<Action, List<KeyCode>> keyBindings = new Dictionary<Action, List<KeyCode>>();
    private Dictionary<KeyCode, KeyState> keyStates = new Dictionary<KeyCode, KeyState>();

    void Awake()
    {
        if (Inputs == null)
            Inputs = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // Default keybindings
        BindKey(Action.Jump, KeyCode.Space);
        BindKey(Action.Jump, KeyCode.W);
        BindKey(Action.Jump, KeyCode.UpArrow);
        BindKey(Action.Left, KeyCode.A);
        BindKey(Action.Left, KeyCode.LeftArrow);
        BindKey(Action.Right, KeyCode.D);
        BindKey(Action.Right, KeyCode.RightArrow);
        BindKey(Action.Primary, KeyCode.Mouse0);
    }

    void FixedUpdate()
    {
        foreach (KeyCode key in new List<KeyCode>(keyStates.Keys))
        {
            if (Input.GetKeyDown(key))
                keyStates[key] = KeyState.Pressed;
            else if (Input.GetKey(key))
                keyStates[key] = KeyState.Held;
            else if (Input.GetKeyUp(key))
                keyStates[key] = KeyState.Released;
            else
                keyStates[key] = KeyState.None;
        }
    }

    /// <summary>
    /// Returns the most "interesting" key state for the given action.
    /// </summary>
    public KeyState GetActionState(Action action)
    {
        if (!keyBindings.ContainsKey(action))
            return KeyState.None;

        KeyState bestState = KeyState.None;

        foreach (KeyCode key in keyBindings[action])
        {
            KeyState state = keyStates.ContainsKey(key) ? keyStates[key] : KeyState.None;

            if (bestState == KeyState.None ||
                (bestState == KeyState.Held && (state == KeyState.Pressed || state == KeyState.Released)) ||
                state == KeyState.Pressed || state == KeyState.Released)
            {
                bestState = state;
            }
        }

        return bestState;
    }

    /// <summary>
    /// Binds a key to an action.
    /// </summary>
    public void BindKey(Action action, KeyCode key)
    {
        if (!keyBindings.ContainsKey(action))
            keyBindings[action] = new List<KeyCode>();

        if (!keyBindings[action].Contains(key))
            keyBindings[action].Add(key);

        // Initialize key state
        if (!keyStates.ContainsKey(key))
            keyStates[key] = KeyState.None;
    }
}
