using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public CanvasGroup HUDCanvas, cinematicCanvas, deathScreen, blackScreen;
    public Image lifeBar, CinematicTopBar, CinematicBottomBar, heartImage;
    public RectTransform respawnTextHolder;

    public static UiController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    Coroutine HeartBeatRoutine;
    private void Start()
    {
        CalculateLife(PlayerController.instance.life);
        RemoveDeathScreen();
        if(HeartBeatRoutine != null)
        {
            StopCoroutine(HeartBeatRoutine);
        }
        HeartBeatRoutine = StartCoroutine(HeartBeat());
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

    public void CalculateLife(float lifeValue)
    {
            lifeBar.fillAmount = CommonScript.Remap(lifeValue, 0, 100, 0, 1);
    }

    public IEnumerator FillFillbar(float from, float to)
    {
        float fill, timeElapsed = 0, time = 0.5f;
        while (timeElapsed < time)
        {
            yield return null;
            fill = Mathf.Lerp(from, to, timeElapsed);
            UiController.instance.CalculateLife(fill);
            timeElapsed += Time.deltaTime;
        }
    }

    public void DeathScreen()
    {
        deathScreen.DOFade(1, 1.5f);
        respawnTextHolder.DOScaleY(0, 0);
        respawnTextHolder.DOScaleY(1.5f, 1.5f).SetEase(Ease.Linear);

    }

    IEnumerator HeartBeat()
    {
        while (true)
        {
            yield return null;
            heartImage.transform.DOScale(1, 0);
            heartImage.transform.DOScale(0.8f, 0.5f);
            yield return CommonScript.GetDelay(0.8f);
            heartImage.transform.DOScale(1, 0.5f);
            yield return CommonScript.GetDelay(0.8f);
        }
    }

    public void RemoveDeathScreen()
    {
        deathScreen.DOFade(0, 0);
    }

    public IEnumerator ResetUI()
    {
        RemoveDeathScreen();
        StartCoroutine(BringCinematicFramesOut());
        CalculateLife(PlayerController.instance.life);
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
