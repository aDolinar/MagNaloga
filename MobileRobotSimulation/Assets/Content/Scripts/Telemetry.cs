using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Telemetry
{
    public float maxSteering;
    public float maxVelocity;

    [HideInInspector] public float realTorque;
    [HideInInspector] public float realSteering;
    [HideInInspector] public float realBraking;
    [HideInInspector] public float realVelocity;

    [HideInInspector] public Vector3 realOrientation;
    [HideInInspector] public Vector3 realPosition;

    public float targetVelocity;
    public float targetSteering;

    [HideInInspector] public float errorVelocity;
    [HideInInspector] public float errorSteering;

    public string DebugTelemetry()
    {
        return ("torque: " + realTorque.ToString() + "   steering: " + realSteering.ToString() + "   braking: " + realBraking.ToString() + "   velocity: " + realVelocity.ToString());
    }
    public void Reset()
    {
        realTorque = 0f;
        realSteering = 0f;
        realBraking = 0f;
        realVelocity = 0f;
        targetVelocity = 0f;
        targetSteering = 0f;
        errorVelocity = 0f;
        errorSteering = 0f;
    }
}