using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RootMotion.Dynamics;
using UnityEngine;
using RootMotion.FinalIK;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class TwoHandWeaponHold : MonoBehaviour
{
	[SerializeField] private Transform _rightHandTarget;
    [Tooltip("The left hand target parented to the right hand.")]
    public Transform leftHandTarget;
    [Tooltip("Left hand poser (poses fingers to match the left hand target).")]
    public Poser leftHandPoser;
    
	[FormerlySerializedAs("bodyIK")] [SerializeField] private FullBodyBipedIK _bodyIK;
	[FormerlySerializedAs("aimIK")] [SerializeField] private AimIK _aimIK;
	[SerializeField] private LookAtIK _lookAtIK;
	[SerializeField] private GrounderFBBIK _grounderIK;
	[SerializeField] private PuppetMaster _puppetMaster;

	public bool AimEnabled = false;
	public bool LookEnabled = false;
	
	[Header("Weights")]
	[Range(0f, 1f)] public float weight = 1f;
    [Tooltip("The weight of pinning the left hand to the prop.")] [Range(0f, 1f)]
    public float leftHandWeight = 1f;
	[Range(0f,1f)]
    public float rotationWeight = 0.5f;

	private Vector3 targetPosRelativeToRight;
	private Quaternion targetRotRelativeToRight;

	[FormerlySerializedAs("aimTarget")] [Header("Targets")][SerializeField]
	private Transform _aimTarget;
	[FormerlySerializedAs("lookTarget")][SerializeField]
	private Transform _lookTarget;

	void Start()
	{
		if(_bodyIK == null)
			_bodyIK = GetComponent<FullBodyBipedIK>();
		if(_aimIK == null)
			_aimIK = GetComponent<AimIK>();
		if (_lookAtIK == null)
			_lookAtIK = GetComponent<LookAtIK>();
		if (_grounderIK == null)
			_grounderIK = GetComponent<GrounderFBBIK>();


		if(_bodyIK != null)
			_bodyIK.enabled = false;
		if(_aimIK != null)
			_aimIK.enabled = false;
		if (_grounderIK != null)
			_grounderIK.enabled = false;
		if (_lookAtIK != null)
			_lookAtIK.enabled = false;

		_bodyIK.solver.rightHandEffector.target = _rightHandTarget;

		// Get a call from FBBIK each time it has finished updating
		_bodyIK.solver.OnPostUpdate += AfterFBBIKSolve;

		_puppetMaster.OnRead += OnPuppetMasterWrite;
		_puppetMaster.OnFixTransforms += OnPuppetMasterFixTransforms;
		
		if (_bodyIK.solver.rightHandEffector.target == null && _rightHandTarget == null) 
			Debug.LogError("Right Hand Effector needs a Target");

	}
	
	void OnPuppetMasterFixTransforms() {
		if (!enabled) return;

		_aimIK.solver.FixTransforms();
		_lookAtIK.solver.FixTransforms();
		_bodyIK.solver.FixTransforms();
	}

	//Solve IK after Physics.
	void OnPuppetMasterWrite()
	{
		if (!enabled) return;
		if (AimEnabled)
		{
			_aimIK.solver.IKPosition = _aimTarget.position;
			_aimIK.solver.Update();
		}
		
		// Get the position/rotation of the left hand target relative to the right hand.
		targetPosRelativeToRight = _bodyIK.references.rightHand.InverseTransformPoint(leftHandTarget.position);
		targetRotRelativeToRight = Quaternion.Inverse(_bodyIK.references.rightHand.rotation) * leftHandTarget.rotation;

		// Set the position/rotation of the left hand target relative to the right hand effector target.
		_bodyIK.solver.leftHandEffector.rotation = _bodyIK.solver.rightHandEffector.target.rotation * targetRotRelativeToRight;
		_bodyIK.solver.leftHandEffector.position = _bodyIK.solver.rightHandEffector.target.position + _bodyIK.solver.rightHandEffector.target.rotation * targetPosRelativeToRight;

		// Weights
		_bodyIK.solver.rightHandEffector.positionWeight = weight;
		_bodyIK.solver.rightHandEffector.rotationWeight = rotationWeight * weight;
		_bodyIK.solver.leftHandEffector.rotationWeight = rotationWeight * weight;

		float wL = leftHandWeight * weight;
		_bodyIK.solver.leftHandEffector.positionWeight = wL;
		leftHandPoser.weight = wL;
		
		_bodyIK.solver.Update();
		_grounderIK.solver.Update();

		if (LookEnabled)
		{
			_lookAtIK.solver.IKPosition = _lookTarget.position;
			_lookAtIK.solver.Update();
		}
	}
	
	void FixedUpdate()
	{
		foreach (Muscle m in _puppetMaster.muscles) if (m.rigidbody.IsSleeping()) m.rigidbody.WakeUp();
	}
	
    // NOTE: Any changes to done to IK effectors needs to be before LateUpdate. Coroutines not plausible (Maybe?).
    void AfterFBBIKSolve()
    {
		// Rotate the hand bones to effector.rotation directly instead of using effector.rotationWeight that might fail to get the limb bending right under some circumstances
		_bodyIK.solver.leftHandEffector.bone.rotation = Quaternion.Slerp(_bodyIK.solver.leftHandEffector.bone.rotation, _bodyIK.solver.leftHandEffector.rotation, leftHandWeight * weight);
		_bodyIK.solver.rightHandEffector.bone.rotation = Quaternion.Slerp(_bodyIK.solver.rightHandEffector.bone.rotation, _bodyIK.solver.rightHandEffector.rotation, weight);
	}

    public void ReleaseLeftHand()
    {
	    DOVirtual.Float(1f, 0f, 0.25f, SetLeftHandWeight);
    }

    public void TightenLeftHand()
    {
	    DOVirtual.Float(0f, 1f, 0.25f, SetLeftHandWeight);
    }

    public void SetWeight(float weight, float duration)
    {
	    var currentWeight = this.weight;
	    DOVirtual.Float(currentWeight, weight, duration, UpdateWeight);
    }

    private void UpdateWeight(float weight)
    {
	    this.weight = weight;
    }

    private void SetLeftHandWeight(float value)
    {
	    leftHandWeight = value;
    }

	// Clean up the delegate
	void OnDestroy() 
	{
		if (_bodyIK != null) _bodyIK.solver.OnPostUpdate -= AfterFBBIKSolve;
		if (_puppetMaster != null)
		{
			_puppetMaster.OnWrite -= OnPuppetMasterWrite;
			_puppetMaster.OnFixTransforms -= OnPuppetMasterFixTransforms;
		}
	}
}
