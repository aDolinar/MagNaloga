using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Regulator
{
    public float velocityP;
    public float velocityI;
    public float velocityErrorMax;
    [HideInInspector] public float velocityErrorAccumulated;

    public float steeringP;
    public float steeringI;
    public float steeringErrorMax;
    [HideInInspector] public float steeringErrorAccumulated;

    public void AddIerrors(float IErrorVelocity, float IErrorSteering)
    {
        velocityErrorAccumulated += IErrorVelocity * Time.fixedDeltaTime;
        if (Mathf.Abs(velocityErrorAccumulated) > velocityErrorMax)
        {
            velocityErrorAccumulated = Mathf.Sign(velocityErrorAccumulated) * velocityErrorMax;

        }
        steeringErrorAccumulated += IErrorSteering * Time.fixedDeltaTime;
        if (Mathf.Abs(steeringErrorAccumulated) > steeringErrorMax)
        {
            steeringErrorAccumulated = Mathf.Sign(steeringErrorAccumulated) * steeringErrorMax;
        }
    }
    public string DebugRegulator()
    {
        return ("Velocity error acc: " + velocityErrorAccumulated.ToString() + "   Steering error acc: " + steeringErrorAccumulated.ToString());
    }
    public void Reset()
    {
        velocityErrorAccumulated = 0f;
        steeringErrorAccumulated = 0f;
    }
}