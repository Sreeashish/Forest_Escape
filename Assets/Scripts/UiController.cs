using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    [Header("UI ELEMENTS & ATTRIBUTES")]
    public CanvasGroup HUDCanvas;
    public CanvasGroup cinematicCanvas, deathScreen, blackScreen, crosshairReticle, teleportationIndicator;
    public Image lifeBar, runicBar, CinematicTopBar, CinematicBottomBar, heartImage, crosshairReticleImage;
    public RectTransform respawnTextHolder;

    [Header("SPRITES")]
    public Sprite eKey;
    public Sprite markerIcon, wKey, aKey, sKey, dKey, mouseIcon, lMBIcon, rMBIcon, mMBIcon;

    [Header("WORLDSPACE UI ELEMENTS")]
    public Transform worldspaceUIElementsHolder;

    public static UiController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        CalculateLife(PlayerController.instance.life);
        RemoveDeathScreen();
        StartHeartBeat();
        if (PlayerController.instance.combatController.isTeleportable)
        {
            StartTeleportationIndicator();
        }
        else
        {
            StopTeleportationIndicator();
        }
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

    public void CalculateRunic(float runicValue)
    {
        runicBar.fillAmount = CommonScript.Remap(runicValue, 0, 3, 0, 1);
    }

    public IEnumerator FillFillbar(float from, float to, bool life, float duration = 0.7f)
    {
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            yield return null;
            float t = timeElapsed / duration;
            float fill = Mathf.Lerp(from, to, t);
            if (life)
            {
                CalculateLife(fill);
            }
            else
            {
                CalculateRunic(fill);
            }
            timeElapsed += Time.deltaTime;
        }

        if (life)
        {
            CalculateLife(to);
        }
        else
        {
            CalculateRunic(to);
        }
    }

    public void DeathScreen()
    {
        deathScreen.DOFade(1, 1.5f);
        respawnTextHolder.DOScaleY(0, 0);
        respawnTextHolder.DOScaleY(1.5f, 1.5f).SetEase(Ease.Linear);
    }

    public void Crosshair(bool turn)
    {
        if (turn)
        {
            crosshairReticle.DOFade(1, 0);
        }
        else if (!turn)
        {
            crosshairReticle.DOFade(0, 0);
        }
    }

    public void CrosshairState(RaycastHit hit, bool scan = false)
    {
        if (scan)
        {
            if (hit.collider.CompareTag("Enemy"))
                crosshairReticleImage.color = Color.red;
            else
                crosshairReticleImage.color = Color.white;
        }
        else
        {
            crosshairReticleImage.color = Color.white;
        }
    }

    Coroutine HeartBeatRoutine;
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
    void StartHeartBeat()
    {
        if (HeartBeatRoutine != null)
        {
            StopCoroutine(HeartBeatRoutine);
        }
        HeartBeatRoutine = StartCoroutine(HeartBeat());
    }
    void StopHeartBeat()
    {
        if (HeartBeatRoutine != null)
        {
            StopCoroutine(HeartBeatRoutine);
        }
    }


    Coroutine TeleportationIndicatorRoutine;
    IEnumerator TeleportationIndicator()
    {
        while (true)
        {
            yield return null;
            teleportationIndicator.DOFade(1, 1);
            yield return CommonScript.GetDelay(1);
            teleportationIndicator.DOFade(0, 1);
            yield return CommonScript.GetDelay(1);
        }
    }
    void StartTeleportationIndicator()
    {
        if (TeleportationIndicatorRoutine != null)
        {
            StopCoroutine(TeleportationIndicatorRoutine);
        }
        TeleportationIndicatorRoutine = StartCoroutine(TeleportationIndicator());
    }
    void StopTeleportationIndicator()
    {
        if (TeleportationIndicatorRoutine != null)
        {
            StopCoroutine(TeleportationIndicatorRoutine);
        }
        teleportationIndicator.DOFade(0, 0);
    }


    public void RemoveDeathScreen()
    {
        CommonScript.CanvasOff(deathScreen);
    }

    public IEnumerator ResetUI()
    {
        StartCoroutine(BringCinematicFramesOut());
        CalculateLife(PlayerController.instance.life);
        yield return CommonScript.GetDelay(0.8f);
        RemoveDeathScreen();
        BringBlackScreen(false);
    }

    public void BringBlackScreen(bool bringIn, float customTime = 1)
    {
        if (!bringIn)
            blackScreen.DOFade(1, 0);
        else
            blackScreen.DOFade(0, customTime);
    }
}
