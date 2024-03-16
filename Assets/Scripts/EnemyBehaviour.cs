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
    public Animation enemyAnimation;
    public NavMeshAgent enemyAgent;
    public List<Transform> patrolPoints = new();

    private void Start()
    {
        transform.position = spawnPoint.position;
        EnemyActions(EnemyState.Patrol);
    }

    public void EnemyActions(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:

                break;
            case EnemyState.Patrol:
                if(partolRoutine != null)
                {
                    StopCoroutine(partolRoutine);
                }
                partolRoutine = StartCoroutine(PatrolState());
                break;
            case EnemyState.Chase:

                break;
            case EnemyState.Battle:

                break;
            case EnemyState.Death:

                break;
        }
    }

    public IEnumerator IdleState()
    {
        yield return null;
        enemyAnimation.Play("Idle");
    }

    public Coroutine partolRoutine;
    public IEnumerator PatrolState()
    {
        while (true)
        {
            yield return null;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                enemyAgent.SetDestination(patrolPoints[i].position);
                enemyAnimation.Play("Walking");
                yield return CommonScript.UntilNear(enemyAgent.transform, patrolPoints[i]);
                enemyAgent.enabled = false;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, patrolPoints[i].rotation, 360);
                enemyAnimation.Play("Watching");
                yield return CommonScript.GetDelay(watchTime);
                enemyAgent.enabled = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Ground"))
        {
            Debug.Log("ON GROUND");
        }
    }
}
