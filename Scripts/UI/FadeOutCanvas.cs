using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FadeOutCanvas : MonoBehaviour
{
    public CanvasGroup canvas;
    public float Duration = 3f;
    private float _currentValue = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        DOVirtual.Float(_currentValue, 0, Duration, UpdateAlpha);

    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        DOVirtual.Float(_currentValue, 1, Duration, UpdateAlpha);
    }

    public void UpdateAlpha(float value)
    {
        _currentValue = value;
        canvas.alpha = value;
    }
}
