using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public KeyCode respawnButton;
    public bool isPlayerDead;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        Application.targetFrameRate = 60;
    }

    public void OnPlayerDead()
    {
        isPlayerDead = true;
        StartCoroutine(CheckForRespawn());
    }

    IEnumerator CheckForRespawn()
    {
        while (isPlayerDead)
        {
            yield return null;
            PlayerRespawn();
        }
    }

    void PlayerRespawn()
    {
        PlayerController player = PlayerController.instance;
        if (player.playerState == PlayerController.PlayerState.Dead)
        {
            if (Input.GetKeyDown(respawnButton))
            {
                isPlayerDead = false;
                player.ResetPlayer(player.levelController.respawnPoint.position);
                UiController.instance.StartCoroutine(UiController.instance.ResetUI());
            }
        }
    }
}
