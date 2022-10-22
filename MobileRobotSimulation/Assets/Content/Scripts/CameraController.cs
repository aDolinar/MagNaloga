using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float interpolationT;
    public float speed;
    public Transform target;
    public Transform mobileTarget;
    Vector3[] cameraPos;
    float[] cameraRot;
    bool verticalEnabled;
    bool verticalMode;
    bool farMode;
    public KeyCode[] keys;
    public Transform cam;
    Quaternion rot90;
    Quaternion rotm90;
    Camera cam2;
    const float fov1 = 18;
    const float fov2 = 60;
    float targetFov;


    void Start()
    {
        GameObject startRobot = GameObject.FindGameObjectWithTag("Start");
        if (startRobot!=null)
        {
            target.position = startRobot.transform.position;
        }
        verticalMode = false;
        farMode = false;
        verticalEnabled = true;
        transform.position = target.position;
        transform.rotation = target.rotation;
        rot90 = Quaternion.Euler(0f, 90f, 0f);
        rotm90 = Quaternion.Euler(0f, -90f, 0f);
        cameraPos = new Vector3[2];
        cameraRot = new float[2];
        cameraPos[0] = cam.localPosition;
        cameraPos[1] = new Vector3(0f, cameraPos[0].magnitude, 0f);
        cameraRot[0] = cam.localEulerAngles.x;
        cameraRot[1] = 90f;
        cam2=cam.GetComponent<Camera>();
        cam2.fieldOfView = fov1;
        targetFov = fov1;
    }
    private void Update()
    {
        if (mobileTarget == null)
        {
            //inputs
            if (Input.GetKey(keys[0]))
            {
                target.transform.position += target.transform.forward * speed * Time.deltaTime;
            }
            if (Input.GetKey(keys[1]))
            {
                target.transform.position -= target.transform.forward * speed * Time.deltaTime;
            }
            if (Input.GetKey(keys[2]))
            {
                target.transform.position -= target.transform.right * speed * Time.deltaTime;
            }
            if (Input.GetKey(keys[3]))
            {
                target.transform.position += target.transform.right * speed * Time.deltaTime;
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                if (Input.GetKeyDown(keys[i]))
                {
                    mobileTarget = null;
                    break;
                }
            }
        }
        if (Input.GetKeyDown(keys[4]))
        {
            target.transform.rotation *= rot90;
        }
        if (Input.GetKeyDown(keys[5]))
        {
            target.transform.rotation *= rotm90;
        }
        if (Input.GetKeyDown(keys[6]))
        {
            if (verticalEnabled)
                StartCoroutine(ToggleVertical());
        }
        if (Input.GetKeyDown(keys[7]))
        {
            farMode = !farMode;
            if (farMode)
            {
                targetFov = fov2;
            }
            else
            {
                targetFov = fov1;
            }
        }
    }
    IEnumerator ToggleVertical()
    {
        verticalEnabled = false;
        WaitForFixedUpdate WFFU = new WaitForFixedUpdate();
        Vector3 newTarget;
        float newRot;
        verticalMode = !verticalMode;
        if (verticalMode)
        {
            target.transform.rotation *= Quaternion.Euler(0f, 45f, 0f);
            newTarget = cameraPos[1];
            newRot = cameraRot[1];
        }
        else
        {
            target.transform.rotation *= Quaternion.Euler(0f, -45f, 0f);
            newTarget = cameraPos[0];
            newRot = cameraRot[0];
        }
        Quaternion targetRot = Quaternion.Euler(newRot, 0f, 0f);
        for (int i = 0; i < 50; i++)
        {
            cam.localPosition = Vector3.Lerp(cam.localPosition, newTarget, interpolationT);
            cam.localRotation = Quaternion.Slerp(cam.localRotation, targetRot, interpolationT);
            yield return WFFU;
        }
        cam.localPosition = newTarget;
        cam.localRotation = targetRot;
        verticalEnabled = true;
    }
    void FixedUpdate()
    {
        if (mobileTarget!=null)
        {
            target.position = mobileTarget.position;
        }

        transform.position = Vector3.Lerp(transform.position, target.position, interpolationT);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, interpolationT);
        cam2.fieldOfView = Mathf.Lerp(cam2.fieldOfView, targetFov, interpolationT);
    }
    public void AssignNewMobileTarget(Transform newTarget)
    {
        mobileTarget = newTarget;
    }
}