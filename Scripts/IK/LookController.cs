using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RootMotion.FinalIK;
using UnityEngine;

//Controls Look and Aim effectors to animate IK.
[RequireComponent(typeof(LookAtIK))]
[RequireComponent(typeof(AimIK))]
public class LookController : MonoBehaviour
{
    [SerializeField] private Transform _lookAtEffector;
    [SerializeField] private Transform _aimAtEffector;
    private GameObject _currentAimTarget;
    private bool _hasAimTarget;
    private GameObject _currentLookTarget;
    private bool _hasLookTarget;

    private LookAtIK _lookAtIK;
    private AimIK _aimIK;

    [Range(0f,1f)][SerializeField]
    private float _lookAtWeight = 0.5f;
    [Range(0f,1f)][SerializeField]
    private float _aimAtWeight = 1.0f;

    private Tween _tween;

    private void Awake()
    {
        _lookAtIK = GetComponent<LookAtIK>();
        _aimIK = GetComponent<AimIK>();
    }

    private void Start()
    {
        if(_lookAtEffector == null)
            Debug.LogWarning("Look Effector field was not set!");
        if(_aimAtEffector == null)
            Debug.LogWarning("Aim Effector field was not set!");
        _lookAtIK.solver.IKPositionWeight = _lookAtWeight;
        _aimIK.solver.IKPositionWeight = _aimAtWeight;
    }

    public void LookAt(GameObject target)
    {
        if (target == _currentLookTarget) return;
        _currentLookTarget = target;
        _hasLookTarget = _currentLookTarget != null;
        if (!_hasLookTarget)
        {
            DOTween.To(_lookAtIK.solver.SetIKPositionWeight, _lookAtIK.solver.IKPositionWeight, 0, 0.25f);
        }
        else
        {
            DOTween.To(_lookAtIK.solver.SetIKPositionWeight, _lookAtIK.solver.IKPositionWeight, _lookAtWeight, 0.25f);
        }
        
    }

    public void AimAt(GameObject target)
    {
        if (target == _currentAimTarget) return;
        _currentAimTarget = target;
        _hasAimTarget = _currentAimTarget != null;
        if (!_hasAimTarget)
        {
            DOTween.To(_aimIK.solver.SetIKPositionWeight, _aimIK.solver.IKPositionWeight, 0, 0.25f);
        }
        else
        {
            DOTween.To(_aimIK.solver.SetIKPositionWeight, _aimIK.solver.IKPositionWeight, _aimAtWeight, 0.25f);
        }
    }

    //TODO: Slight jitterness, try waiting for tween to finish before updating position again.
    //TODO: Tween in the At methds and set the target object in the IK components on tween complete. Optimization.
    private void Update()
    {
        if (_hasLookTarget)
        {
            _tween =
            _lookAtEffector.transform.DOMove(_currentLookTarget.transform.position, 0.5f)
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Normal);
        }
        
        if (_hasAimTarget)
        {
            _tween =
            _aimAtEffector.transform.DOMove(_currentAimTarget.transform.position, 0.5f)
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Normal);
        }
    }
}
