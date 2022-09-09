using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager instance;
    public Transform BuildingParent;
    bool buildMode;
    [Header("KeyCodes")]
    public KeyCode toggleBuildMode;
    public KeyCode rotateBuilding;
    public KeyCode snapMode;
    [Header("Prefabs")]
    public BuildingBlueprint[] blueprints;
    int selectedBlueprint;
    public Material selectionMat;
    public Material demolishMat;
    Material[] savedMats;
    GameObject ghostObj;
    GameObject demolishObj;
    Camera cam;
    CameraController camCtrl;
    LayerMask floorMask;
    LayerMask buildingMask;
    LayerMask robotMask;
    //preallocatted vars
    int rot;
    float snapSize;
    Ray ray;
    RaycastHit hit;
    Vector3 posVect;
    void Start()
    {
        instance = this;
        SetBuildMode(false);
        selectedBlueprint = 0;
        cam = Camera.main;
        floorMask = LayerMask.GetMask("Floor");
        buildingMask = LayerMask.GetMask("Buildings","Markers","LineFollow");
        robotMask = LayerMask.GetMask("Robot");
        rot = 0;
        snapSize = 1f;
        for (int i = 0; i < blueprints.Length; i++)
        {
            blueprints[i].ID = i;
        }
        camCtrl = cam.transform.parent.gameObject.GetComponent<CameraController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleBuildMode))
        {
            SetBuildMode(!buildMode);
        }
        if (buildMode)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                CyclePrefabs(true);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                CyclePrefabs(false);
            }
            if (Input.GetKeyDown(rotateBuilding))
            {
                Rotate();
            }
            if (Input.GetKeyDown(snapMode))
            {
                CycleSnap();
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Build();
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (ghostObj!=null)
                {
                    ghostObj.transform.localScale = Vector3.one * 0.001f;
                }
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                Demolish();

                if (ghostObj != null)
                {
                    ghostObj.transform.localScale = Vector3.one;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray,out hit, 200f, robotMask))
                {
                    camCtrl.AssignNewMobileTarget(hit.transform);
                }
            }
        }
    }
    void Demolish()
    {
        ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 500f, buildingMask))
        {
            GameObject clickedGameObject = hit.collider.gameObject;
            if (clickedGameObject.transform.parent!=null)
            {
                clickedGameObject = clickedGameObject.transform.parent.gameObject;
            }
            //first click
            if (demolishObj==null)
            {
                demolishObj = clickedGameObject;
                MeshRenderer MR = demolishObj.GetComponent<MeshRenderer>();
                Material[] mats = MR.materials;
                savedMats = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    savedMats[i] = mats[i];
                    mats[i] = demolishMat;
                }
                MR.materials = mats;

            }
            else
            {
                //second click
                if (clickedGameObject == demolishObj)
                {
                    Destroy(demolishObj);
                    demolishObj = null;
                }
                //different target
                else
                {
                    MeshRenderer MR = demolishObj.GetComponent<MeshRenderer>();
                    Material[] mats = MR.materials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        mats[i] = savedMats[i];
                    }
                    MR.materials = mats;

                    demolishObj = clickedGameObject;
                    MR = demolishObj.GetComponent<MeshRenderer>();
                    mats = new Material[MR.materials.Length];
                    mats = MR.materials;
                    savedMats = new Material[mats.Length];
                    for (int i = 0; i < mats.Length; i++)
                    {
                        savedMats[i] = mats[i];
                        mats[i] = demolishMat;
                    }
                    MR.materials = mats;
                }
            }


        }
    }
    private void FixedUpdate()
    {
        if (ghostObj)
        {
            Vector3 newPos = GetPosFromMousePos() + blueprints[selectedBlueprint].offset;
            ghostObj.transform.position = newPos;
        }
    }
    void Build()
    {
        if (ghostObj != null)
        {

            GameObject newObj=Instantiate(blueprints[selectedBlueprint].prefab, ghostObj.transform.position, ghostObj.transform.rotation);
            newObj.transform.parent = BuildingParent;
        }
    }
    void Rotate()
    {
        rot += 90;
        if (rot > 359)
            rot = 0;
        if (ghostObj != null)
        {
            ghostObj.transform.rotation = GetBuildRot();
        }
    }
    Quaternion GetBuildRot()
    {
        return Quaternion.Euler(0f, (float)rot, 0f);
    }
    void SetupGhost()
    {
        if (ghostObj != null)
        {
            Destroy(ghostObj);
            ghostObj = null;
        }
        ghostObj = Instantiate(blueprints[selectedBlueprint].prefab, GetPosFromMousePos() + blueprints[selectedBlueprint].offset, GetBuildRot());
        MeshRenderer MR = ghostObj.GetComponent<MeshRenderer>();
        Material[] mats = MR.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i] = selectionMat;
        }
        MR.materials = mats;
        BoxCollider BC = ghostObj.GetComponent<BoxCollider>();
        if (BC != null)
        {
            BC.enabled = false;
            return;
        }
        MeshCollider MC = ghostObj.GetComponent<MeshCollider>();
        if (MC != null)
        {
            MC.enabled = false;
            return;
        }
        SphereCollider SC = ghostObj.GetComponent<SphereCollider>();
        if (SC != null)
        {
            SC.enabled = false;
            return;
        }
    }
    void CycleSnap()
    {
        snapSize *= 10f;
        if (snapSize > 100f)
            snapSize = 1f;
    }
    void SetBuildMode(bool newState)
    {
        buildMode = newState;
        if (newState)
        {
            if (ghostObj == null)
            {
                SetupGhost();
            }
        }
        else
        {
            Destroy(ghostObj);
            ghostObj = null;
            if (demolishObj!=null)
            {
                MeshRenderer MR = demolishObj.GetComponent<MeshRenderer>();
                Material[] mats = MR.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = savedMats[i];
                }
                MR.materials = mats;
                demolishObj = null;
            }
        }
    }
    void CyclePrefabs(bool dir)
    {
        if (dir)
        {
            selectedBlueprint++;
            if (selectedBlueprint >= blueprints.Length)
            {
                selectedBlueprint = 0;
            }
        }
        else
        {
            selectedBlueprint--;
            if (selectedBlueprint < 0)
            {
                selectedBlueprint = blueprints.Length - 1;
            }
        }
        if (ghostObj != null)
        {
            SetupGhost();
        }
    }
    Vector3 GetPosFromMousePos()
    {
        ray = cam.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, 200f,floorMask);
        posVect = hit.point * snapSize;
        posVect = new Vector3(Mathf.Round(posVect.x), Mathf.Round(posVect.y), Mathf.Round(posVect.z)) / snapSize;
        return posVect;
    }
}
