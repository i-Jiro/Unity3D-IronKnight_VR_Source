using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

//Rotates hand effector left or right relative to the target weapon.
[RequireComponent(typeof(Tracker))]
public class DefenseStance : MonoBehaviour
{
    public bool debugMode;
    [SerializeField] private Transform _handEffector;
    [FormerlySerializedAs("_midSection")] [SerializeField] private Transform _midBodyLevel;
    [FormerlySerializedAs("_HeadSection")] [SerializeField] private Transform _headLevel;
    [SerializeField] private GameObject _target;
    [SerializeField] private SO_BlockingPose _blockingPose;
    [SerializeField] private bool _isTracking = false;

    [Header("Fine Tuning")]
    [SerializeField] private float _duration = 1f;
    public Vector3 offset;
    [SerializeField] private float _minXBound = -0.20f;
    [SerializeField] private float _maxXBound = 0.30f;
    [SerializeField] private float _minYBound = 0.80f;
    [SerializeField] private float _maxYBound = 1.0f;

    private enum States {UpperLeft, UpperRight, LowerRight, LowerLeft, Idle, TopMiddle}
    private States _currentState = States.Idle;
    private Vector3 _directionToTarget;
    private bool _hasTarget = false;

    private void Start()
    {
        DOTween.Init();
    }

    //Visual representation of constraints.
    private void OnDrawGizmos()
    {
        if (!debugMode) return;
        
        var zOffset = 0.5f;
        Gizmos.matrix = transform.localToWorldMatrix;
        
        if (_handEffector != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(transform.InverseTransformPoint(_handEffector.position), Vector3.one * 0.05f);
        }
        
        Gizmos.color = Color.green;
        Vector3 xBoundsMax = new Vector3(_maxXBound, (_maxYBound + _minYBound) * 0.5f, zOffset);
        Vector3 xBoundsMin = new Vector3(_minXBound, (_maxYBound + _minYBound) * 0.5f, zOffset);
        Gizmos.DrawLine(xBoundsMax, xBoundsMin);

        Vector3 yBoundsMin = new Vector3((_minXBound + _maxXBound) * 0.5f, _minYBound, zOffset);
        Vector3 yBoundsMax = new Vector3((_minXBound + _maxXBound) * 0.5f, _maxYBound, zOffset);
        Gizmos.DrawLine(yBoundsMax, yBoundsMin);
    }

    // Update is called once per frame
    void Update()
    {
        if(_isTracking)
            UpdateStance();
    }
    
    //TODO: Random number gen on SO poses for variations.
    void UpdateStance()
    {
        if (!_hasTarget)
        {
            if (_currentState != States.Idle)
            {
                _handEffector.DOLocalMove(_blockingPose.Default.position, _duration)
                    .SetUpdate(UpdateType.Normal);
                _handEffector.DOLocalRotate(_blockingPose.Default.rotation.eulerAngles, _duration)
                    .SetUpdate(UpdateType.Normal);
                _currentState = States.Idle;
            }
            return;
        }
        
        _directionToTarget = (_target.transform.position - transform.position).normalized;
        var crossHorizontal = Vector3.Cross(_directionToTarget, transform.forward);
        var midDot = Vector3.Dot(_midBodyLevel.transform.up, (_target.transform.position - _midBodyLevel.position).normalized);
        var headDot = Vector3.Dot(_headLevel.transform.up, (_target.transform.position - _headLevel.position).normalized);

        if (headDot > 0)
        {
            if (_currentState != States.TopMiddle)
            {
                if (crossHorizontal.y > 0)
                {
                    _handEffector.transform.DOLocalMove(_blockingPose.MiddleTopLeft.position, _duration)
                        .SetUpdate(UpdateType.Normal);
                    _handEffector.transform.DOLocalRotate(_blockingPose.MiddleTopLeft.rotation.eulerAngles, _duration, RotateMode.Fast)
                        .SetUpdate(UpdateType.Normal);
                    _currentState = States.TopMiddle;
                }
                else
                {
                    _handEffector.transform.DOLocalMove(_blockingPose.MiddleTopRight.position, _duration)
                        .SetUpdate(UpdateType.Normal);
                    _handEffector.transform.DOLocalRotate(_blockingPose.MiddleTopRight.rotation.eulerAngles, _duration, RotateMode.Fast)
                        .SetUpdate(UpdateType.Normal);
                    _currentState = States.TopMiddle;
                }
            }
            return;
        }

        switch (crossHorizontal.y)
        {
            case > 0 when _currentState != States.UpperLeft:
                /*
                _handEffector.transform.DOLocalMove(_blockingPose.UpperLeft.position, _duration)
                    .SetUpdate(UpdateType.Normal); 
                */
                _handEffector.transform.DOLocalRotate(_blockingPose.UpperLeft.rotation.eulerAngles, _duration, RotateMode.Fast)
                    .SetUpdate(UpdateType.Normal);
                _currentState = States.UpperLeft;
                break;
            case < 0 when _currentState != States.UpperRight:
                /*
                _handEffector.transform.DOLocalMove(_blockingPose.UpperRight.position, _duration)
                    .SetUpdate(UpdateType.Normal);
                */
                _handEffector.transform.DOLocalRotate(_blockingPose.UpperRight.rotation.eulerAngles, _duration, RotateMode.Fast)
                    .SetUpdate(UpdateType.Normal);
                _currentState = States.UpperRight;
                break;
        }
        
        //TODO: Refactor for clarity.
        //TODO: Min and Max Z axis. Or just constrain to a z offset?
        //Follows tracked weapon.
        var localPos = transform.InverseTransformPoint(_target.transform.position + offset);
        localPos.z = transform.InverseTransformPoint(_handEffector.position).z;
        
        localPos.x = Mathf.Clamp( localPos.x, _minXBound, _maxXBound);
        localPos.y = Mathf.Clamp( localPos.y, _minYBound, _maxYBound);
        
        _handEffector.transform.DOMove(transform.TransformPoint(localPos), 0.50f)
            .SetUpdate(UpdateType.Normal);
    }

    public void EnableTracking()
    {
        //_handEffector.transform.DOKill();
        _isTracking = true;
    }

    public void DisableTracking()
    {
        //_handEffector.transform.DOKill();
        _handEffector.DOLocalMove(_blockingPose.Default.position, _duration)
            .SetUpdate(UpdateType.Normal);
        _handEffector.DOLocalRotate(_blockingPose.Default.rotation.eulerAngles, _duration)
            .SetUpdate(UpdateType.Normal);
        _currentState = States.Idle;
        _isTracking = false;
        
    }
    
    public void SetTarget(GameObject target)
    {
        _target = target;
        _hasTarget = true;
    }

    public void RemoveTarget()
    {
        _target = null;
        _hasTarget = false;
    }
}
