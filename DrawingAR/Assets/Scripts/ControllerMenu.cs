using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerMenu : MonoBehaviour
{
    [SerializeField] Drawing drawing;
    [SerializeField] UIManager uiManager;

    private void Update()
    {
        if(OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
        {
            drawing.DeleteLines();
        }

        if(OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            drawing.PaletteController();
            uiManager.SetUIVisibility(0);
        }
    }
}
