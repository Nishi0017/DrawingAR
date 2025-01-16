using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChanger : MonoBehaviour
{
    [SerializeField] Drawing drawing;

    [SerializeField] Slider widthSlider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Color"))
        {
            Color color = other.GetComponent<Renderer>().material.color;
            float r = color.r;
            float g = color.g;
            float b = color.b;
            float w = widthSlider.value;

            drawing.ColorChange(r, g, b, w);
        }
    }
}
