using UnityEngine;

public class ColorSelector : MonoBehaviour
{
    Transform innerObject;
    Transform deepObject;
    Vector3 startPosition;
    Vector3 targetPosition;
    void Start()
    {
        innerObject = transform.Find("Inner");
        deepObject = innerObject.Find("Deep");
        startPosition = innerObject.localPosition;
        targetPosition = new Vector3((startPosition.x - 2) * 3 + 2, 0, 0);
    }

    public void Expand(float interpolationFactor) {
        innerObject.localPosition = Vector3.Lerp(startPosition, targetPosition, interpolationFactor);
    }
}
