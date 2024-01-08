using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimation;
using DG.Tweening;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
   public PlayerController Player;
   public float UnitsMultiplier = 5f;
   
   [Header("Health Meter")]
   [SerializeField] private TextMeshPro _healthText;
   [SerializeField] private Image _healthBar;
   [Tooltip("How long it takes to countdown/up to updated HP.")]
   [SerializeField] private float _healthDeductDuration = 1f;

   private float _maxHealth = 0f;
   private float _lastHealth = 0f;
   
   private AudioSource _audioSource;

   [Header("Speedometer")]
   [SerializeField] private TextMeshPro _speedoMeterText;
   [SerializeField] private Image _speedFillBar;
   [Tooltip("In meters.")]
   [SerializeField] private float _maxSpeed = 5f;

   [Header("Message Box")]
   [SerializeField] private Image _messageBox;
   [SerializeField] private TextMeshPro _messageText;
   [Range(0f,1f)]
   [SerializeField] private float _maxAlpha = 0.85f;
   [SerializeField] private float _transitionTime = 0.75f;
   private Queue<SO_Message> _messageQueue = new Queue<SO_Message>();
   public AudioClip MessageRecievedClip;

   public void Awake()
   {
      _audioSource = GetComponent<AudioSource>();
      if (Player) return;
      Player = Camera.main.GetComponentInParent<PlayerController>();
      if(!Player)
         Debug.LogWarning("Could not find player controller!");
   }

   public void Start()
   {
      if (!Player){gameObject.SetActive(true); return;}
      
      Player.HealthHandler.OnDamage += UpdateHealth;
      _lastHealth = Player.HealthHandler.StartingHealthPoints;
      _maxHealth = _lastHealth;
      ChangeHealthValues((int)_maxHealth);
      _messageBox.gameObject.SetActive(false);
   }
   
   //Update and display speed
   public void FixedUpdate()
   {
      UpdateSpeedometer();
   }

   public void DisplayMesssage(string text, float duration)
   {
      _messageText.text = text;
      _messageBox.gameObject.SetActive(true);
      _audioSource.PlayOneShot(MessageRecievedClip);
      DOVirtual.Float(0, _maxAlpha, _transitionTime, ChangeMessageAlpha)
         .OnComplete(() => { Timing.RunCoroutine(_MessageDuration(duration),Segment.SlowUpdate); });
   }

   public void DisplayMessage(SO_Message data)
   {
      _messageText.text = data.Message;
      _messageBox.gameObject.SetActive(true);
      _audioSource.PlayOneShot(MessageRecievedClip);
      if (data.ChainedMessage != null)
      {
         _messageQueue.Enqueue(data.ChainedMessage);
      }
      
      DOVirtual.Float(0, _maxAlpha, _transitionTime, ChangeMessageAlpha)
         .OnComplete(() => { Timing.RunCoroutine(_MessageDuration(data.Duration),Segment.SlowUpdate); });
   }
   
   public void EndMessage()
   {
      DOVirtual.Float(_maxAlpha, 0, _transitionTime, ChangeMessageAlpha)
         .OnComplete(() =>
         {
            _messageBox.gameObject.SetActive(false);
            if (_messageQueue.TryDequeue(out var message))
            {
               DisplayMessage(message);
            }
         });
   }
   
   private IEnumerator<float> _MessageDuration(float duration)
   {
      yield return Timing.WaitForSeconds(duration);
      EndMessage();
   }
   
   private void ChangeMessageAlpha(float alpha)
   {
      Color tempBoxColor = _messageBox.color;
      tempBoxColor.a = alpha;
      Color tempTextColor = _messageText.color;
      _messageBox.color = tempBoxColor;
      tempTextColor.a = alpha;
      _messageText.color = tempTextColor;
   }

   private void UpdateSpeedometer()
   {
      var speed = Player.Body.LocoBall.velocity.magnitude;
      speed = (speed * UnitsMultiplier);

      _speedoMeterText.text = speed switch
      {
         < 100 and >= 10 => "0" + ((int)(speed)).ToString(),
         < 10 => "00" + ((int)(speed)).ToString(),
         _ => ((int)(speed)).ToString()
      };

      //TODO: Find correct unit scale.
      _speedFillBar.fillAmount = speed  / (_maxSpeed * UnitsMultiplier);
   }
   
   private void UpdateHealth()
   {
      var newHealth = Player.HealthHandler.CurrentHealthPoints;
      DOVirtual.Int((int)_lastHealth, (int)newHealth, _healthDeductDuration, ChangeHealthValues);
      _lastHealth = newHealth;
   }
   
   private void ChangeHealthValues(int value)
   {
      _healthText.text = value.ToString();
      _healthBar.fillAmount = value / _maxHealth;
   }

   private void OnDestroy()
   {
      if (Player)
      {
         Player.HealthHandler.OnDamage -= UpdateHealth;
      }
   }
}
