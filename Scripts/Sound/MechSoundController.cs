using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;


public class MechSoundController : MonoBehaviour
{
    [SerializeField] private AudioSource _feetAudioSource;
    [SerializeField] private List<AudioClip> _footStepClips;
    [SerializeField] private AudioClip _dodgeClip;

    //Called by animation events
    public void PlayFootStep()
    {
        int rand = Random.Range(0, _footStepClips.Count);
        _feetAudioSource.PlayOneShot(_footStepClips[rand]);
    }

    public void PlayDodge()
    {
        _feetAudioSource.PlayOneShot(_dodgeClip);
    }
}
