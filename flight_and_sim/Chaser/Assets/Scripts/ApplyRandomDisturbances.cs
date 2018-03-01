using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyRandomDisturbances : MonoBehaviour {

    public float maxSingleAxisTorque;
    public float maxSingleAxisForce;

    private Rigidbody rb;

	void Start()
    {
        rb = GetComponent<Rigidbody>();

        ApplyRandomTorque();
        ApplyRandomForce();
    }

    private void ApplyRandomTorque()
    {
        float xTorque = Random.Range(0.0f, maxSingleAxisTorque);
        float yTorque = 0.0f;
        float zTorque = 0.0f;

        Debug.Log("Applying torque of (" + xTorque + ", " + yTorque + ", " + zTorque + ")");

        rb.AddTorque(xTorque, yTorque, zTorque);
    }

    private void ApplyRandomForce()
    {
        float xForce = Random.Range(0.0f, maxSingleAxisForce);
        float yForce = Random.Range(0.0f, maxSingleAxisForce);
        float zForce = Random.Range(0.0f, maxSingleAxisForce);

        Debug.Log("Applying force of (" + xForce + ", " + yForce + ", " + zForce + ")");

        rb.AddForce(xForce, yForce, zForce);
    }
    
}
