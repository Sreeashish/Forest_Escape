using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

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
}
