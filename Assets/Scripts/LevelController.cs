using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Levels { Level1, Level2, Level3, Level4 };
public class LevelController : MonoBehaviour
{
    public Levels level;
    public List<Interactable> interactables;
    public MeshRenderer water;
    public bool onBoardingCompleted;
    public Transform respawnPoint;
    public KeyCode respawnButton;

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
    }

    private void Update()
    {
        InteractableMarkerLookAts();
        PlayerRespawn();
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
            interactables[i].isInteractable = true;
        }
    }

    public void TurnAllInteractablesOff()
    {
        for (int i = 0; i < interactables.Count; i++)
        {
            interactables[i].isInteractable = false;
        }
    }

    void InteractableMarkerLookAts()
    {
        for (int i = 0; i < interactables.Count; i++)
        {
            if (interactables[i].marker != null)
                interactables[i].marker.transform.LookAt(PlayerController.instance.cameraTransform);
        }
    }

    void PlayerRespawn()
    {
        if (PlayerController.instance.playerState == PlayerController.PlayerState.Dead)
        {
            if (Input.GetKeyDown(respawnButton))
            {
                PlayerController.instance.ResetPlayer(respawnPoint.position);
                UiController.instance.StartCoroutine(UiController.instance.ResetUI());
            }
        }
    }
}
