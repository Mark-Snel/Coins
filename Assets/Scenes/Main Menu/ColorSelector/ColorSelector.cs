using UnityEngine;
using UnityEngine.InputSystem;

public class ColorSelector : MonoBehaviour
{
    private MenuCoin coin;
    private ObjectDecorator coinDecorator;
    private ObjectDecorator decorator;
    Transform innerObject;
    Transform deepObject;
    Vector3 startPosition;
    Vector3 targetPosition;
    private bool selected = false;
    private Vector3 defaultScale;
    private Vector3 selectedScale = new Vector3(1.5f, 1.5f, 1);
    private float scaleSmoothFactor = 25f;
    private InputAction attackAction;
    void Start()
    {
        attackAction = InputSystem.actions.FindAction("Attack");
        Transform coinObject = GameObject.Find("Coin").transform;
        coinDecorator = coinObject.GetComponent<ObjectDecorator>();
        coin = coinObject.GetComponent<MenuCoin>();
        innerObject = transform.Find("Inner");
        deepObject = innerObject.Find("Deep");
        decorator = deepObject.GetComponent<ObjectDecorator>();
        startPosition = innerObject.localPosition;
        targetPosition = new Vector3((startPosition.x - 2) * 3 + 2, 0, 0);
        defaultScale = deepObject.localScale;
    }

    public void Expand(float interpolationFactor) {
        if (decorator.Color == ColorManager.GetColor() && interpolationFactor <= 0.1f) {
            deepObject.Find("Secondary").GetComponent<SpriteRenderer>().color = Color.white;
        }
        innerObject.localPosition = Vector3.Lerp(startPosition, targetPosition, interpolationFactor);
        defaultScale = new Vector3(Mathf.Min(interpolationFactor, 1), 1, 1);
    }

    public void MouseEnter() {
        selected = true;
        coinDecorator.Color = decorator.Color;
    }
    public void MouseExit() {
        selected = false;
    }
    void Update() {
        Vector3 scale = Vector3.Lerp(deepObject.localScale, selected ? selectedScale : defaultScale, 1 - Mathf.Exp(-scaleSmoothFactor * Time.deltaTime));
        deepObject.localScale = scale;
        if (attackAction.WasCompletedThisFrame() && selected) {
            coin.SelectColor();
        }
    }
}
