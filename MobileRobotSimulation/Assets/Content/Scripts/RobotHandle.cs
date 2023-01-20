using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotHandle : MonoBehaviour
{
    //outside components
    public Transform steering;
    public Transform lidarOrigin;
    Rigidbody RB;
    //classes
    public Telemetry telemetry;
    public Regulator regulator;
    //wheels
    public WheelCollider[] wheels;
    public Transform[] tyres;
    //preallocated
    Vector3 tmpPos;
    Quaternion tmpRot;
    //lidar
    const int lidarCount = 5;
    const float lidarAngleDelta = 30f;
    const float lidarRange = 20f;
    Vector3[] lidarForwards;
    float[] lidarValues;
    RaycastHit hit;
    //linefollow
    const int lineFollowCount = 7;
    const float lineFollowDistDelta = 0.075f;
    const float lineFollowRange = 0.5f;
    Vector3[] lineFollowOrigins;
    bool[] lineFollowValues;
    LayerMask lineFollowMask;
    //tagReader
    public Transform tagReader;
    LayerMask tagMask;
    const float tagRange=0.75f;
    Collider[] foundTags;

    //udp
    public UDPComms udp;
    float[] dataToSend;
    const int totalData = 21;
    int k;
    float lineFollowResult;
    void Start()
    {
        RB = GetComponent<Rigidbody>();

        telemetry.realTorque = 0f;
        telemetry.realBraking = 0f;
        telemetry.realSteering = 0f;
        //init lidar
        lidarForwards = new Vector3[lidarCount];
        lidarValues = new float[lidarCount];
        for (int i = 0; i < lidarCount; i++)
        {
            lidarForwards[i] = Quaternion.Euler(0f, lidarAngleDelta * (i - lidarCount / 2), 0f) * Vector3.forward;
        }
        //init lineFollow
        lineFollowOrigins = new Vector3[lineFollowCount];
        lineFollowValues = new bool[lineFollowCount];
        for (int i = 0; i < lineFollowCount; i++)
        {
            lineFollowOrigins[i] = lidarOrigin.localPosition + Vector3.right * (i - lineFollowCount / 2) * lineFollowDistDelta;
        }
        lineFollowMask = LayerMask.GetMask("LineFollow");
        //init udp
        dataToSend = new float[totalData];
        OnMatlabReset();
        //init tagReader
        tagMask = LayerMask.GetMask("Markers");
        InvokeRepeating("CheckForTag", 1f, 0.2f);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (udp.GetDataIdx(0)>1f)
        {
            OnMatlabReset();
        }
        UpdateTargets(udp.GetDataIdx(1), udp.GetDataIdx(2));
        UpdateTelemetry();
        RegulateTelemetryValues();

        ApplyTorque();
        ApplyBrakingTorque();
        ApplySteering();
        ApplyTyrePose();
        RotateSteeringIndicator();

        HandleLidar();
        HandleLineFollow();

        SendDataUDP();
        //Debug.Log(telemetry.DebugTelemetry() + "   " + regulator.DebugRegulator());
    }
    void CheckForTag()
    {
        foundTags=Physics.OverlapSphere(tagReader.position, tagRange,tagMask);
        if (foundTags.Length!=0)
        {
            telemetry.lastMarkerDetected = foundTags[0].transform.GetSiblingIndex();
        }
    }

    void UpdateTargets(float newTargetVelocity, float newTargetSteering)
    {
        telemetry.targetVelocity = newTargetVelocity;
        telemetry.targetSteering = newTargetSteering;

        if (Mathf.Abs(telemetry.realVelocity) > telemetry.maxVelocity)
        {
            telemetry.targetVelocity = telemetry.maxVelocity * Mathf.Sign(telemetry.targetVelocity);
        }
        if (Mathf.Abs(telemetry.targetSteering) > telemetry.maxSteering)
        {
            telemetry.targetSteering = telemetry.maxSteering * Mathf.Sign(telemetry.targetSteering);
        }
    }
    void UpdateTelemetry()
    {
        telemetry.realVelocity = Vector3.Dot(RB.velocity,transform.forward);
        telemetry.realOrientation = transform.rotation.eulerAngles;
        telemetry.realPosition = transform.position;

        telemetry.errorVelocity = telemetry.targetVelocity - telemetry.realVelocity;
        telemetry.errorSteering = telemetry.targetSteering - telemetry.realSteering;
    }

    void RegulateTelemetryValues()
    {
        regulator.AddIerrors(telemetry.errorVelocity, telemetry.errorSteering);
        telemetry.realTorque = regulator.velocityP * telemetry.errorVelocity + regulator.velocityI * regulator.velocityErrorAccumulated;
        if (Mathf.Approximately(telemetry.targetVelocity,0f))
        {
            //Debug.Log("BRAKE "+telemetry.targetVelocity);
            telemetry.realBraking = 20f;
        }
        else
        {
            telemetry.realBraking = 0f;
        }
        telemetry.realSteering += (regulator.steeringP * telemetry.errorSteering + regulator.steeringI * regulator.steeringErrorAccumulated) * Time.fixedDeltaTime;
    }
    void ApplyTorque()
    {
        for (int i = 0; i < 2; i++)
        {
            wheels[i].motorTorque = telemetry.realTorque;
        }
    }
    void ApplyBrakingTorque()
    {
        for (int i = 0; i < 4; i++)
        {
            wheels[i].brakeTorque = telemetry.realBraking;
        }
    }
    void ApplySteering()
    {
        for (int i = 0; i < 2; i++)
        {
            wheels[i].steerAngle = telemetry.realSteering;
        }
    }
    void ApplyTyrePose()
    {
        for (int i = 0; i < 4; i++)
        {
            wheels[i].GetWorldPose(out tmpPos, out tmpRot);
            tyres[i].SetPositionAndRotation(tmpPos, tmpRot);
        }
    }
    void RotateSteeringIndicator()
    {
        steering.localRotation = Quaternion.Euler(-90f, telemetry.realSteering, 0f);
    }
    void HandleLidar()
    {
        for (int i = 0; i < lidarCount; i++)
        {
            if (Physics.Raycast(lidarOrigin.position, transform.rotation * lidarForwards[i], out hit, lidarRange))
            {
                lidarValues[i] = hit.distance;
            }
            else
            {
                lidarValues[i] = lidarRange;
            }
        }
    }

    void HandleLineFollow()
    {
        for (int i = 0; i < lineFollowCount; i++)
        {
            if (Physics.Raycast(transform.position + transform.rotation * lineFollowOrigins[i], Vector3.down, lineFollowRange, lineFollowMask))
            {
                lineFollowValues[i] = true;
            }
            else
            {
                lineFollowValues[i] = false;
            }
        }
    }
    void SendDataUDP()
    {
        //robot pose, first is 0
        k = 0;
        dataToSend[k++] = transform.position.x;
        dataToSend[k++] = transform.position.y;
        dataToSend[k++] = transform.position.z;

        dataToSend[k++] = transform.rotation.eulerAngles.x;
        dataToSend[k++] = transform.rotation.eulerAngles.y;
        dataToSend[k++] = transform.rotation.eulerAngles.z;

        //robot telemetry, first is 6
        dataToSend[k++] = telemetry.realVelocity;
        dataToSend[k++] = telemetry.realSteering;

        dataToSend[k++] = telemetry.errorVelocity;
        dataToSend[k++] = telemetry.errorSteering;

        dataToSend[k++] = telemetry.realTorque;
        dataToSend[k++] = telemetry.realBraking;

        //simulation time, first is 12
        dataToSend[k++] = Time.time;

        //robot lidar, first is 13
        for (int i = 0; i < lidarCount; i++)
        {
            dataToSend[k++] = lidarValues[i];
        }

        //robot lineFollow, first is 13+lidarCount
        lineFollowResult = 0f;
        for (int i = 0; i < lineFollowCount; i++)
        {
            if (lineFollowValues[i])
            {
                lineFollowResult += Mathf.Pow(10f, i);
            }
        }
        dataToSend[k++] = lineFollowResult;
        dataToSend[k++] = telemetry.activeCollisions;
        dataToSend[k++] = telemetry.lastMarkerDetected;
        //send data
        udp.SendData(dataToSend);
    }
    public void OnMatlabReset()
    {
        GameObject startMarker = GameObject.FindGameObjectWithTag("Start");
        RB.velocity = Vector3.zero;
        RB.angularVelocity = Vector3.zero;
        transform.position = startMarker.transform.position + Vector3.up * 0.45f;
        transform.rotation = startMarker.transform.rotation;
        telemetry.Reset();
        regulator.Reset();
        telemetry.lastMarkerDetected = FindClosestMarker(transform.position);
    }
    public void MoveRobotOnLoad(Vector3 pos, Quaternion rot)
    {
        RB.velocity = Vector3.zero;
        RB.angularVelocity = Vector3.zero;
        transform.position = pos+Vector3.up*0.45f;
        transform.rotation = rot;
        telemetry.Reset();
        regulator.Reset();
        telemetry.lastMarkerDetected = FindClosestMarker(transform.position);
    }
    private void OnCollisionEnter(Collision collision)
    {
        telemetry.activeCollisions++;
    }
    private void OnCollisionExit(Collision collision)
    {
        telemetry.activeCollisions--;
    }
    int FindClosestMarker(Vector3 worldPos)
    {
        float closest = Mathf.Infinity;
        int closestID = 0;
        float tmpDist;
        GameObject[] allMarkers = GameObject.FindGameObjectsWithTag("Marker");
        if (allMarkers.Length>0)
        {
            for (int i = 0; i < allMarkers.Length; i++)
            {
                tmpDist = (worldPos - allMarkers[i].transform.position).magnitude;
                if (closest > tmpDist)
                {
                    closest = tmpDist;
                    closestID = i;
                }
            }
            return allMarkers[closestID].transform.GetSiblingIndex();
        }
        else
        {
            return 0;
        }
    }
}