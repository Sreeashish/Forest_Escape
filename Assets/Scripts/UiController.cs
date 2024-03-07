using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public CanvasGroup HUDCanvas, cinematicCanvas, deathScreen, blackScreen;
    public Image lifeBar, CinematicTopBar, CinematicBottomBar;
    public RectTransform respawnTextHolder;

    public static UiController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        CalculateLife();
        RemoveDeathScreen();
    }

    public IEnumerator BringCinematicFramesIn()
    {
        yield return null;
        CommonScript.CanvasOn(cinematicCanvas);
        CommonScript.CanvasOff(HUDCanvas);
        CinematicTopBar.rectTransform.DOAnchorPosY(-22, 1);
        CinematicBottomBar.rectTransform.DOAnchorPosY(22, 1);
    }

    public IEnumerator BringCinematicFramesOut()
    {
        yield return null;
        CinematicTopBar.rectTransform.DOAnchorPosY(22, 0.5f);
        CinematicBottomBar.rectTransform.DOAnchorPosY(-22, 0.5f);
        yield return CommonScript.GetDelay(0.5f);
        CommonScript.CanvasOff(cinematicCanvas);
        CommonScript.CanvasOn(HUDCanvas);
    }

    public void CalculateLife()
    {
        lifeBar.fillAmount = CommonScript.Remap(PlayerController.instance.life, 0, 100, 0, 1);
    }

    public void DeathScreen()
    {
        deathScreen.DOFade(1, 1.5f);
        respawnTextHolder.DOScaleY(0, 0);
        respawnTextHolder.DOScaleY(1.5f, 1.5f).SetEase(Ease.Linear);

    }

    public void RemoveDeathScreen()
    {
        deathScreen.DOFade(0, 0);
    }

    public IEnumerator ResetUI()
    {
        RemoveDeathScreen();
        StartCoroutine(BringCinematicFramesOut());
        CalculateLife();
        yield return CommonScript.GetDelay(0.8f);
        BlackScreenOut();
    }

    public void BlackScreenIn()
    {
        blackScreen.DOFade(1, 0);
    }

    void BlackScreenOut()
    {
        blackScreen.DOFade(0, 1);
    }
}
