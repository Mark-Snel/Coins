using UnityEngine;
using static ColorManager;

public class ObjectDecorator : MonoBehaviour
{
    public bool dynamic;
    [SerializeField] private int color;
    private float offset;
    private SpriteRenderer primary;
    private SpriteRenderer secondary;
    private SpriteRenderer shadow;
    private Transform shadowTransform;

    void Start() {
        primary = transform.Find("Primary")?.GetComponent<SpriteRenderer>();
        secondary = transform.Find("Secondary")?.GetComponent<SpriteRenderer>();
        shadow = transform.Find("Shadow")?.GetComponent<SpriteRenderer>();
        shadowTransform = transform.Find("Shadow");

        if (primary == null) Debug.LogWarning("Primary SpriteRenderer not found!");
        if (secondary == null) Debug.LogWarning("Secondary SpriteRenderer not found!");
        if (shadow == null) Debug.LogWarning("Shadow SpriteRenderer not found!");
        if (shadowTransform == null) Debug.LogWarning("Shadow Transform not found!");
        float offsetX = shadowTransform.position.x - transform.position.x;
        float offsetY = shadowTransform.position.y - transform.position.y;
        offset = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);
        updateColor();
    }

    void Update() {
        if (dynamic) {
            shadowTransform.position = new Vector3(transform.position.x + offset, transform.position.y, shadowTransform.position.z);
        }
    }

    void updateColor() {
        Debug.Log("a");
        ColorSet colorSet = GetColor(color);
        primary.color = colorSet.Primary;
        secondary.color = colorSet.Secondary;
        shadow.color = colorSet.Secondary;
    }

    private void OnValidate() {
        if (primary) {
            updateColor();
        }
    }
}
