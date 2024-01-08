using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;
    public string Name = "";
    public float startDelay = 5f;
    public float endDelay = 10f;
    public bool startMissionOnLoad = true;
    private bool _hasStarted = false;
    
    public UnityEvent OnMissionStart;
    public UnityEvent OnMissionComplete;

    [System.Serializable]
    public class Objective
    {
        public int id;
        public bool IsComplete;
        public UnityEvent OnComplete;
    }

    public List<Objective> Objectives;
    private Dictionary<int, Objective> ObjectivesDict = new Dictionary<int, Objective>();

    [Header("DEBUG")]
    [SerializeField] private float _completedObjectives;

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
    }

    private void Start()
    {
        _completedObjectives = 0;
        foreach (var objective in Objectives)
        {
            ObjectivesDict.Add(objective.id, objective);
        }
        GameManager.Instance.ChangePlayerMeshToMech();
        if (startMissionOnLoad)
            Timing.RunCoroutine(_StartRoutine());
    }

    public void CompleteObjective(int id)
    {
        if (ObjectivesDict.TryGetValue(id, out var objective))
        {
            objective.IsComplete = true;
            objective.OnComplete?.Invoke();
            _completedObjectives++;
        }
        else
        {
            Debug.Log($"Failed to complete objective with ID: {id}");
        }
        
        if (_completedObjectives >= ObjectivesDict.Count)
        {
            EndMission();
        }
    }
    

    public void StartMission()
    {
        if (_hasStarted) return;
        OnMissionStart?.Invoke();
    }

    public void EndMission()
    {
        Timing.RunCoroutine(_EndRoutine(), Segment.Update);
    }
    
    private IEnumerator<float> _StartRoutine()
    {
        yield return Timing.WaitForSeconds(startDelay);
        OnMissionStart?.Invoke();
    }

    private IEnumerator<float> _EndRoutine()
    {
        yield return Timing.WaitForSeconds(endDelay);
        GameManager.Instance.ChangePlayerMeshToHands();
        OnMissionComplete?.Invoke();
    }
}
