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
    public Transform spawnPoint, hUDPoint;
    public float life, maxLife, watchTime, chaseDistance, attackDistance, collisionCheckRadius, uiRenderDistance;
    public EnemyState enemyState, previousState;
    public bool hasBeenProvoked;
    public Animation enemyAnimation;
    public NavMeshAgent enemyAgent;
    public ParticleSystem forcefieldParticle, bloodParticle, deathParticle;
    public List<Transform> patrolPoints = new();

    [Header("HUD")]
    public GameObject hUDPrefab;
    public EnemyHUDController enemyHUDController;

    private void Start()
    {
        life = maxLife;
        transform.position = spawnPoint.position;
        ForceFieldAttack(false);
        CreateHUD();
        enemyHUDController.StartCoroutine(enemyHUDController.FillLifebar(life, maxLife));
    }

    float distBtwEnemyAndPlayer;
    private void Update()
    {
        if (enemyState != EnemyState.Death && enemyState != EnemyState.GettingHit)
        {
            distBtwEnemyAndPlayer = Vector3.Distance(transform.position, PlayerController.instance.transform.position);
            if (!hasBeenProvoked && distBtwEnemyAndPlayer <= chaseDistance)
            {
                if (enemyState != EnemyState.Chase)
                {
                    hasBeenProvoked = true;
                }
            }
            else if (enemyState != EnemyState.Patrol && !hasBeenProvoked)
            {
                EnemyActions(EnemyState.Patrol);
            }
            else if (hasBeenProvoked && enemyState != EnemyState.Chase && enemyState != EnemyState.Battle)
            {
                EnemyActions(EnemyState.Chase);
            }

            if (hasBeenProvoked && distBtwEnemyAndPlayer < attackDistance && enemyState != EnemyState.Battle)
            {
                EnemyActions(EnemyState.Battle);
            }

            if (hasBeenProvoked && distBtwEnemyAndPlayer > attackDistance && enemyState != EnemyState.Chase && enemyState == EnemyState.Battle)
            {
                EnemyActions(EnemyState.Chase);
            }

            if (hasBeenProvoked && distBtwEnemyAndPlayer > chaseDistance)
            {
                GetDistracted();
            }
        }
        if (distBtwEnemyAndPlayer <= uiRenderDistance && CameraController.instance.IsInsideClippingPlanes(transform))
        {
            DisplayHUD();
        }
        else
        {
            enemyHUDController.ShowHUD(false);
        }
    }

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
                LifeDepletion(damageIfAny);
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
        while (true)
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
        while (hasBeenProvoked)
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
        hasBeenProvoked = false;
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
    #endregion

    #region Life
    void LifeDepletion(float damage)
    {
        float currentLife = life;
        life -= damage;
        life = Mathf.Clamp(life, 0, maxLife);
        enemyHUDController.StartCoroutine(enemyHUDController.FillLifebar(currentLife, life));
        bloodParticle.Play();
        enemyAnimation.Play("Hit");
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
        enemyHUDController.UpdatePosition(hUDPoint);
        AssignHUDState();
    }

    void AssignHUDState()
    {
        distBtwEnemyAndPlayer = Vector3.Distance(transform.position, PlayerController.instance.transform.position);
        if (distBtwEnemyAndPlayer <= uiRenderDistance)
        {
            enemyHUDController.SetHUDState(EnemyHUDController.HUDState.Icon);
        }
        if(hasBeenProvoked)
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
