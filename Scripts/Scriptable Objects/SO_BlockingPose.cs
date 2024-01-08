using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "Blocking Pose Data", menuName = "NPC Data/Blocking Pose Data", order = 1)]
public class SO_BlockingPose : ScriptableObject
{
    [System.Serializable]
    public struct Quadrant
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public Quadrant UpperLeft;
    public Quadrant UpperRight;
    public Quadrant LowerLeft;
    public Quadrant LowerRight;
    [FormerlySerializedAs("MiddleTop")] public Quadrant MiddleTopLeft;
    public Quadrant MiddleTopRight;
    public Quadrant Default;
}
