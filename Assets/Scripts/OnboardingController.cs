using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnboardingController : MonoBehaviour
{
    public enum OnBoardingPhase { Phase0, Phase1, Phase2, Phase3, Phase4 };

    public CanvasGroup onBoardingCanvas;
    public RectTransform onboardingTab;
    public Image icon, icon2, icon3, icon4;
    public TMP_Text instructiontext;    
    public OnBoardingPhase phase;

    public static OnboardingController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void ShowOnboarding(Sprite sprite, string instruction, bool extra = false, Sprite sprite2 = null, Sprite sprite3 = null, Sprite sprite4 = null)
    {
        ExtraIconsVisibility(false);
        CommonScript.CanvasOn(onBoardingCanvas);
        onBoardingCanvas.transform.DOLocalMoveX(1, 1).SetEase(Ease.InOutBounce);
        icon.sprite = sprite;
        icon.preserveAspect = true;
        instructiontext.text = instruction;
        if (extra)
        {
            ExtraIconsVisibility(true);
            icon2.sprite = sprite2;
            icon2.preserveAspect = true;
            icon3.sprite = sprite3;
            icon3.preserveAspect = true;
            icon4.sprite = sprite4;
            icon4.preserveAspect = true;
        }
    }

    public IEnumerator HideOnboarding()
    {
        yield return null;
        onBoardingCanvas.transform.DOLocalMoveX(-500, 1);
        yield return CommonScript.GetDelay(1);
        CommonScript.CanvasOff(onBoardingCanvas);
        icon.sprite = null;
        instructiontext.text = null;
        ExtraIconsVisibility(false);
    }

    public IEnumerator StartOnBoarding()
    {
        if (!PlayerController.instance.levelController.onBoardingCompleted && phase == OnBoardingPhase.Phase0)
        {
            PlayerController.instance.TurnMovementOnorOff(false);
            PlayerController.instance.ToggleMouseControlOnorOff(true);
            ShowOnboarding(UiController.instance.mouseIcon, "To Look Around");
            yield return CommonScript.GetDelay(5);
            yield return StartCoroutine(HideOnboarding());
            PlayerController.instance.ToggleControlsOnorOff(true);
            yield return CommonScript.GetDelay(1);

            ShowOnboarding(UiController.instance.wKey, "To Move Around", true, UiController.instance.aKey, UiController.instance.sKey, UiController.instance.dKey);
            phase = OnBoardingPhase.Phase1;
            PlayerController.instance.levelController.TurnAllInteractablesOn();
            yield return CommonScript.GetDelay(5);
            yield return StartCoroutine(HideOnboarding());
            yield return CommonScript.GetDelay(1);
            yield break;
        }
        if (!PlayerController.instance.levelController.onBoardingCompleted && phase == OnBoardingPhase.Phase1)
        {
            ShowOnboarding(UiController.instance.eKey, "To Interact");
            phase = OnBoardingPhase.Phase2;
            yield return CommonScript.GetDelay(5);
            yield return StartCoroutine(HideOnboarding());
            yield return CommonScript.GetDelay(1);
            yield break;
        }
        if (!PlayerController.instance.levelController.onBoardingCompleted && phase == OnBoardingPhase.Phase2)
        {
            print("Start Phase3");
            yield break;
        }
    }

    void ExtraIconsVisibility(bool visible)
    {
        if (visible)
        {
            icon2.gameObject.SetActive(true);
            icon3.gameObject.SetActive(true);
            icon4.gameObject.SetActive(true);
        }
        else
        {
            icon2.gameObject.SetActive(false);
            icon3.gameObject.SetActive(false);
            icon4.gameObject.SetActive(false);
        }
    }
}
