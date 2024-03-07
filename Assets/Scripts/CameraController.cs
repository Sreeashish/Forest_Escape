using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlayerController;

public class CameraController : MonoBehaviour
{
    [Header("Camera Attributes")]
    public CinemachineBrain cameraBrain;
    public Camera cameraComponent;
    Vector3 posb4Cinematic, rotb4Cinematic;

    public IEnumerator StartCinematic(Transform lookAt, Transform camPos)
    {
        cameraBrain.enabled = false;
        posb4Cinematic = transform.position;
        rotb4Cinematic = transform.rotation.eulerAngles;
        transform.DOLookAt(lookAt.eulerAngles, 2);
        UiController.instance.StartCoroutine(UiController.instance.BringCinematicFramesIn());
        MoveandRotateCamera(camPos, 2);
        StartCoroutine(ChangeFOV());
        yield return CommonScript.GetDelay(2);
        yield return StartCoroutine(EndCinematic());
    }

    public IEnumerator EndCinematic()
    {
        transform.DOMove(posb4Cinematic, 2);
        transform.DORotate(rotb4Cinematic, 2);
        yield return UiController.instance.StartCoroutine(UiController.instance.BringCinematicFramesOut());
        yield return CommonScript.GetDelay(1);
        cameraBrain.enabled = true;
    }

    public IEnumerator DeathCam(Transform lookat, Transform camPosition)
    {
        yield return null;
        cameraBrain.enabled = false;
        transform.DOLookAt(lookat.eulerAngles, 3);
        PlayerController.instance.MakePlayerState(PlayerState.Dead);
        UiController.instance.StartCoroutine(UiController.instance.BringCinematicFramesIn());
        yield return CommonScript.GetDelay(0.5f);
        UiController.instance.DeathScreen();
        MoveandRotateCamera(camPosition, 3);
        StartCoroutine(ChangeFOV());
    }

    IEnumerator ChangeFOV()
    {
        float t = 0, duration = 1;
        while (t < duration)
        {
            yield return null;
            cameraComponent.fieldOfView = Mathf.Lerp(50, 70, t/duration);
            t += Time.deltaTime;
        }
    }

    void MoveandRotateCamera(Transform to, float time)
    {
        transform.DOMove(to.position, time);
        transform.DORotate(to.rotation.eulerAngles, time);
    }
}
