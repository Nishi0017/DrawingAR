using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    [SerializeField] Drawing drawing;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Color"))
        {
            Color color = other.GetComponent<Renderer>().material.color;
            drawing.ColorChange(color);
        }
    }
}
