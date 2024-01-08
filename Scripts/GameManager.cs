using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerController playerController;
    private PlayerMeshChanger _playerMeshChanger;

    public delegate void OnLoadCompleteEventHandler();
    public OnLoadCompleteEventHandler OnLoadComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _playerMeshChanger = playerController.GetComponent<PlayerMeshChanger>();
    }
    
    public void Start()
    {
        //Freezes Player on launch.
        playerController.FreezeRigidBody();
        playerController.GetComponent<PlayerMeshChanger>().SwitchToHandMesh();
    }

    public void ChangePlayerMeshToHands()
    {
        _playerMeshChanger.SwitchToHandMesh();
    }
    
    public void ChangePlayerMeshToMech()
    {
        _playerMeshChanger.SwitchToMechMesh();
    }

    public void LoadComplete()
    {
        OnLoadComplete?.Invoke();
        LightProbes.Tetrahedralize();
        Transform newPosition = GameObject.FindWithTag("PlayerStartPosition").transform;
        playerController.RelocateRig(newPosition.position, newPosition.forward);
        playerController.ReleaseRigidBody();
        //playerController.Body.Calibrate();
    }
}
