using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Message Data", menuName = "Game Data/Message Data", order = 1)]
public class SO_Message : ScriptableObject
{
    public float Duration = 1f;
    public string Message = "";
    public SO_Message ChainedMessage;
}
