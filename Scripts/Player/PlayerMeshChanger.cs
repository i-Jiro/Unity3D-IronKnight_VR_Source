using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeshChanger : MonoBehaviour
{
    [SerializeField] private GameObject MechaMeshGameObject;
    [SerializeField] private GameObject LeftHandGameObjestMesh;
    [SerializeField] private GameObject RightHandGameObjectMesh;

    public void SwitchToMechMesh()
    {
        LeftHandGameObjestMesh.SetActive(false);
        RightHandGameObjectMesh.SetActive(false);
        MechaMeshGameObject.SetActive(true);
    }

    public void SwitchToHandMesh()
    {
        LeftHandGameObjestMesh.SetActive(true);
        RightHandGameObjectMesh.SetActive(true);
        MechaMeshGameObject.SetActive(false);
    }
}
