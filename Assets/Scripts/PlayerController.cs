using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables
    public enum PlayerState { Idle, Walking, Running, Jumping, Dead }
    public enum PlayerMode { Exploring, CombatReady, MidAttack }

    [Header("Player Attributes")]
    public CharacterController player;
    public CinemachineFreeLook freeLookCamera;
    public CombatController combatController;
    public Transform playerBody, enemyTarget;
    public Animation playerAnimation;
    public Rigidbody playerRB;
    public PlayerState playerState;
    public PlayerMode playerMode;
    public bool isControllable, onground, raycasting;
    public float walkingSpeed, sprintSpeed, turnSmoothTime = 0.1f;
    public KeyCode sprintButton;
    public KeyCode interactionButton;
    public LayerMask groundMask;
    public ParticleSystem bloodParticle, splashParticle, sparks;
    public float life, maxLife, regenerationStartTime;
    float turnVelocity;


    public bool OnGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 1, groundMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [Header("Camera")]
    public CameraController cameraController;
    public Transform cameraTransform, cameraCinematicPoint, deathCamPoint, aimCameraPoint;
    public float mouseAimSensitivityX, mouseAimSensitivityY;


    [Header("Current Level Info")]
    public Levels currentLevel;
    public LevelController levelController;
    public List<Interactable> interactablesInCurrentLevel;

    public static PlayerController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    #endregion

    private void Start()
    {
        life = maxLife;
        currentLevel = levelController.level;
        interactablesInCurrentLevel = new List<Interactable>();
        interactablesInCurrentLevel = levelController.interactables;
        if (levelController.onBoardingCompleted)
            ToggleControlsOnorOff(true);

        TurnRayOnorOff(true);
    }

    void Update()
    {
        PlayerMovement();
        GroundCheck();
        CheckForInteractions();
        CombatActivations();
        LifeRegeneration();
    }

    private void LateUpdate()
    {
        transform.position = playerBody.position;
    }

    #region PlayerMovement
    void PlayerMovement()
    {
        if (isControllable)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0f) * Vector3.forward;
                if (Input.GetKey(sprintButton))
                {
                    player.Move(moveDirection.normalized * sprintSpeed * Time.deltaTime);
                    SetPlayerState(PlayerState.Running);
                }
                else
                {
                    player.Move(moveDirection.normalized * walkingSpeed * Time.deltaTime);
                    SetPlayerState(PlayerState.Walking);
                }
            }
            else
            {
                if (playerMode != PlayerMode.MidAttack)
                {
                    SetPlayerState(PlayerState.Idle);
                }
            }
        }
    }

    IEnumerator PlayerJump(Transform jumpToPos)
    {
        ToggleControlsOnorOff(false);
        TurnRayOnorOff(false);
        yield return null;
        playerRB.useGravity = false;
        playerState = PlayerState.Jumping;
        player.transform.DORotate(jumpToPos.eulerAngles, 0.5f);
        transform.DOJump(jumpToPos.position, 5, 1, 2);
        playerAnimation.Play("JumpStart");
        yield return CommonScript.GetDelay(0.5f);
        playerAnimation.Play("JumpIdle");
        yield return CommonScript.GetDelay(0.5f);
        playerAnimation.Play("JumpLand");
        yield return CommonScript.GetDelay(0.5f);
        transform.position = jumpToPos.position;
        SetPlayerState(PlayerState.Idle);
        TurnRayOnorOff(true);
        ToggleControlsOnorOff(true);
    }

    #endregion

    #region CombatBehaviour

    void CombatActivations()
    {
        if (levelController.onBoardingCompleted)
        {
            if (Input.GetMouseButton(1) && playerMode != PlayerMode.MidAttack)
            {
                EnableCombatMode(true);
            }
            if (Input.GetMouseButtonUp(1))
            {
                EnableCombatMode(false);
            }
        }
    }

    void EnableCombatMode(bool enable)
    {
        if (enable)
        {
            SetPlayerMode(PlayerMode.CombatReady);
            combatController.CrosshairRay();
            cameraController.CombatCamera(true, aimCameraPoint);
            UiController.instance.Crosshair(true);
            AimCamera();
        }
        else
        {
            SetPlayerMode(PlayerMode.Exploring);
            cameraController.CombatCamera(false);
            UiController.instance.Crosshair(false);
        }
    }

    float rotationY = 0f; //this float is for the function below only
    public void AimCamera()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float rotationX = transform.localEulerAngles.y + mouseX * mouseAimSensitivityX;
        float mouseYInput = mouseY * mouseAimSensitivityY;
        rotationY -= mouseYInput;
        rotationY = Mathf.Clamp(rotationY, -30f, 20f);

        transform.localEulerAngles = new Vector3(0, rotationX, 0);
        cameraTransform.localEulerAngles = new Vector3(rotationY, 0, 0);
    }

    public IEnumerator TeleportPlayer(Vector3 teleportTo)
    {
        ToggleControlsOnorOff(false);
        UiController.instance.BringBlackScreen(true);
        yield return CommonScript.GetDelay(0.2f);
        transform.position = teleportTo;
        UiController.instance.BringBlackScreen(false, 0.2f);
    }
    #endregion

    #region Sensors
    void GroundCheck()
    {
        if (raycasting)
        {
            if (!OnGround())
            {
                onground = false;
                playerRB.useGravity = true;
            }
            else
            {
                onground = true;
                playerRB.useGravity = false;
            }
        }
    }

    void CheckForInteractions()
    {
        for (int i = 0; i < interactablesInCurrentLevel.Count; i++)
        {
            if (playerMode == PlayerMode.Exploring)
            {
                if (Vector3.Distance(transform.position, interactablesInCurrentLevel[i].transform.position) <= 5)
                {
                    if (CameraController.instance.IsInsideClippingPlanes(interactablesInCurrentLevel[i].transform) && interactablesInCurrentLevel[i].isInteractable)
                    {
                        interactablesInCurrentLevel[i].DisplayMarker(true);
                    }
                    else
                    {
                        interactablesInCurrentLevel[i].DisplayMarker(false);
                    }
                        interactablesInCurrentLevel[i].MarkerState(false);
                    
                    if (Vector3.Distance(transform.position, interactablesInCurrentLevel[i].transform.position) <= 2f)
                    {
                        if (interactablesInCurrentLevel[i].isInteractable && !interactablesInCurrentLevel[i].oneTimeInterationOver)
                        {
                            interactablesInCurrentLevel[i].MarkerState(true);
                            if (levelController.level == Levels.Level1 && !levelController.onBoardingCompleted)
                            {
                                OnboardingController.instance.StartCoroutine(OnboardingController.instance.StartOnBoarding());
                            }
                            StartCoroutine(ActivateInteraction(interactablesInCurrentLevel[i]));
                        }
                    }
                }
                else
                {
                    interactablesInCurrentLevel[i].DisplayMarker(false);
                }
            }
            else
            {
                interactablesInCurrentLevel[i].DisplayMarker(false);
            }
        }
    }

    IEnumerator ActivateInteraction(Interactable interactable)
    {
        if (Input.GetKeyDown(interactionButton))
        {
            interactable.DisplayMarker(false);
            switch (interactable.interactionType)
            {
                case Interactable.InteractionType.OpenPrison:
                    ToggleControlsOnorOff(false);
                    interactable.TurnInteraction(false);
                    transform.DORotate(interactable.transform.eulerAngles, 0.5f);
                    StartCoroutine(interactable.OpenPrisonDoor());
                    yield return StartCoroutine(cameraController.StartCinematic(interactable.lookAtObject, cameraCinematicPoint));
                    ToggleControlsOnorOff(true);
                    break;
                case Interactable.InteractionType.Jump:
                    interactable.TurnInteraction(false);
                    StartCoroutine(PlayerJump(interactable.interactionItem));
                    interactable.TurnInteraction(true);
                    interactable.DisplayMarker(true);
                    break;
                case Interactable.InteractionType.HealthChest:
                    ToggleControlsOnorOff(false);
                    interactable.TurnInteraction(false);
                    transform.DORotate(interactable.transform.eulerAngles, 0.5f);
                    StartCoroutine(interactable.OpenChest());
                    interactable.oneTimeInterationOver = true;
                    yield return StartCoroutine(cameraController.StartCinematic(interactable.lookAtObject, interactable.customCameraPoint));
                    ToggleControlsOnorOff(true);
                    break;
            }
        }
    }
    #endregion

    #region Life

    bool regenerationStarted; //boolean flag for below function only
    void LifeRegeneration()
    {
        if (life <= 99 && !regenerationStarted)
        {
            if(RegenerationRoutine != null)
            {
                StopCoroutine(RegenerationRoutine);
            }
            RegenerationRoutine = StartCoroutine(LifeRegen());
        }
    }

    Coroutine RegenerationRoutine;
    IEnumerator LifeRegen()
    {
        float currentLife;
        regenerationStarted = true;
        yield return CommonScript.GetDelay(regenerationStartTime);
        while (life < maxLife)
        {
            yield return null;
            currentLife = life;
            life += 1;
            life = Mathf.Clamp(life, 0, maxLife);
            yield return UiController.instance.StartCoroutine(UiController.instance.FillFillbar(currentLife, life, true));
            yield return CommonScript.GetDelay(0.1f);
        }
        regenerationStarted = false;
    }

    public IEnumerator GainLife(float value)
    {
        float currentLife = life;
        life += value;
        life = Mathf.Clamp(life, 0, maxLife);
        sparks.Play();
        yield return UiController.instance.StartCoroutine(UiController.instance.FillFillbar(currentLife, life, true));
    }

    IEnumerator LifeDepletion(float damage)
    {
        yield return null;
        if (life >= -0.1f)
        {
            float currentLife = life;
            life -= damage;
            life = Mathf.Clamp(life, 0, maxLife);
            yield return UiController.instance.StartCoroutine(UiController.instance.FillFillbar(currentLife, life, true));
            bloodParticle.Play();
            playerAnimation.Play("Hit");
            if (life <= 0)
            {
                StartCoroutine(Death());
            }
        }
    }

    public Coroutine DamageCoroutine;
    public IEnumerator GettingDamagePerSecond(float damage, bool continous = true)
    {
        if (continous)
        {
            while (true)
            {
                yield return null;
                yield return StartCoroutine(LifeDepletion(damage));
                yield return CommonScript.GetDelay(1);
            }
        }
        else
        {
            yield return StartCoroutine(LifeDepletion(damage));
            TurnMovementOnorOff(false);
            yield return CommonScript.GetDelay(0.75f);
            TurnMovementOnorOff(true);
        }
    }

    public void DealDamage(float damage, bool continousDamage)
    {
        StopRegeneration();
        if (continousDamage)
        {
            TurnMovementOnorOff(false);
            if (DamageCoroutine != null)
            {
                StopCoroutine(DamageCoroutine);
            }
            DamageCoroutine = StartCoroutine(GettingDamagePerSecond(damage));
        }
        else
        {
            if (DamageCoroutine != null)
            {
                StopCoroutine(DamageCoroutine);
            }
            DamageCoroutine = StartCoroutine(GettingDamagePerSecond(damage, false));
        }
    }

    public IEnumerator Death()
    {
        if (DamageCoroutine != null)
        {
            StopCoroutine(DamageCoroutine);
        }
        ToggleControlsOnorOff(false);
        yield return CommonScript.GetDelay(0.2f);
        SetPlayerState(PlayerState.Dead);
        yield return cameraController.StartCoroutine(cameraController.DeathCam(transform, deathCamPoint));
    }
    #endregion

    #region SubFunctions
    public void ToggleControlsOnorOff(bool turnOn)
    {
        if (turnOn)
        {
            isControllable = true;
            freeLookCamera.enabled = true;
        }
        else
        {
            SetPlayerState(PlayerState.Idle);
            isControllable = false;
            freeLookCamera.enabled = false;
        }
    }

    public void TurnMovementOnorOff(bool onOff)
    {
        if (onOff)
            isControllable = true;
        else
            isControllable = false;
    }

    public void ToggleMouseControlOnorOff(bool onOff)
    {
        if (onOff)
            freeLookCamera.enabled = true;
        else
            freeLookCamera.enabled = false;
    }

    void TurnRayOnorOff(bool onOff)
    {
        if (onOff)
            raycasting = true;
        else
            raycasting = false;
    }


    public void ResetPlayer(Vector3 position)
    {
        UiController.instance.BringBlackScreen(true);
        transform.position = position;
        playerBody.position = position;
        cameraController.cameraBrain.enabled = false;
        TurnRayOnorOff(false);
        ToggleControlsOnorOff(false);
        SetPlayerState(PlayerState.Idle);
        EnableCombatMode(false);
        life = 100;
        cameraController.cameraBrain.enabled = true;
        ToggleControlsOnorOff(true);
        TurnRayOnorOff(true);
        StopAllCoroutines();
    }

    public void SetPlayerState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                playerState = PlayerState.Idle;
                playerAnimation.Play("Idle");
                break;
            case PlayerState.Walking:
                playerState = PlayerState.Walking;
                playerAnimation.Play("Walking");
                break;
            case PlayerState.Running:
                playerState = PlayerState.Running;
                playerAnimation.Play("Running");
                break;
            case PlayerState.Dead:
                playerState = PlayerState.Dead;
                playerAnimation.Play("Death");
                break;
        }
    }

    public void SetPlayerMode(PlayerMode mode)
    {
        switch (mode)
        {
            case PlayerMode.Exploring:
                playerMode = PlayerMode.Exploring;
                break;
            case PlayerMode.CombatReady:
                playerMode = PlayerMode.CombatReady;
                TurnMovementOnorOff(true);
                break;
            case PlayerMode.MidAttack:
                playerMode = PlayerMode.MidAttack;
                TurnMovementOnorOff(false);
                playerAnimation.Play("Punch");
                break;
        }
    }

    void StopRegeneration()
    {
        if (RegenerationRoutine != null)
        {
            StopCoroutine(RegenerationRoutine);
        }
        regenerationStarted = false;
    }
    #endregion
}
