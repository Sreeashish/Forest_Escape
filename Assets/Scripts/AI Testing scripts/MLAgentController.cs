using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Events;

public class MLAgentController : Agent
{
    public Transform endPoint;
    public Transform[] waypoints;
    public Transform spawnPoint;
    public LayerMask obstacleMask;
    public LayerMask coverMask;
    public float moveSpeed = 3f;
    public float rotationSpeed = 360f;
    public float visionRange = 10f;
    public Rigidbody rigidBody;
    public string interactableTag, interactableClassName, interactionFunctionName;
    public Type interactableClass;

    private int currentWaypointIndex = 0;
    private float previousDistanceToEnd;
    private Vector3 previousPosition;

    void Start()
    {
        if (!string.IsNullOrEmpty(interactableClassName))
        {
            interactableClass = Type.GetType(interactableClassName);
        }
        previousPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = spawnPoint.position + Vector3.up * 0.1f;
        transform.rotation = Quaternion.identity;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        previousDistanceToEnd = Vector3.Distance(transform.localPosition, endPoint.localPosition);
        previousPosition = transform.position;

        currentWaypointIndex = 0;
        Debug.Log($"Episode Ended. Total Reward: {GetCumulativeReward()}");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);

        if (currentWaypointIndex < waypoints.Length)
        {
            sensor.AddObservation(waypoints[currentWaypointIndex].localPosition);
        }
        else
        {
            sensor.AddObservation(endPoint.localPosition);
        }

        float distanceToTarget = Vector3.Distance(transform.localPosition, GetCurrentTarget().localPosition);
        sensor.AddObservation(distanceToTarget);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveForward = actions.ContinuousActions[0];
        float moveSideways = actions.ContinuousActions[1];
        float interactAction = actions.ContinuousActions[2];

        Vector3 forwardMovement = transform.forward * moveForward * moveSpeed * Time.deltaTime;
        Vector3 sidewaysMovement = transform.right * moveSideways * moveSpeed * Time.deltaTime;

        rigidBody.MovePosition(rigidBody.position + forwardMovement + sidewaysMovement);

        float distanceToTarget = Vector3.Distance(transform.localPosition, GetCurrentTarget().localPosition);

        float distanceTraveled = Vector3.Distance(previousPosition, transform.position);
        AddReward(distanceTraveled * 0.001f);
        previousPosition = transform.position;

        if (distanceToTarget < previousDistanceToEnd)
        {
            AddReward(0.01f);
        }
        else
        {
            AddReward(-0.005f);
        }

        previousDistanceToEnd = distanceToTarget;

        if (distanceToTarget < 1.5f)
        {
            if (currentWaypointIndex < waypoints.Length)
            {
                AddReward(0.5f);
                Debug.Log($"Waypoint {currentWaypointIndex + 1} reached!");
                currentWaypointIndex++;
            }
            else if (currentWaypointIndex == waypoints.Length)
            {
                AddReward(1.0f);
                Debug.Log("DESTINATION REACHED");
                EndEpisode();
            }
        }

        if (interactAction > 0.5f)
        {
            FindInteractables();
        }
    }

    private Transform GetCurrentTarget()
    {
        if (currentWaypointIndex < waypoints.Length)
        {
            return waypoints[currentWaypointIndex];
        }
        return endPoint;
    }

    private void FindInteractables()
    {
        if (string.IsNullOrEmpty(interactableTag) || interactableClass == null)
        {
            Debug.LogWarning("Interactable tag or class is not defined.");
            return;
        }

        GameObject[] allInteractables = GameObject.FindGameObjectsWithTag(interactableTag);

        foreach (GameObject obj in allInteractables)
        {
            float distanceToObj = Vector3.Distance(transform.position, obj.transform.position);
            if (distanceToObj < 2f)
            {
                Debug.Log("Interactable found: " + obj.name);
                MonoBehaviour interactableCls = obj.GetComponent(interactableClass) as MonoBehaviour;

                if (interactableCls != null)
                {
                    InteractWith(interactableCls);
                }
            }
        }
    }

    void InteractWith(MonoBehaviour interactable)
    {
        if (interactable == null || string.IsNullOrEmpty(interactionFunctionName))
        {
            Debug.LogWarning("Interaction failed: No valid method or function name.");
            return;
        }

        System.Reflection.MethodInfo function = interactable.GetType().GetMethod(interactionFunctionName);

        if (function != null)
        {
            function.Invoke(interactable, null);
        }
        else
        {
            Debug.LogWarning($"Method '{interactionFunctionName}' not found on {interactable.name}");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");
        continuousActions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & obstacleMask) != 0)
        {
            AddReward(-0.5f);
            Debug.Log("Hit an obstacle! Stronger penalty applied.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        if (waypoints != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawWireSphere(waypoints[i].position, 1f);
            }
        }
    }
}
