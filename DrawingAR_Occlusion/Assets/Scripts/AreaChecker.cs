using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaChecker : MonoBehaviour
{
    [SerializeField] string checkTagName = "CanPaint";
    private bool inArea = false;
    private bool isAreaEnter, isAreaStay, isAreaExit;

    public bool InArea()
    {
        if(isAreaEnter || isAreaStay)
        {
            inArea = true;
        }
        else if (isAreaExit)
        {
            inArea = false;
        }

        isAreaEnter = false;
        isAreaStay = false;
        isAreaExit = false;
        return inArea;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == checkTagName)
        {
            isAreaEnter = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == checkTagName)
        {
            isAreaStay = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == checkTagName)
        {
            isAreaExit = true;
        }
    }
}
