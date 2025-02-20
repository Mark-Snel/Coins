using UnityEngine;
using static ColorManager;

public class ObjectDecorator : MonoBehaviour
{
    public bool dynamic;
    [SerializeField] private int color;
    private bool started = false;
    private float offset;
    private SpriteRenderer primary;
    private SpriteRenderer extra;
    private SpriteRenderer secondary;
    private SpriteRenderer shadow;
    private Transform shadowTransform;
    public int Color {
        get {return color;}
        set {
            if (color != value) {
                color = value;
                updateColor();
            }
        }
    }

    void Start() {
        primary = transform.Find("Primary")?.GetComponent<SpriteRenderer>();
        extra = transform.Find("Extra")?.GetComponent<SpriteRenderer>();
        secondary = transform.Find("Secondary")?.GetComponent<SpriteRenderer>();
        shadowTransform = transform.Find("Shadow");
        if (shadowTransform != null) shadow = shadowTransform.GetComponent<SpriteRenderer>();

        if (!primary) Debug.LogWarning("Primary SpriteRenderer not found!");
        if (shadowTransform != null) {
            float offsetX = shadowTransform.position.x - transform.position.x;
            float offsetY = shadowTransform.position.y - transform.position.y;
            offset = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);
        }
        started = true;
        updateColor();
    }

    void LateUpdate() {
        if (dynamic && shadowTransform != null) {
            shadowTransform.position = new Vector3(transform.position.x + offset, transform.position.y, shadowTransform.position.z);
        }
    }

    void updateColor() {
        if (started) {
            ColorSet colorSet = GetColor(color);
            primary.color = colorSet.Primary;
            primary.enabled = false;//I had to do this to fix some sort of weird bug where it just didnt display the primary sprite
            primary.enabled = true;
            if (secondary != null) secondary.color = colorSet.Secondary;
            if (shadow != null) shadow.color = colorSet.Secondary;
            if (extra != null) extra.color = colorSet.Primary;
        }
    }

    private void OnValidate() {
        if (primary) {
            updateColor();
        }
    }
}
