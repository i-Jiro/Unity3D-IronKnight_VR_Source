using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MEC;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class InterceptAttack : MonoBehaviour
{
   public bool DebugMode = false;
   public bool isActive = true;
   public Transform HandEffector;

   [SerializeField] private int totalBlockPoints = 10;
   [SerializeField] private Transform _startBlockPoint;
   [SerializeField] private Transform _endBlockPoint;
   [SerializeField] private Vector3 _rotateOnAxis = new Vector3(0, 0, 0);
   [SerializeField] float _interceptSpeed = 10.0f;
   private readonly List<Vector3> _blockPoints = new List<Vector3>();
   private readonly Dictionary<InterceptData, float> _interceptDict = new Dictionary<InterceptData, float>();

   [Header("Tracking Circle Range")] [SerializeField]
   private Vector3 _trackingCenter = Vector3.zero;

   [SerializeField] private float _trackingRadius = 1f;

   private Vector3 _startingPosition;
   private Quaternion _startingRotation;

   private SphereCollider _sphereCollider;

   private bool _isIntercepting = false;
   private Vector3 _activeInterceptPoint;
   private Vector3 _activeBlockPoint;

   private Vector3 _blockPointToHandRelative;
   private Quaternion _targetRotation;

   private GameObject _target;

   public UnityEvent<GameObject> OnTargetFound;
   public UnityEvent OnTargetLost;

   public delegate void InterceptStartEventHandler();

   public event InterceptStartEventHandler OnInterceptStart;

   public delegate void InterceptEndEventHandler();

   public event InterceptEndEventHandler OnInterceptEnd;

   private struct InterceptData
   {
      public Vector3 BlockPoint;
      public Vector3 InterceptPoint;
   }

   private void Awake()
   {
      _sphereCollider = GetComponent<SphereCollider>();
   }

   private void OnDrawGizmos()
   {
      Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
      if (!DebugMode) return;

      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(_trackingCenter, _trackingRadius);


      if (_sphereCollider != null)
      {
         Gizmos.color = Color.green;
         Gizmos.DrawWireSphere(_sphereCollider.center, _sphereCollider.radius);
      }

      if (HandEffector != null)
      {
         Gizmos.color = Color.blue;
         Gizmos.DrawCube(transform.InverseTransformPoint(HandEffector.position), Vector3.one * 0.05f);
      }

      Gizmos.color = Color.red;
      Gizmos.DrawSphere(transform.InverseTransformPoint(_activeInterceptPoint), 0.025f);

      if (_blockPoints.Count < 0) return;
      foreach (var point in _blockPoints)
      {
         Gizmos.color = _activeBlockPoint != point ? Color.yellow : Color.green;
         Gizmos.DrawSphere(transform.InverseTransformPoint(point), 0.025f);
      }
   }

   private void Update()
   {
      //TODO: Overlap sphere does not match Gizmo sphere
      var colliders = Physics.OverlapSphere(transform.TransformPoint(_trackingCenter), _trackingRadius);

      bool targetFound = false;
      foreach (var collider in colliders)
      {
         if (collider.gameObject.CompareTag("Weapon"))
         {
            targetFound = true;
            SetTarget(collider.gameObject);
            if (DebugMode)
               Debug.DrawLine(collider.transform.position, transform.position, Color.red);
         }
      }

      if (targetFound == false)
      {
         RemoveCurrentTarget();
      }
   }

   //TODO: Instead of trigger, activate interception based on raycast distance to incoming weapon.
   private void OnTriggerEnter(Collider other)
   {
      if (!isActive) return;
      if (_isIntercepting) return;
      if (other.CompareTag("Weapon"))
      {
         var otherRB = other.GetComponentInParent<Rigidbody>();
         var otherVelocity = otherRB.velocity;
         var otherPosition = other.transform.position;
         OnInterceptStart?.Invoke();
         _isIntercepting = true;
         if (TryFindIntercept(otherPosition, otherVelocity, out var fastestIntercept))
         {
            Intercept(fastestIntercept);
         }
         else
         {
            //Attempt to block with just given entry point of trigger zone.
            //TODO: Attempt to block here with random values.
            var closestBlockPoint = Vector3.zero;
            var shortestDistance = Mathf.Infinity;
            //Find the closest blockpoint to the incoming weapon.
            foreach (var point in _blockPoints)
            {
               Vector3 blockPointToOther = otherPosition - point;
               var distance = blockPointToOther.sqrMagnitude;
               if (distance < shortestDistance)
               {
                  closestBlockPoint = point;
                  shortestDistance = distance;
               }
            }

            var blockPointToHandRelative = HandEffector.position - closestBlockPoint;
            var handDestinationPos = otherPosition + blockPointToHandRelative;
            HandEffector.transform.DOMove(handDestinationPos, Random.Range(1f, _interceptSpeed))
               .SetSpeedBased(true)
               .OnComplete(() => { OnInterceptEnd?.Invoke(); });
            Debug.Log("No intercept found. Attempting to block.");
            Timing.RunCoroutine(ResetPosition(1.5f));
         }

      }
   }

   private void SetTarget(GameObject target)
   {
      OnTargetFound.Invoke(target);
   }

   private void RemoveCurrentTarget()
   {
      OnTargetLost.Invoke();
   }

   //Creates a line of block point references along the weapon.
   private void InitializeBlockPoints()
   {
      _blockPoints.Clear();
      Vector3 direction = (_endBlockPoint.position - _startBlockPoint.position);
      float distance = direction.magnitude;
      float spacing = distance / totalBlockPoints;
      float nextPosition = 0f;
      for (int i = 0; i < totalBlockPoints; i++)
      {
         if (i == 0)
         {
            nextPosition = spacing;
         }
         else
         {
            nextPosition += spacing;
         }

         var blockPosition = _startBlockPoint.transform.position + (direction.normalized * nextPosition);
         _blockPoints.Add(blockPosition);
      }
   }

   //Find viable intercepts points. Outputs fastest intercept out of the viable points.
   private bool TryFindIntercept(Vector3 incomingPosition, Vector3 incomingVelocity, out InterceptData fastestIntercept)
   {
      _interceptDict.Clear();
      //Snapshot block points across weapon.
      InitializeBlockPoints();
      foreach (var point in _blockPoints)
      {
         if (!FindInterceptPoint(incomingPosition,
                point,
                incomingVelocity,
                _interceptSpeed,
                out var interceptPoint,
                out var totalTime))
            continue;

         var data = new InterceptData
         {
            BlockPoint = point,
            InterceptPoint = interceptPoint
         };

         _interceptDict.Add(data, totalTime);
      }

      //No intercept found.
      if (_interceptDict.Count <= 0)
      {
         fastestIntercept = new InterceptData();
         fastestIntercept.BlockPoint = Vector3.zero;
         fastestIntercept.InterceptPoint = Vector3.zero;
         return false;
      }

      fastestIntercept = _interceptDict.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
      _activeInterceptPoint = fastestIntercept.InterceptPoint;
      _activeBlockPoint = fastestIntercept.BlockPoint;
      return true;
   }

   //Move hand effector towards given intercept point.
   private void Intercept(InterceptData data)
   {
      _blockPointToHandRelative = HandEffector.position - _activeBlockPoint;
      var newHandPos = _activeInterceptPoint + _blockPointToHandRelative;

      HandEffector.transform.DOMove(newHandPos, _interceptSpeed)
         .SetSpeedBased(true)
         .SetUpdate(UpdateType.Normal)
         .OnComplete(() => Timing.RunCoroutine(ResetPosition(1.5f), Segment.Update));
      //Debug.Log($"Total time: {_interceptDict[data]}");
   }

   //Reset to starting position for testing.
   IEnumerator<float> ResetPosition(float time)
   {
      yield return Timing.WaitForSeconds(time);
      _isIntercepting = false;
      OnInterceptEnd?.Invoke();
   }


   // a: other pos, b block point, vA,
   private bool FindInterceptPoint(Vector3 a, Vector3 b, Vector3 vA, float sB,
      out Vector3 interceptPoint, out float time)
   {
      var aToB = b - a;
      var dC = aToB.magnitude;
      var alpha = Vector3.Angle(aToB, vA) * Mathf.Deg2Rad;
      var sA = vA.magnitude;
      var r = sA / sB;
      if (InterceptMath.SolveQuadratic(1 - r * r, 2 * r * dC * Mathf.Cos(alpha), -(dC * dC), out var root1,
             out var root2) == 0)
      {
         interceptPoint = Vector3.zero;
         time = 0f;
         return false;
      }

      var dA = Mathf.Max(root1, root2);
      var t = dA / sB;
      var c = a + vA * t;

      interceptPoint = c;
      time = t;
      return true;
   }
}


public static class InterceptMath
{
   public static int SolveQuadratic(float a, float b, float c, out float root1, out float root2)
   {
      var discriminant = (b * b) - (4 * a * c);
      if (discriminant < 0)
      {
         root1 = Mathf.Infinity;
         root2 = -root1;
         return 0;
      }

      root1 = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
      root2 = (-b - Mathf.Sqrt(discriminant)) / (2 * a);

      return discriminant > 0 ? 2 : 1;
   }
}
