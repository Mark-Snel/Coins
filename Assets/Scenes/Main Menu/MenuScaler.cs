using UnityEngine;

public class MenuScaler : MonoBehaviour
{
    public float initialWidth = 11;
    public float minWidth = 7;
    private Vector3 originalPosition;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        originalPosition = transform.position;
    }

    void Update()
    {
        float cameraHeight = cam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.aspect;
        float maxOffset = (initialWidth - minWidth) / 2;
        float offset = Mathf.Max(Mathf.Min(-(1 / maxOffset) * cameraWidth + (2 * initialWidth)/(initialWidth - minWidth), maxOffset), 0);
        transform.position = new Vector3(originalPosition.x - offset, originalPosition.y, originalPosition.z);
    }
}
