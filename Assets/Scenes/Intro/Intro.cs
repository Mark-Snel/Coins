using UnityEngine;
using TMPro;

public class Intro : MonoBehaviour
{
    private SpriteRenderer sr;
    private TextMeshProUGUI topText, oinsText, bottomText, subText;
    private bool transitionStarted = false;
    private float ticks = 0f;
    private string[] introTexts = {
        "Many, many,", "The majesty of", "Unlimited", "Praise the", "The power of",
        "A lot of", "Dreams &", "Fortune favors the", "Whimsical", "Glorious",
        "Embrace the", "All powerful", "Majestic", "Limitless"
    };

    void Start()
    {
        sr = transform.Find("Circle").GetComponent<SpriteRenderer>();

        // Finding TextMeshProUGUI elements inside the Canvas
        GameObject canvas = GameObject.Find("Canvas");
        topText = canvas.transform.Find("Top").GetComponent<TextMeshProUGUI>();
        oinsText = canvas.transform.Find("Oins").GetComponent<TextMeshProUGUI>();
        bottomText = canvas.transform.Find("Bottom").GetComponent<TextMeshProUGUI>();
        subText = canvas.transform.Find("Sub").GetComponent<TextMeshProUGUI>();
        topText.text = introTexts[Random.Range(0, introTexts.Length)];

        sr.enabled = true;
    }

    void Update()
    {
        float cameraHalfHeight = Camera.main.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;
        ticks += Time.deltaTime * 40;

        //C movement
        if (ticks < 37f)
        {
            float scale = Mathf.Max(-0.015f * (ticks - 25f) * (ticks - 65f) + 1f, 1f) / 3;
            float rotation = 0.01507964473723f * ticks * (ticks - 70.710678118654752f);
            float x = (rotation + 6f * Mathf.PI) * 0.054f * (transform.localScale.x / 2 + cameraHalfWidth);

            transform.localScale = new Vector3(scale, scale, transform.localScale.z);
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            transform.rotation = Quaternion.Euler(0f, 0f, -rotation * Mathf.Rad2Deg);
        }
        else if (ticks < 40f)
        {
            float scale = Mathf.Max(-0.015f * (ticks - 25f) * (ticks - 65f) + 1f, 1f) / 3;
            float rotation = -6f * Mathf.PI;
            transform.localScale = new Vector3(scale, scale, transform.localScale.z);
            transform.position = new Vector3(0f, transform.position.y, transform.position.z);
            transform.rotation = Quaternion.Euler(0f, 0f, rotation * Mathf.Rad2Deg);
        }
        else if (ticks < 70f)
        {
            float scale = 2.192f;
            float rotation = -6f * Mathf.PI;
            float t = (ticks - 40f) / 30f;
            float x = -2.8f + 2.8f * (Mathf.Pow(1 - t, 3) + 3f * t * Mathf.Pow(1 - t, 2));

            transform.localScale = new Vector3(scale, scale, transform.localScale.z);
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            transform.rotation = Quaternion.Euler(0f, 0f, rotation * Mathf.Rad2Deg);
        }

        //Text opacity
        if (ticks >= 45 && ticks < 200)
        {
            float oinsOpacity = Mathf.Clamp01((ticks - 45f) / 80f);
            SetTextOpacity(topText, oinsOpacity);

            float bottomOpacity = Mathf.Clamp01((ticks - 120f) / 20f);
            SetTextOpacity(bottomText, bottomOpacity);
            SetTextOpacity(subText, bottomOpacity);

            if (ticks > 80 && ticks < 120)
            {
                float topOpacity = Mathf.Clamp01((ticks - 80f) / 40f);
                SetTextOpacity(oinsText, topOpacity);
            }
            else if (ticks >= 120)
            {
                SetTextOpacity(oinsText, 1f);
            }
        }
        else if (ticks >= 220 && !transitionStarted) {
            SceneTransition.LoadScene("Main Menu", true);
            transitionStarted = true;
        }
    }

    void SetTextOpacity(TextMeshProUGUI text, float opacity)
    {
        if (text != null)
        {
            Color color = text.color;
            color.a = opacity;
            text.color = color;
        }
    }
}
