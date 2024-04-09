using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHUDController : MonoBehaviour
{
    public enum HUDState { Icon, LifeBar }
    public HUDState hUDState;
    public RectTransform iconPos;
    public Image lifebar, lifebarHolder, iconHolder;
    public CanvasGroup lifeBarHolderCanvas, hUDCanvas;
    public Vector3 offset;

    public void ShowHUD(bool show)
    {
        if (show)
        {
            CommonScript.CanvasOn(hUDCanvas);
        }
        else
        {
            CommonScript.CanvasOff(hUDCanvas);
        }
    }

    public void SetHUDState(HUDState state)
    {
        ShowHUD(true);
        switch (state)
        {
            case HUDState.Icon:
                IconOnlyState();
                break;
            case HUDState.LifeBar:
                LifeBarWithIcon();
                break;
        }
    }

    void IconOnlyState()
    {
        lifebarHolder.rectTransform.localScale = Vector2.zero;
        CommonScript.CanvasOff(lifeBarHolderCanvas);
        iconHolder.rectTransform.anchoredPosition = new Vector2(38, 0);
        iconHolder.rectTransform.localScale = new Vector2(2, 2);
    }

    void LifeBarWithIcon()
    {
        lifebarHolder.rectTransform.localScale = Vector2.one;
        CommonScript.CanvasOn(lifeBarHolderCanvas);
        iconHolder.rectTransform.anchoredPosition = new Vector2(7, 0);
        iconHolder.rectTransform.localScale = Vector2.one;
    }

    public void UpdatePosition(Transform enemy)
    {
        transform.position = Camera.main.WorldToScreenPoint(enemy.position) + offset;
    }

    void CalculateLife(float life)
    {
        lifebar.fillAmount = CommonScript.Remap(life, 0, 100, 0, 1);
    }

    public IEnumerator FillLifebar(float from, float to, float duration = 0.7f)
    {
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            yield return null;
            float t = timeElapsed / duration;
            float fill = Mathf.Lerp(from, to, t);
            CalculateLife(fill);
            timeElapsed += Time.deltaTime;
        }
        CalculateLife(to);
    }
}
