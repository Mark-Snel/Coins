using UnityEngine;

public class ColorSelectorCollider : MonoBehaviour
{
    private ColorSelector selector;
    void Start()
    {
        selector = GetComponentInParent<ColorSelector>();
    }
    void OnMouseEnter() {
        selector.MouseEnter();
    }

    void OnMouseExit() {
        selector.MouseExit();
    }
}
