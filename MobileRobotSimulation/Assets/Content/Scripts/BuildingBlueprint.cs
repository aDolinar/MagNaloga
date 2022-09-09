using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BuildingBlueprint
{
    public GameObject prefab;
    public Vector3 offset;
    [HideInInspector] public int ID;
}