using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]

public class AIPlayerController : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public UnityEngine.Object obj;
    public string functionname, className;
    public Type cla2;
    public Interactable hgg2;
    public List<Destinations> destinations = new();
    public UnityEvent<MonoBehaviour> interactionFunction;
    public Transform playerTransform, endGoalPoint;
    public float interactionCheckRange;
    public bool isHiding, inMovement;

    private void Start()
    {
        Type type = destinations[0].destinationPoint.GetType();

        while (type.BaseType != null && type.BaseType != typeof(MonoBehaviour) && type.BaseType != typeof(object))
        {
            type = type.BaseType;
        }

        cla2 = Type.GetType(className);
        Debug.Log(cla2);

        Debug.Log("Highest-Level Base Class: " + type);
        Type type2 = obj.GetComponent<PlayerController>().GetType();
        Component p = obj.GetComponent(cla2);
        MethodInfo method = type2.GetMethod(functionname);
        Debug.Log(method);
        method.Invoke(p, new object[] { hgg2 });
    }

    void Update()
    {
        AIMovement();
    }

    void AIMovement()
    {
        SimulateMovement();
    }

    void SimulateMovement()
    {
        if(!inMovement)
        StartCoroutine(DestinationSetter(endGoalPoint));
    }

    IEnumerator DestinationSetter(Transform destination)
    {
        inMovement = true;
        navMeshAgent.SetDestination(destination.position);

        bool stuck = false;
        const float stuckThreshold = 0.1f;
        const float stuckCheckInterval = 0.5f;
        const float stuckTimeout = 2.0f;
        Vector3 lastPosition = playerTransform.position;
        float timeSinceLastMovement = 0f;

        while (Vector3.Distance(playerTransform.position, destination.position) > 1)
        {
            yield return null;
            yield return new WaitForSeconds(stuckCheckInterval);

            float distanceMoved = Vector3.Distance(playerTransform.position, lastPosition);

            if (distanceMoved < stuckThreshold)
            {
                timeSinceLastMovement += stuckCheckInterval;

                if (timeSinceLastMovement >= stuckTimeout)
                {
                    Debug.Log("Player is stuck!");
                    navMeshAgent.ResetPath();
                    stuck = true;
                    break;
                }
            }
            else
            {
                timeSinceLastMovement = 0f;
            }

            lastPosition = playerTransform.position;
        }
        if(stuck ==true)
        {
            StartCoroutine(SimulateInteraction());
            stuck = false;
        }
        inMovement = false;
    }

    IEnumerator SimulateInteraction()
    {
        float maxRange = 20f;
        float step = 1f;
        bool interactableFound = false;
        float initialrange = interactionCheckRange;

        while (true)
        {
            for (int i = 0; i < destinations.Count; i++)
            {
                if (Vector3.Distance(playerTransform.position, destinations[i].destinationPoint.transform.position) <= interactionCheckRange)
                {
                    yield return StartCoroutine(DestinationSetter(destinations[i].destinationPoint.transform));

                    var interactable = destinations.Find(x => x.passed == false)?.destinationPoint;
                    if (interactable != null)
                    {
                        interactionFunction.Invoke((MonoBehaviour)interactable);
                    }
                    else
                    {
                        Debug.LogError("No valid destination point found!");
                    }

                    destinations[i].passed = true;
                    interactableFound = true;
                    break; 
                }
            }

            if (interactableFound)
            {
                print(interactionCheckRange);
                interactionCheckRange = initialrange;
                print(interactionCheckRange);
                break;
            }
            interactionCheckRange += step;

            if (interactionCheckRange > maxRange)
            {
                break; 
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(playerTransform.position, interactionCheckRange);
        }
    }
}

[System.Serializable]
public class Destinations
{
    public MonoBehaviour destinationPoint;
    public bool passed;
}
