using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rangefinder : MonoBehaviour {

    public UIUpdater uiUpdater;

    public float maxRange;
    public Text rangeText;

    private Transform myTransform;
	
	void Update()
    {
        myTransform = transform;

        float range = FindRange();
        uiUpdater.UpdateRangeDisplay(range);
	}

    private float FindRange()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(myTransform.position, myTransform.forward, out hitInfo, maxRange))
        {
            return hitInfo.distance;
        }
        else
        {
            // No hit!
            return -1.0f;
        }
    }


}
