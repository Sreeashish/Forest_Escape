using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    public enum EnemyType { Shroom }
    public enum EnemyState { Idle, Patrol, Chase, Battle, Death }

    [Header("Enemy Attributes")]
    public EnemyType enemyType;
    public Transform spawnPoint;
    public float life, watchTime;
    public EnemyState enemyState;
    public bool hasBeenProvoked;
    public Animation enemyAnimation;
    public NavMeshAgent enemyAgent;
    public ParticleSystem forcefieldParticle;
    public List<Transform> patrolPoints = new();

    private void Start()
    {
        transform.position = spawnPoint.position;
        //StartCoroutine(EnemySensor());
    }

    public void EnemyActions(EnemyState state)
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

                break;
            case EnemyState.Death:

                break;
        }
    }

    float distBtwEnemyAndPlayer;
    private void Update()
    {
        distBtwEnemyAndPlayer = Vector3.Distance(transform.position, PlayerController.instance.transform.position);
        if (distBtwEnemyAndPlayer <= 5 && !hasBeenProvoked)
        {
            if (enemyState != EnemyState.Chase)
            {
                hasBeenProvoked = true;
            }
        }
        else if (enemyState != EnemyState.Patrol && !hasBeenProvoked)
        {
            print("start");
            EnemyActions(EnemyState.Patrol);
        }
        else if (hasBeenProvoked && enemyState != EnemyState.Chase)
        {
            EnemyActions(EnemyState.Chase);
        }

        if (distBtwEnemyAndPlayer > 8 && hasBeenProvoked)
        {
            GetDistracted();
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
        enemyState = EnemyState.Patrol;
        while (true)
        {
            yield return null;
            int randomIndex = Random.Range(0, patrolPoints.Count);
            Transform randomPoint = patrolPoints[randomIndex];

            enemyAgent.SetDestination(randomPoint.position);
            enemyAnimation.Play("Walking");
            yield return CommonScript.UntilNear(enemyAgent.transform, randomPoint);
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
        enemyState = EnemyState.Chase;
        while (hasBeenProvoked)
        {
            yield return null;
            enemyAnimation.Play("Running");
            enemyAgent.SetDestination(PlayerController.instance.enemyTarget.position);
        }
    }

    public Coroutine battleRoutine;
    //public IEnumerator Battle()
    //{
    //    enemyState = EnemyState.Battle;

    //}

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            Debug.Log("ON GROUND");
        }
        if (collision.collider.CompareTag("Runic"))
        {
            Debug.Log("GOT HIT");
        }
    }

    public void ResetEnemey()
    {
        StopAllCoroutines();
    }
}
