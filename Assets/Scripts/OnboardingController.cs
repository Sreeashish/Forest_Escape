using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnboardingController : MonoBehaviour
{
    public CanvasGroup onBoardingCanvas;
    public RectTransform onboardingTab;
    public Sprite mouseIcon, WASDIcon, eIcon;
    public Image icon;
    public TMP_Text instructiontext;
    public bool phase1done, phase2done;

    public static OnboardingController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void ShowOnboarding(Sprite sprite, string instruction)
    {
        CommonScript.CanvasOn(onBoardingCanvas);
        onBoardingCanvas.transform.DOLocalMoveX(1, 1).SetEase(Ease.InOutBounce);
        icon.sprite = sprite;
        instructiontext.text = instruction;
    }

    public IEnumerator HideOnboarding()
    {
        yield return null;
        onBoardingCanvas.transform.DOLocalMoveX(-500, 1);
        CommonScript.CanvasOff(onBoardingCanvas);
        icon.sprite = null;
        instructiontext.text = null;
    }

    public IEnumerator StartOnBoarding()
    {
        if (!PlayerController.instance.levelController.onBoardingCompleted && !phase1done)
        {
            PlayerController.instance.TurnMovementOnorOff(false);
            PlayerController.instance.ToggleMouseControlOnorOff(true);
            ShowOnboarding(mouseIcon, "To Look Around");
            yield return CommonScript.GetDelay(5);
            yield return StartCoroutine(HideOnboarding());
            PlayerController.instance.ToggleControlsOnorOff(true);
            yield return CommonScript.GetDelay(1);
            
            ShowOnboarding(WASDIcon, "To Move Around");
            phase1done = true;
            PlayerController.instance.levelController.TurnAllInteractablesOn();
            yield return CommonScript.GetDelay(5);
            yield return StartCoroutine(HideOnboarding());
            yield return CommonScript.GetDelay(1);
            yield break;
        }
        if(!PlayerController.instance.levelController.onBoardingCompleted && !phase2done)
        {
            ShowOnboarding(eIcon, "To Interact");
            phase2done = true;
            yield return CommonScript.GetDelay(5);
            yield return StartCoroutine(HideOnboarding());
            yield return CommonScript.GetDelay(1);
            yield break;
        }
    }
}
