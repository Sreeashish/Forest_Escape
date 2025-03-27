using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    public enum EnemyType { Shroom }
    public enum EnemyState { Idle, Patrol, Chase, Battle, GettingHit, Death }

    [Header("Enemy Attributes")]
    public EnemyType enemyType;
    public Levels gameLevel;
    public Transform spawnPoint, hUDPoint, recoilHitPoint;
    public float life, maxLife, watchTime, chaseDistance, attackDistance, collisionCheckRadius, uiRenderDistance;
    public EnemyState enemyState, previousState;
    public bool hasBeenSpotted, hasBeenProvoked;
    public LayerMask playerMask;
    public Transform rayOrigin;
    public float visionRange, angleBetweenRays, noOfRays;
    public Animation enemyAnimation;
    public NavMeshAgent enemyAgent;
    public ParticleSystem forcefieldParticle, bloodParticle, deathParticle;
    public List<Transform> patrolPoints = new();

    [Header("HUD")]
    public GameObject hUDPrefab;
    public EnemyHUDController enemyHUDController;

    float DistanceBtwEnemyAndPlayer()
    {
        float Distance;
        Distance = Vector3.Distance(transform.position, PlayerController.instance.transform.position);
        return Distance;
    }

    private void Start()
    {
        life = maxLife;
        transform.position = spawnPoint.position;
        ForceFieldAttack(false);
        CreateHUD();
        enemyHUDController.StartCoroutine(enemyHUDController.FillLifebar(life, maxLife));
        gameLevel = GameController.instance.currentLevel.level;
    }


    private void Update()
    {
        AssignBehaviour();
        //AssignActions();
        DisplayHUD();
    }
    #region StateChecking
    void AssignBehaviour()
    {
        OpenEyeSensors();
        if (enemyState != EnemyState.Death && enemyState != EnemyState.GettingHit)
        {
            if (enemyState != EnemyState.Patrol && !hasBeenSpotted && enemyState != EnemyState.Chase && enemyState != EnemyState.Battle
                || enemyState != EnemyState.Patrol && !hasBeenProvoked && enemyState != EnemyState.Chase && enemyState != EnemyState.Battle)
            {
                EnemyActions(EnemyState.Patrol);
            }
            if (hasBeenSpotted && enemyState != EnemyState.Chase && !hasBeenProvoked
                || enemyState == EnemyState.Battle && enemyState != EnemyState.Chase && hasBeenProvoked
                || enemyState == EnemyState.Battle && enemyState != EnemyState.Chase && hasBeenSpotted
                || hasBeenProvoked && enemyState != EnemyState.Chase && enemyState != EnemyState.Battle)
            {
                EnemyActions(EnemyState.Chase);
            }
            if (DistanceBtwEnemyAndPlayer() < attackDistance && enemyState != EnemyState.Battle)
            {
                EnemyActions(EnemyState.Battle);
            }
            if (hasBeenSpotted && DistanceBtwEnemyAndPlayer() > chaseDistance && !hasBeenProvoked)
            {
                GetDistracted();
            }
        }
    }

    void OpenEyeSensors()
    {
        if (!hasBeenSpotted)
        {
            float step = angleBetweenRays / (noOfRays - 1);
            for (int i = 0; i < noOfRays; i++)
            {
                float angle = -angleBetweenRays / 2 + i * step;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

                Ray ray = new(rayOrigin.position, direction);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, visionRange, playerMask))
                {
                    hasBeenSpotted = true;
                }
                Vector3 endPoint = ray.origin + ray.direction * visionRange;
                Debug.DrawLine(rayOrigin.position, endPoint, Color.red);
            }
        }
    }

    //void AssignActions()
    //{
    //    if (enemyState != EnemyState.Death && enemyState != EnemyState.GettingHit)
    //    {
    //        if (enemyState != EnemyState.Patrol && !hasBeenSpotted && enemyState != EnemyState.Chase && enemyState != EnemyState.Battle
    //            || enemyState != EnemyState.Patrol && !hasBeenProvoked && enemyState != EnemyState.Chase && enemyState != EnemyState.Battle)
    //        {
    //            EnemyActions(EnemyState.Patrol);
    //        }
    //        if (!hasBeenSpotted && DistanceBtwEnemyAndPlayer() < chaseDistance && enemyState != EnemyState.Chase && !hasBeenProvoked
    //            || DistanceBtwEnemyAndPlayer() > attackDistance && enemyState == EnemyState.Battle && enemyState != EnemyState.Chase && hasBeenProvoked
    //            || DistanceBtwEnemyAndPlayer() > attackDistance && enemyState == EnemyState.Battle && enemyState != EnemyState.Chase && hasBeenSpotted
    //            || hasBeenProvoked && enemyState != EnemyState.Chase && enemyState != EnemyState.Battle)
    //        {
    //            hasBeenSpotted = true;
    //            EnemyActions(EnemyState.Chase);
    //        }
    //        if (DistanceBtwEnemyAndPlayer() < attackDistance && enemyState != EnemyState.Battle)
    //        {
    //            EnemyActions(EnemyState.Battle);
    //        }
    //        if (hasBeenSpotted && DistanceBtwEnemyAndPlayer() > chaseDistance && !hasBeenProvoked)
    //        {
    //            GetDistracted();
    //        }
    //    }
    //}
    #endregion

    #region Behaviours
    public void EnemyActions(EnemyState state, float damageIfAny = 0)
    {
        switch (state)
        {
            case EnemyState.Idle:

                break;

            case EnemyState.Patrol:
                StopAllCoroutines();
                if (patrolRoutine != null)
                {
                    StopCoroutine(patrolRoutine);
                }
                patrolRoutine = StartCoroutine(Patrol());
                break;

            case EnemyState.Chase:
                StopAllCoroutines();
                if (chaseRoutine != null)
                {
                    StopCoroutine(chaseRoutine);
                }
                chaseRoutine = StartCoroutine(ChasePlayer());
                break;

            case EnemyState.Battle:
                StopAllCoroutines();
                if (battleRoutine != null)
                {
                    StopCoroutine(battleRoutine);
                }
                battleRoutine = StartCoroutine(Battle());
                break;

            case EnemyState.GettingHit:
                StopAllCoroutines();
                StartCoroutine(LifeDepletion(damageIfAny));
                break;

            case EnemyState.Death:
                StopAllCoroutines();
                StartCoroutine(Death());
                break;
        }
    }

    public IEnumerator IdleState()
    {
        yield return null;
        enemyState = EnemyState.Idle;
        enemyAnimation.Play("Idle");
    }

    public Coroutine patrolRoutine; //variable for below coroutine
    public IEnumerator Patrol()
    {
        AssignHUDState();
        enemyState = EnemyState.Patrol;
        ForceFieldAttack(false);
        while (enemyState == EnemyState.Patrol)
        {
            yield return null;
            int randomIndex = Random.Range(0, patrolPoints.Count);
            Transform randomPoint = patrolPoints[randomIndex];
            EnableAgent();
            enemyAgent.SetDestination(randomPoint.position);
            enemyAnimation.Play("Walking");
            yield return CommonScript.UntilNear(enemyAgent.transform, randomPoint, 2);
            enemyAgent.enabled = false;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, randomPoint.rotation, 360);
            enemyAnimation.Play("Watching");
            yield return CommonScript.GetDelay(watchTime);
            enemyAgent.enabled = true;
        }
    }

    public Coroutine chaseRoutine; //variable for below coroutine
    public IEnumerator ChasePlayer()
    {
        AssignHUDState();
        enemyState = EnemyState.Chase;
        ForceFieldAttack(false);
        while (enemyState == EnemyState.Chase)
        {
            yield return null;
            enemyAnimation.Play("Running");
            EnableAgent();
            enemyAgent.SetDestination(PlayerController.instance.enemyTarget.position);
        }
    }

    public Coroutine battleRoutine;
    public IEnumerator Battle()
    {
        AssignHUDState();
        enemyState = EnemyState.Battle;
        while (enemyState == EnemyState.Battle)
        {
            yield return null;
            enemyAnimation.Play("Taunting");
            ForceFieldAttack(true);
            yield return CommonScript.GetDelay(2f);
            ForceFieldAttack(false);
        }
    }

    void ForceFieldAttack(bool attack)
    {
        if (attack)
        {
            forcefieldParticle.Play();

            if (PlayerController.instance.playerState != PlayerController.PlayerState.Dead)
            {
                PlayerController.instance.DealDamage(15, false);
            }
        }
        else
        {
            forcefieldParticle.Stop();
        }
    }

    void GetDistracted()
    {
        hasBeenSpotted = false;
        StopAllCoroutines();
        if (patrolRoutine != null)
        {
            StopCoroutine(patrolRoutine);
        }
        patrolRoutine = StartCoroutine(Patrol());
    }

    public void GetHit(float damage)
    {
        previousState = enemyState;
        EnemyActions(EnemyState.GettingHit, damage);
    }

    IEnumerator GetHitAnimation()
    {
        bloodParticle.Play();
        enemyAnimation.Play("Hit");
        enemyAgent.enabled = false;
        Vector3 recoilPos = recoilHitPoint.position;
        transform.DOMove(recoilPos, 0.5f);
        yield return CommonScript.GetDelay(0.5f);
        enemyAgent.enabled = true;
    }
    #endregion

    #region Life
    IEnumerator LifeDepletion(float damage)
    {
        if (!hasBeenProvoked)
        {
            hasBeenProvoked = true;
        }
        float currentLife = life;
        life -= damage;
        life = Mathf.Clamp(life, 0, maxLife);
        enemyHUDController.StartCoroutine(enemyHUDController.FillLifebar(currentLife, life));
        yield return StartCoroutine(GetHitAnimation());
        if (life <= 0)
        {
            EnemyActions(EnemyState.Death);
        }
        else
        {
            EnemyActions(previousState);
        }
    }

    IEnumerator Death()
    {
        enemyState = EnemyState.Death;
        enemyAnimation.Play("Death");
        yield return transform.DOScale(2, 1.5f);
        yield return CommonScript.GetDelay(1.5f);
        deathParticle.Play();
        yield return CommonScript.GetDelay(1.5f);
        gameObject.SetActive(false);
        enemyHUDController.ShowHUD(false);
        if (gameLevel == Levels.Level1)
        {
            GameController.instance.currentLevel.EnableLastInteractionForTheLevel();
        }
    }
    #endregion

    #region HUD
    void CreateHUD()
    {
        GameObject hud = Instantiate(hUDPrefab, UiController.instance.worldspaceUIElementsHolder);
        enemyHUDController = hud.GetComponent<EnemyHUDController>();
        enemyHUDController.ShowHUD(false);
        AssignHUDState();
    }

    void DisplayHUD()
    {
        if (!CameraController.instance.IsInsideClippingPlanes(transform))
        {
            enemyHUDController.ShowHUD(false);
        }
        if (DistanceBtwEnemyAndPlayer() <= uiRenderDistance && CameraController.instance.IsInsideClippingPlanes(transform)
            || hasBeenProvoked && CameraController.instance.IsInsideClippingPlanes(transform))
        {
            enemyHUDController.UpdatePosition(hUDPoint);
            AssignHUDState();
        }
        else if (!hasBeenProvoked && DistanceBtwEnemyAndPlayer() >= uiRenderDistance)
        {
            enemyHUDController.ShowHUD(false);
        }
    }

    void AssignHUDState()
    {
        if (DistanceBtwEnemyAndPlayer() <= uiRenderDistance)
        {
            enemyHUDController.SetHUDState(EnemyHUDController.HUDState.Icon);
        }
        if (hasBeenSpotted || hasBeenProvoked)
        {
            enemyHUDController.SetHUDState(EnemyHUDController.HUDState.LifeBar);
        }
    }
    #endregion

    #region SubFuctions
    public void ResetEnemey()
    {
        StopAllCoroutines();
    }

    void EnableAgent()
    {
        if (!enemyAgent.enabled)
            enemyAgent.enabled = true;
    }
    #endregion
}
