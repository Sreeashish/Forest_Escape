using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    public enum InteractionType { Jump, OpenPrison, Portal, HealthChest }
    public InteractionType interactionType;
    public bool isInteractable, oneTimeInterationOver;
    public Transform interactionItem, lookAtObject, customCameraPoint;
    public AudioClip interactionSound;
    public Light interactionLight;
    public ParticleSystem interactionParticle;
    public Image marker;
    public CanvasGroup markerCanvas;
    public RectTransform interactionMarker;
    public float lifeInChest;
    public IEnumerator OpenPrisonDoor()
    {
        Collider collider = interactionItem.GetComponent<Collider>();
        collider.enabled = false;
        interactionItem.transform.DOLocalMoveY(9, 2);
        interactionLight.enabled = false;
        yield return CommonScript.GetDelay(8);
        interactionItem.transform.DOLocalMoveY(6.41f, 2).SetEase(Ease.InOutBounce);
        yield return CommonScript.GetDelay(2);
        TurnInteraction(true);
        interactionLight.enabled = true;
        CreateMarker();
        collider.enabled = true;
        if (collider.bounds.Contains(PlayerController.instance.transform.position))
        {
            PlayerController.instance.StartCoroutine(PlayerController.instance.Death());
        }
    }

    public IEnumerator OpenChest()
    {
        yield return null;
        interactionParticle.Play();
        interactionItem.DORotate(new Vector3(0,0,0), 1);
        PlayerController.instance.StartCoroutine(PlayerController.instance.GainLife(lifeInChest));
    }

    public void TurnInteraction(bool on)
    {
        if (on)
            isInteractable = true;
        else
            isInteractable = false;
    }

    public void MarkerState(bool eKey)
    {
        if(!eKey)
        {
            interactionMarker.DOScale(0.5f,0);
            marker.sprite = UiController.instance.markerIcon;
        }
        else
        {
            interactionMarker.DOScale(2, 0);
            marker.sprite = UiController.instance.eKey;
        }
    }

    public void DisplayMarker(bool on)
    {
        if (on)
        {
            if (markerCanvas != null)
            {
                CommonScript.CanvasOn(markerCanvas);
            }
        }
        else
        {
            if (markerCanvas != null)
            {
                CommonScript.CanvasOff(markerCanvas);
            }
        }
    }

    public void DestroyMarker()
    {
        marker.gameObject.SetActive(false);
        MarkerState(false);
    }

    public void CreateMarker()
    {
        marker.gameObject.SetActive(true);
    }
}
