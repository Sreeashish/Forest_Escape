using Cinemachine;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using static PlayerController;

public class CameraController : MonoBehaviour
{
    [Header("Camera Attributes")]
    public CinemachineBrain cameraBrain;
    public Camera cameraComponent;
    public CinemachineFreeLook freeLookCamera;
    Vector3 posb4Cinematic, rotb4Cinematic;

    public IEnumerator StartCinematic(Transform lookAt, Transform camPos)
    {
        cameraBrain.enabled = false;
        posb4Cinematic = transform.position;
        rotb4Cinematic = transform.rotation.eulerAngles;
        UiController.instance.StartCoroutine(UiController.instance.BringCinematicFramesIn());
        transform.DOMove(camPos.position, 2);
        transform.DOLookAt(lookAt.position, 2);

        StartCoroutine(ChangeFOV(50, 70, 1));
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

    bool combatCamIn; //boolean flag for the below function
    public void CombatCamera(bool on, Transform camPos = null)
    {
        if (on)
        {
            Quaternion camDirection = transform.rotation;
            camDirection.x = 0;
            camDirection.z = 0;
            
            if (!combatCamIn)
            {
                cameraBrain.enabled = false;
                freeLookCamera.enabled = false;
                PlayerController.instance.transform.rotation = Quaternion.Slerp(transform.rotation, camDirection, 20 * Time.deltaTime);
                StartCoroutine(ChangeFOV(50, 70, 0.5f));
                transform.position = camPos.position;
                transform.rotation = camPos.rotation;
                combatCamIn = true;
            }
            
        }
        else if (!on)
        {
            cameraBrain.enabled = true;
            freeLookCamera.enabled = true;
            combatCamIn = false;
        }
    }

    public IEnumerator DeathCam(Transform lookat, Transform camPosition)
    {
        yield return null;
        cameraBrain.enabled = false;
        transform.DOLookAt(lookat.eulerAngles, 3);
        PlayerController.instance.SetPlayerState(PlayerState.Dead);
        UiController.instance.StartCoroutine(UiController.instance.BringCinematicFramesIn());
        yield return CommonScript.GetDelay(0.5f);
        UiController.instance.DeathScreen();
        MoveandRotateCamera(camPosition, 3);
        StartCoroutine(ChangeFOV(50, 70, 1f));
        yield return CommonScript.GetDelay(2.5f);
        GameController.instance.OnPlayerDead();
    }

    IEnumerator ChangeFOV(float from, float to, float time)
    {
        float t = 0, duration = time;
        while (t < duration)
        {
            yield return null;
            cameraComponent.fieldOfView = Mathf.Lerp(from, to, t / duration);
            t += Time.deltaTime;
        }
    }

    void MoveandRotateCamera(Transform to, float time)
    {
        transform.DOMove(to.position, time);
        transform.DORotate(to.rotation.eulerAngles, time);
    }
}
