using UnityEngine;
using TMPro;
using static ColorManager;

public class TextDecorator : MonoBehaviour
{
    [SerializeField] private int color = 11;
    private TextMeshProUGUI primary;
    private TextMeshProUGUI secondary;

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
        primary = transform.Find("Primary")?.GetComponent<TextMeshProUGUI>();
        secondary = transform.Find("Secondary")?.GetComponent<TextMeshProUGUI>();
        if (primary == null) Debug.LogWarning("Primary Text not found!");
        if (secondary == null) Debug.LogWarning("Secondary Text not found!");
        updateColor();
    }


    void updateColor() {
        ColorSet colorSet = GetColor(color);
        primary.color = colorSet.Primary;
        secondary.color = colorSet.Secondary;
    }

    void OnValidate() {
        if (primary) {
            updateColor();
        }
    }

    public void ChangeText(string newText) {
        primary.text = newText;
        secondary.text = newText;
    }
}
