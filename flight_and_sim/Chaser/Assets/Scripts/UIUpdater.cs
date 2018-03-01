using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUpdater : MonoBehaviour
{
    public Text xAttText;
    public Text yAttText;
    public Text zAttText;

    public Text xPosText;
    public Text yPosText;
    public Text zPosText;
    
    public Text rangefinderRangeText;

    void Start()
    {
        Application.runInBackground = true;    
    }

    public void UpdateRangeDisplay(float range)
    {
        if (range > 0.0f)
        {
            rangefinderRangeText.text = range.ToString("0.000");
        }
        else
        {
            rangefinderRangeText.text = "INVALID";
        }
    }

    public void UpdateTextFromStateEstimate(StateEstimate se)
    {
        xAttText.text = "x: " + se.xAtt.ToString("0.000");
        yAttText.text = "y: " + se.yAtt.ToString("0.000");
        zAttText.text = "z: " + se.zAtt.ToString("0.000");
                
        xPosText.text = "x: " + se.xPos.ToString("0.000");
        yPosText.text = "y: " + se.yPos.ToString("0.000");
        zPosText.text = "z: " + se.zPos.ToString("0.000");
    }
}
