using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Button : MonoBehaviour
{
    [SerializeField] private bool clickEffect = true;
    [SerializeField] private string destination;
    private Vector3 defaultScale;
    private Vector3 selectedScale;
    [SerializeField] private Vector2 selectedScaleAdditive = new Vector2(0.5f, 0.5f);
    [SerializeField] private float scaleSmoothFactor = 25f;
    public bool selected = false;
    private InputAction attackAction;

    // New callbacks that can be set externally.
    public Action OnClick;
    public Func<bool> CanClickEffect;

    void Start() {
        defaultScale = transform.localScale;
        selectedScale = new Vector3(defaultScale.x + selectedScaleAdditive.x, defaultScale.y + selectedScaleAdditive.y, defaultScale.z);
        attackAction = InputSystem.actions.FindAction("Attack");
    }

    void OnMouseEnter() {
        selected = true;
    }

    void OnMouseExit() {
        selected = false;
    }

    void Update() {
        if (selected && attackAction.WasCompletedThisFrame()) {
            // If a callback has been set, call it; otherwise do the default SceneTransition.
            if (OnClick != null)
                OnClick.Invoke();
            else
                SceneTransition.LoadScene(destination);
        } else if (selected && attackAction.ReadValue<float>() > 0 && clickEffect && (CanClickEffect == null || CanClickEffect())) {
            // Only run click effect if either no delegate is set or the delegate returns true.
            transform.localScale = defaultScale;
        } else {
            transform.localScale = Vector3.Lerp(transform.localScale, selected ? selectedScale : defaultScale, 1 - Mathf.Exp(-scaleSmoothFactor * Time.deltaTime));
        }
    }

    void OnValidate() {
        selectedScale = new Vector3(defaultScale.x + selectedScaleAdditive.x, defaultScale.y + selectedScaleAdditive.y, defaultScale.z);
    }
}
