using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorDetailChanger : MonoBehaviour
{
    [SerializeField] Slider redSlider;
    [SerializeField] Slider greenSlider;
    [SerializeField] Slider blueSlider;
    [SerializeField] Slider widthSlider;

    [SerializeField] Drawing drawing;

    [SerializeField] Renderer sampleColor;
    [SerializeField] TMP_Text redText;
    [SerializeField] TMP_Text greenText;
    [SerializeField] TMP_Text blueText;

    [SerializeField] TMP_Text setButtonText;
    [SerializeField] Image setButton;

    private void Start()
    {
        ChangeSampleColor();
        ChangeTextColor(0);
        ChangeTextColor(1);
        ChangeTextColor(2);

        // Sliderの値が変更されたときに呼び出すメソッドを登録
        redSlider.onValueChanged.AddListener(delegate { ChangeSampleColor(); });
        greenSlider.onValueChanged.AddListener(delegate { ChangeSampleColor(); });
        blueSlider.onValueChanged.AddListener(delegate { ChangeSampleColor(); });

        redSlider.onValueChanged.AddListener(delegate { ChangeTextColor(0); });
        greenSlider.onValueChanged.AddListener(delegate { ChangeTextColor(1); });
        blueSlider.onValueChanged.AddListener(delegate { ChangeTextColor(2); });
    }

    public void ChangeColor()
    {
        float r = redSlider.value;
        float g = greenSlider.value;
        float b = blueSlider.value;
        float w = widthSlider.value;

        drawing.ColorDetailChange(r, g, b, w);

    }

    void ChangeSampleColor()
    {
        float r = redSlider.value;
        float g = greenSlider.value;
        float b = blueSlider.value;

        sampleColor.material.color = new Color(r, g, b);

        setButton.color = new Color(r, g, b);
        setButtonText.color = new Color(1 - r, 1 - g, 1 - b);

    }

    void ChangeTextColor(int num)
    {
        switch (num)
        {
            case 0:
                redText.color = new Color(redSlider.value, 0, 0);
                break;
            case 1:
                greenText.color = new Color(0, greenSlider.value, 0);
                break;
            case 2:
                blueText.color = new Color(0, 0, blueSlider.value);
                break;
        }

    }
}
