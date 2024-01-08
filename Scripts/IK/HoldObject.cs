using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEngine.Serialization;

public class HoldObject : MonoBehaviour
{
    public FullBodyBipedIK bodyIK;
    public AimIK aimIK;
    public LookAtIK lookIK;
    public ObjectIKHoldPose objectHoldPose;
    public HandPoser LeftHandPoser, RightHandPoser;
    public bool LeftHandEnabled = false;
    public bool RightHandEnabled = false;
    
    [Header("IK Weights")]
    [Range(0f,1f)]
    public float LeftHandPositionWeight = 1.0f;
    [Range(0f,1f)]
    public float LeftHandRotationWeight = 1.0f;
    [Range(0f,1f)]
    public float RightHandPositionWeight = 1.0f;
    [Range(0f,1f)]
    public float RightHandRotationWeight = 1.0f;
    [Header("Pose Weights")]
    [Range(0f,1f)]
    public float LeftHandPosePositionWeight = 0.0f;
    [Range(0f,1f)]
    public float LeftHandPoseRotationWeight = 1.0f;
    [Range(0f,1f)]
    public float RightHandPosePositionWeight = 0.0f;
    [Range(0f,1f)]
    public float RightHandPoseRotationWeight = 1.0f;

    private Transform _leftHandTarget, _rightHandTarget;
    
    private void Awake()
    {
        if (bodyIK == null)
            bodyIK = GetComponent<FullBodyBipedIK>();
    }

    private void Start()
    {
        //aimIK.enabled = false;
        bodyIK.enabled = false;
        //lookIK.enabled = false;
        
        if (objectHoldPose != null)
        {
            _leftHandTarget = objectHoldPose.LeftHandTarget;
            _rightHandTarget = objectHoldPose.RightHandTarget;
            LeftHandPoser.poseRoot = _leftHandTarget;
            RightHandPoser.poseRoot = _rightHandTarget;
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": Object Hold Pose field was left empty!");
        }

        if ((_leftHandTarget == null && LeftHandEnabled) || (_rightHandTarget == null && RightHandEnabled))
            Debug.LogWarning(gameObject.name + ": hand targets were not found!");
        
        if(LeftHandPoser == null|| RightHandPoser == null)
            Debug.LogWarning($"{gameObject.name} : hand poser components left empty!");

    }
    
    private void LateUpdate()
    {
        if(aimIK != null)
            aimIK.solver.Update();
        UpdateIK();
        bodyIK.solver.Update();
        if (lookIK != null) 
            lookIK.solver.Update();
    }

    private void UpdateIK()
    {
        if (LeftHandEnabled)
        {
            bodyIK.solver.leftHandEffector.position = _leftHandTarget.position;
            bodyIK.solver.leftHandEffector.rotation = _leftHandTarget.rotation;
            bodyIK.solver.leftHandEffector.positionWeight = LeftHandPositionWeight;
            bodyIK.solver.leftHandEffector.rotationWeight = LeftHandRotationWeight;
            LeftHandPoser.localPositionWeight = LeftHandPosePositionWeight;
            LeftHandPoser.localRotationWeight = LeftHandPoseRotationWeight;
            //Debug.Log($"{bodyIK.solver.leftHandEffector.position}\n{_leftHandTarget.position}");
        }

        if (RightHandEnabled)
        {
            bodyIK.solver.rightHandEffector.position = _rightHandTarget.position;
            bodyIK.solver.rightHandEffector.rotation = _rightHandTarget.rotation;
            bodyIK.solver.rightHandEffector.positionWeight = RightHandPositionWeight;
            bodyIK.solver.rightHandEffector.rotationWeight = RightHandRotationWeight;
            RightHandPoser.localPositionWeight = RightHandPosePositionWeight;
            RightHandPoser.localRotationWeight = RightHandPoseRotationWeight;
        }
    }
}
