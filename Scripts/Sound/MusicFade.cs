using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MusicFade : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeDuration = 1f;
    public float targetVolume = 0.25f;
    public bool PlayOnStart;

    // Start is called before the first frame update
    void Start()
    {
        if(!PlayOnStart) return;
        audioSource.Play();
        audioSource.volume = 0f;
        DOVirtual.Float(0, targetVolume, fadeDuration, UpdateVolume);
    }

    public void FadeInPlay()
    {
        DOVirtual.Float(0, targetVolume, fadeDuration, UpdateVolume);
        audioSource.Play();
    }
    
    public void FadeOutPlay()
    {
        DOVirtual.Float(targetVolume, 0, fadeDuration, UpdateVolume)
            .OnComplete(() => { audioSource.Stop();});
    }

    void UpdateVolume(float value)
    {
        audioSource.volume = value;
    }
}
