using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls the rate of when AI can attack the player.
public class AIDirector : MonoBehaviour
{
    public static AIDirector Instance;
    public bool DebugMode = false;
    
    public int maxAttackers = 4;
    [Tooltip("Seconds in between attacks between NPCs")]
    public float attackInterval = 10f;
    private float _attackCooldown;
    [SerializeField] private PlayerController _player;
    public PlayerController Player => _player;
    
    //NPC Pool
    [SerializeField] private List<NPCBrain> _npcBrains = new List<NPCBrain>();
    private Queue<NPCBrain> _attackQueue = new Queue<NPCBrain>();

    private void Awake()
    {
        //Singleton Instance.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("PlayerController").GetComponent<PlayerController>();
        _attackCooldown = attackInterval;
    }

    private void Update()
    {
        if(_attackQueue.Count <= 0) return;
        //TODO: Randmize time between attacks.
        if (_attackCooldown <= 0)
        {
            _attackQueue.Dequeue().SendEvent("CanAttack");
            _attackCooldown = attackInterval;
        }
        
        _attackCooldown -= Time.deltaTime;
    }

    public void RegisterNPC(NPCBrain npc)
    {
        _npcBrains.Add(npc);
    }

    public bool TryAddToAttackQueue(NPCBrain npc)
    {
        //NPC should only able to queue once.
        if (_attackQueue.Contains(npc)) return false;
        if (_attackQueue.Count >= maxAttackers) return false; //Potentially unnecessary check. 
        _attackQueue.Enqueue(npc);
        return true;
    }
}
