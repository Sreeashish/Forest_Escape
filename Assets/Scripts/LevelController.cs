using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Levels { Level1, Level2, Level3, Level4 };
public class LevelController : MonoBehaviour
{
    public Levels level;
    public List<Interactable> interactables;
    public MeshRenderer water;
    public bool onBoardingCompleted;
    public Transform respawnPoint, markerUIParent;
    public GameObject interactionMarkerPrefab;

    Vector2 waterMoveFrom = new Vector2(0, 0.5f);
    Vector2 waterMoveTo = new Vector2(0, 0);

    private void Start()
    {
        TurnAllInteractablesOn();
        if (level == Levels.Level1)
        {
            TurnAllInteractablesOff();
            if (onBoardingCompleted)
            {
                TurnAllInteractablesOn();
            }
            StartCoroutine(WaterAnimation());
            OnboardingController.instance.StartCoroutine(OnboardingController.instance.StartOnBoarding());
        }
        SpawnInteractionMarkers();
    }

    private void Update()
    {
        UpdateMarkerPositions();
    }
    IEnumerator WaterAnimation()
    {
        while (true)
        {
            yield return null;
            if (water.material.mainTextureOffset == waterMoveTo)
            {
                water.material.DOOffset(waterMoveFrom, 5f);
            }
            if (water.material.mainTextureOffset == waterMoveFrom)
            {
                water.material.DOOffset(waterMoveTo, 5f);
            }
        }
    }

    public void TurnAllInteractablesOn()
    {
        for (int i = 0; i < interactables.Count; i++)
        {
            interactables[i].TurnInteraction(true);
        }
    }

    public void TurnAllInteractablesOff()
    {
        for (int i = 0; i < interactables.Count; i++)
        {
            interactables[i].TurnInteraction(false);
        }
    }

    void SpawnInteractionMarkers()
    {
        for (int i = 0; i < interactables.Count; i++)
        {
            GameObject marker = Instantiate(interactionMarkerPrefab, markerUIParent);
            interactables[i].interactionMarker = marker.GetComponent<RectTransform>();
            interactables[i].marker = marker.GetComponent<Image>();
            interactables[i].markerCanvas = marker.GetComponent<CanvasGroup>();
            interactables[i].MarkerState(false);
        }
    }

    void UpdateMarkerPositions()
    {
        for (int i = 0; i < interactables.Count; i++)
        {
            interactables[i].interactionMarker.position = Camera.main.WorldToScreenPoint(interactables[i].transform.position);
        }
    }
}
