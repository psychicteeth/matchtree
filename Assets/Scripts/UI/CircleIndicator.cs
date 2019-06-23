using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Draws a circular progress/counter indicator.
public class CircleIndicator : MonoBehaviour
{
    // The circle is just a ring gradient and we set the cutoff value accordingly and colour it.
    // set in editor
    public GameObject circlePrefab;

    public float maxValue = 120;
    public float value = 0;
    // Can run around several layers (e.g. first is red, second yellow etc)
    public List<Color> layerColors = new List<Color>();

    // stashed references
    int numLayers;
    List<Image> circleImages = new List<Image>();

    static int colorID = Shader.PropertyToID("_Color");
    static int cutoffID = Shader.PropertyToID("_Cutoff");
    const float minAlpha = 0.002f; // if we cut off at 0 the whole textue backdrop will bleed out
    const float maxAlpha = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        numLayers = layerColors.Count;
        float z = 0;
        foreach (Color c in layerColors)
        {
            GameObject circle = GameObject.Instantiate(circlePrefab);
            circle.transform.SetParent(transform);
            Image image = circle.GetComponent<Image>();
            //image.color = c;
            image.material = new Material(image.material);
            image.material.SetColor(colorID, c);
            image.material.SetFloat(cutoffID, 0);
            circleImages.Add(image);
            // put successive rings in front of one another
            Vector3 pos = image.transform.localPosition;
            pos.z = z;
            z += 1;
            // also recenter it to the parent
            image.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        }
    }

    // Update is called once per frame
    void Update()
    {
        float band = maxValue / numLayers;
        for (int i = 0; i < numLayers; i++)
        {
            // get a value from 0 - 1 representing how much we should be showing of this ring
            float val = Mathf.Clamp(value, band * i, band * (i + 1)) / band;
            val -= i;
            val = Mathf.Clamp01(val);
            // whoops I did the gradient backwards
            val = Mathf.Lerp(minAlpha, maxAlpha, 1.0f - val);
            circleImages[i].material.SetFloat(cutoffID, val);
        }
    }
}
