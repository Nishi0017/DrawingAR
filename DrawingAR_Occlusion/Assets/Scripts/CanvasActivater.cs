using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasActivater : MonoBehaviour
{
    [SerializeField] RectTransform unityCanvas;

    private void OnEnable()
    {
        unityCanvas.sizeDelta = new Vector2(1920, 1080);
    }

    private void OnDisable()
    {
        unityCanvas.sizeDelta = new Vector2(0, 0);
    }
}
