using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    float height;
    Transform gate2;
    bool gateOpen;
    bool animating;
    const int animationDuration = 50;
    const float animationRate = 0.1f;
    WaitForFixedUpdate WFFU;
    GameObject[] robotTransf;
    const float distThreshold = 10f;
    bool needOpen;
    void Start()
    {
        gate2 = transform.GetChild(0);
        height = gate2.localPosition.y;
        gateOpen = true;
        WFFU = new WaitForFixedUpdate();
        animating = false;
        SetDoorState(false);
        robotTransf = GameObject.FindGameObjectsWithTag("Robot");
        
        InvokeRepeating("CheckProximity", 1f, 1f);
    }
    void CheckProximity()
    {
        if (!animating)
        {
            needOpen = false;
            for (int i = 0; i < robotTransf.Length; i++)
            {
                if ((transform.position - robotTransf[i].transform.position).magnitude < distThreshold)
                {
                    needOpen = true;
                    break;
                }
            }
            SetDoorState(needOpen);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            SetDoorState(!gateOpen);
        }
    }
    void SetDoorState(bool newState)
    {
        if (gateOpen!=newState)
        {
            if (!animating)
            {
                StartCoroutine(AnimateDoor(newState));
            }
        }
    }
    IEnumerator AnimateDoor(bool newState)
    {
        gateOpen = newState;
        animating = true;
        Vector3 targetPos=Vector3.zero;
        if (newState)
        {
            targetPos.y = height;
        }
        for (int i = 0; i < animationDuration; i++)
        {
            gate2.transform.localPosition = Vector3.Lerp(gate2.transform.localPosition, targetPos,animationRate);
            yield return WFFU;
        }
        gate2.transform.localPosition = targetPos;
        animating = false;
    }
}
