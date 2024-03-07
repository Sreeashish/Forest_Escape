using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    public enum InteractionType { Jump, OpenPrison, Portal }
    public int portalID;
    public InteractionType interactionType;
    public bool isInteractable;
    public Transform interactionItem, lookAtObject;
    public AudioClip interactionSound;
    public Light interactionLight;
    public Image marker;
    public CanvasGroup markerCanvas;
    public IEnumerator OpenPrisonDoor()
    {
        interactionItem.transform.DOLocalMoveY(9, 2);
        interactionLight.enabled = false;
        yield return CommonScript.GetDelay(8);
        interactionItem.transform.DOLocalMoveY(6.41f, 2).SetEase(Ease.InOutBounce);
        TurnInteractionOn();
        interactionLight.enabled = true;
        CreateMarker();
    }

    public void TurnInteractionOn()
    {
        isInteractable = true;
    }

    public void TurnInteractionOff()
    {
        isInteractable = false;
    }

    public void DisplayMarker()
    {
        if (markerCanvas != null)
        {
            CommonScript.CanvasOn(markerCanvas);
        }
    }

    public void TurnOffMarker()
    {
        if (markerCanvas != null)
        {
            CommonScript.CanvasOff(markerCanvas);
        }
    }

    public void DestroyMarker()
    {
        marker.gameObject.SetActive(false);
    }

    public void CreateMarker()
    {
        marker.gameObject.SetActive(true);
    }
}
