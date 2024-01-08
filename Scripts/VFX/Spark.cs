using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;

public class Spark : MonoBehaviour
{
    [SerializeField] private ParticleSystem _SparkVFX;

    private void OnEnable()
    {
        _SparkVFX.Play();
        Timing.RunCoroutine(_Tick());
    }

    void Complete()
    {
        PoolManager.ReleaseObject(this.gameObject);
    }

    private IEnumerator<float> _Tick()
    {
        while (_SparkVFX.isPlaying)
        {
            yield return Timing.WaitForOneFrame;
        }
        Complete();
    }
}
