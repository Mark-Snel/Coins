using UnityEngine;
using static ColorManager;

public class ObjectDecorator : MonoBehaviour
{
    public bool dynamic;
    [SerializeField] private int color;
    private float offset;
    private SpriteRenderer primary;
    private SpriteRenderer extra;
    private SpriteRenderer secondary;
    private SpriteRenderer shadow;
    private Transform shadowTransform;

    void Start() {
        primary = transform.Find("Primary")?.GetComponent<SpriteRenderer>();
        extra = transform.Find("Extra")?.GetComponent<SpriteRenderer>();
        secondary = transform.Find("Secondary")?.GetComponent<SpriteRenderer>();
        shadowTransform = transform.Find("Shadow");
        if (shadowTransform != null) shadow = shadowTransform.GetComponent<SpriteRenderer>();

        if (primary == null) Debug.LogWarning("Primary SpriteRenderer not found!");
        if (shadow == null) Debug.LogWarning("Shadow SpriteRenderer not found!");
        float offsetX = shadowTransform.position.x - transform.position.x;
        float offsetY = shadowTransform.position.y - transform.position.y;
        offset = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);
        updateColor();
    }

    void Update() {
        if (dynamic && shadowTransform != null) {
            shadowTransform.position = new Vector3(transform.position.x + offset, transform.position.y, shadowTransform.position.z);
        }
    }

    void updateColor() {
        ColorSet colorSet = GetColor(color);
        primary.color = colorSet.Primary;
        if (secondary != null) secondary.color = colorSet.Secondary;
        if (shadow != null) shadow.color = colorSet.Secondary;
        if (extra != null) extra.color = colorSet.Primary;
    }

    private void OnValidate() {
        if (primary) {
            updateColor();
        }
    }
}
