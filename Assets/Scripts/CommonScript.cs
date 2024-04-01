using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public static class CommonScript
{
    public static WaitForSeconds GetDelay(float seconds)
    {
        return new WaitForSeconds(seconds);
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static IEnumerator UntilNear( Transform applicant, Transform destination)
    {
        while (Vector3.Distance(applicant.position, destination.position) >= 1)
        {
            yield return null;
        }
    }

    public static void Delay(float delayTime)
    {
        MakeDelay(delayTime);
    }

    public static IEnumerator MakeDelay(float delay)
    {
        float t = 0;
        while(t < delay)
        {
            yield return null;
            t += Time.deltaTime;
        }
    }

    public static void CanvasOn(CanvasGroup canvas)
    {
        canvas.alpha = 1;
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
    }

    public static void CanvasOff(CanvasGroup canvas)
    {
        canvas.alpha = 0;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
    }
}
