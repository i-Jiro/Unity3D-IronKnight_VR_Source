using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public Image image;

    public void FadeIn(float duration)
    {
        DOVirtual.Float(0, 1, duration, SetImageAlpha)
            .SetEase(Ease.Linear);
    }

    public void FadeOut(float duration)
    {
        DOVirtual.Float(1, 0, duration, SetImageAlpha)
            .SetEase(Ease.Linear);
    }

    public void SetImageAlpha(float value)
    {
        var color = image.color;
        color.a = value;
        image.color = color;
    }
}
