using UnityEngine;

public class Intro : MonoBehaviour//Dont mind me, just taking some old code again, teehee
{

    private float ticks = 0f;

    void Update()
    {
        float cameraHalfHeight = Camera.main.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;
        ticks += Time.deltaTime * 40;

        if (ticks < 37f)
        {
            float scale = Mathf.Max(-0.015f * (ticks - 25f) * (ticks - 65f) + 1f, 1f)/3;
            float rotation = 0.01507964473723f * ticks * (ticks - 70.710678118654752f);
            float x = (rotation + 6f * Mathf.PI) * 0.054f * (transform.localScale.x/2 + cameraHalfWidth);

            transform.localScale = new Vector3(scale, scale, transform.localScale.z);
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            transform.rotation = Quaternion.Euler(0f, 0f, -rotation * Mathf.Rad2Deg);
        }
        else if (ticks < 40f)
        {
            float scale = Mathf.Max(-0.015f * (ticks - 25f) * (ticks - 65f) + 1f, 1f)/3;
            float rotation = -6f * Mathf.PI;
            float x = 0f;

            transform.localScale = new Vector3(scale, scale, transform.localScale.z);
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            transform.rotation = Quaternion.Euler(0f, 0f, rotation * Mathf.Rad2Deg);
        }
        else if (ticks < 70f)
        {
            float scale = 2.192f;
            float rotation = -6f * Mathf.PI;
            float t = (ticks - 40f) / 30f;
            float x = -3.58f + 3.58f * (Mathf.Pow(1 - t, 3) + 3f * t * Mathf.Pow(1 - t, 2));

            transform.localScale = new Vector3(scale, scale, transform.localScale.z);
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            transform.rotation = Quaternion.Euler(0f, 0f, rotation * Mathf.Rad2Deg);
        }
    }
}
