using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Walking, Running, Jumping, Dead }

    [Header("Player Attributes")]
    public CharacterController player;
    public CinemachineFreeLook mouseControls;
    public Transform playerBody;
    public Animation playerAnimation;
    public Rigidbody playerRB;
    public PlayerState playerState;
    public bool isControllable, onground, raycasting, checkCollision;
    public float walkingSpeed, sprintSpeed, turnSmoothTime = 0.1f;
    public KeyCode sprintButton;
    public KeyCode interactionButton;
    public LayerMask groundMask;
    float turnVelocity;
    Vector3 velocity;
    public float life;

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
    public Transform cameraTransform, cameraCinematicPoint, deathCamPoint;

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

    private void Start()
    {
        checkCollision = true;
        currentLevel = levelController.level;
        interactablesInCurrentLevel = new List<Interactable>();
        interactablesInCurrentLevel = levelController.interactables;
        if (levelController.onBoardingCompleted)
            ToggleControlsOn();

        TurnRayOn();
    }

    void Update()
    {
        PlayerMovement();
        GroundCheck();
        CheckForInteractions();
    }

    private void LateUpdate()
    {
        transform.position = playerBody.position;
    }

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
                    MakePlayerState(PlayerState.Running);
                }
                else
                {
                    player.Move(moveDirection.normalized * walkingSpeed * Time.deltaTime);
                    MakePlayerState(PlayerState.Walking);
                }
            }
            else
            {
                MakePlayerState(PlayerState.Idle);
            }
        }
    }

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

    IEnumerator PlayerJump(Transform jumpToPos)
    {
        ToggleControlsOff();
        TurnRayOff();
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
        MakePlayerState(PlayerState.Idle);
        TurnRayOn();
        ToggleControlsOn();
    }

    void CheckForInteractions()
    {
        for (int i = 0; i < interactablesInCurrentLevel.Count; i++)
        {
            if (Vector3.Distance(transform.position, interactablesInCurrentLevel[i].transform.position) <= 5)
            {
                interactablesInCurrentLevel[i].DisplayMarker();
                if (Vector3.Distance(transform.position, interactablesInCurrentLevel[i].transform.position) <= 2)
                {
                    if (interactablesInCurrentLevel[i].isInteractable)
                    {
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
                interactablesInCurrentLevel[i].TurnOffMarker();
            }
        }
    }

    IEnumerator ActivateInteraction(Interactable interactable)
    {
        if (Input.GetKeyDown(interactionButton))
        {
            interactable.DestroyMarker();
            switch (interactable.interactionType)
            {
                case Interactable.InteractionType.OpenPrison:
                    ToggleControlsOff();
                    interactable.TurnInteractionOff();
                    transform.DORotate(interactable.transform.eulerAngles, 0.5f);
                    StartCoroutine(interactable.OpenPrisonDoor());
                    yield return StartCoroutine(cameraController.StartCinematic(interactable.lookAtObject, cameraCinematicPoint));
                    ToggleControlsOn();
                    break;
                case Interactable.InteractionType.Jump:
                    interactable.TurnInteractionOff();
                    StartCoroutine(PlayerJump(interactable.interactionItem));
                    interactable.TurnInteractionOn();
                    interactable.CreateMarker();
                    break;
            }
        }
    }

    IEnumerator LifeDepletion(float damage)
    {
        yield return null;
        if (life >= -0.1f)
        {
            float currentLife = life;
            float decreasedLife;
            life -= damage;
            decreasedLife = life;
            yield return UiController.instance.StartCoroutine(UiController.instance.FillFillbar(currentLife, decreasedLife));
            if (life <= 0)
            {
                StartCoroutine(Death());
            }
        }
    }

    public Coroutine DamageCoroutine;
    public IEnumerator GettingDamagePerSecond(float damage)
    {
        while (true)
        {
            yield return null;
            yield return StartCoroutine(LifeDepletion(damage));
            ArrestMovement();
            playerAnimation.Play("Hit");
            yield return CommonScript.GetDelay(1);
        }
    }

    IEnumerator Death()
    {
        if(DamageCoroutine != null)
        {
            StopCoroutine(DamageCoroutine);
        }
        ToggleControlsOff();
        yield return CommonScript.GetDelay(0.2f);
        MakePlayerState(PlayerState.Idle);
        yield return cameraController.StartCoroutine(cameraController.DeathCam(transform, deathCamPoint));
    }

    //private void OnControllerColliderHit(ControllerColliderHit hit)
    //{
    //    if (hit.collider.CompareTag("Water"))
    //    {
    //        if (DamageCoroutine != null)
    //        {
    //            Debug.Log("STOPPED");
    //            StopCoroutine(DamageCoroutine);
    //            FreeMovement();
    //        }
    //        DamageCoroutine = StartCoroutine(GettingDamagePerSecond(25));
    //    }
    //}

    #region SubFunctions
    public void ToggleControlsOn()
    {
        isControllable = true;
        mouseControls.enabled = true;
    }

    public void ToggleControlsOff()
    {
        MakePlayerState(PlayerState.Idle);
        isControllable = false;
        mouseControls.enabled = false;
    }

    public void ArrestMovement()
    {
        isControllable = false;
    }

    public void FreeMovement()
    {
        isControllable = true;
    }

    public void ToggleMouseControlOn()
    {
        mouseControls.enabled = true;
    }

    public void ToggleMouseControlOff()
    {
        mouseControls.enabled = false;
    }

    void TurnRayOn()
    {
        raycasting = true;
    }

    void TurnRayOff()
    {
        raycasting = false;
    }

    public void ResetPlayer(Vector3 position)
    {
        UiController.instance.BlackScreenIn();
        transform.position = position;
        playerBody.position = position;
        cameraController.cameraBrain.enabled = false;
        TurnRayOff();
        ToggleControlsOff();
        MakePlayerState(PlayerState.Idle);
        life = 100;
        cameraController.cameraBrain.enabled = true;
        ToggleControlsOn();
        TurnRayOn();
    }

    public void MakePlayerState(PlayerState state)
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

    #endregion
}
