using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TestInteractions : MonoBehaviour
{
    public InteractionType interactionType;
    public GameObject interactionItem;

    public void PerformInteraction()
    {
        switch (interactionType)
        {
            case InteractionType.OpenPrison:
            StartCoroutine(OpenDoor());
            break;
        }
    }

    IEnumerator OpenDoor()
    {
        Collider collider = interactionItem.GetComponent<Collider>();
        collider.enabled = false;
        interactionItem.transform.DOLocalMoveY(9, 2);
        yield return CommonScript.GetDelay(50);
        interactionItem.transform.DOLocalMoveY(6.41f, 2).SetEase(Ease.InOutBounce);
        yield return CommonScript.GetDelay(2);
        collider.enabled = true;
    }
}
