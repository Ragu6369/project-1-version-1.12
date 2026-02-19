using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyGravityControllerNavmesh))]
public class EnemyNavAIController : MonoBehaviour
{
    [Header("References")]
    public EnemyGravityControllerNavmesh gravityController;
    public Transform player;

    [Header("Movement & patrol")]
    public float patrolRadiusOnPlanet = 12f;
    public float waypointReachThreshold = 2.0f;
    public float repickDelay = 0.5f;

    [Header("Detection (ray-based + FOV)")]
    public float detectionDistance = 20f;
    public float fieldOfView = 90f;
    public float hideTime = 3f;
    public LayerMask detectionMask;  // include Player and occluders

    private NavMeshAgent agent;
    private bool isChasing = false;
    private bool isHidden = false;
    private Vector3 currentPatrolTarget;
    private Coroutine hideRoutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gravityController = GetComponent<EnemyGravityControllerNavmesh>();

        agent.updateRotation = false; // gravity controller handles rotation
        agent.autoBraking = true;
        agent.autoRepath = true;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Start()
    {
        PickNewPatrolPoint();
    }

    private void Update()
    {
        if (isHidden) return;

        if (PlayerVisibleByRayAndFOV())
        {
            if (!isChasing)
            {
                isChasing = true;
            }
            ChasePlayer();
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                if (hideRoutine != null) StopCoroutine(hideRoutine);
                hideRoutine = StartCoroutine(HideThenResumePatrol());
            }
            else
            {
                Patrol();
            }
        }
    }

    private bool PlayerVisibleByRayAndFOV()
    {
        if (player == null || gravityController.currentPlanet == null) return false;

        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > detectionDistance) return false;

        // FOV test
        Vector3 forwardTangent = Vector3.ProjectOnPlane(transform.forward, gravityController.GetSurfaceNormal()).normalized;
        Vector3 toPlayerTangent = Vector3.ProjectOnPlane(toPlayer, gravityController.GetSurfaceNormal()).normalized;
        float angle = Vector3.Angle(forwardTangent, toPlayerTangent);
        if (angle > fieldOfView * 0.5f) return false;

        // Raycast confirmation (line-of-sight)
        Ray ray = new Ray(transform.position, toPlayer.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, detectionDistance, detectionMask))
        {
            if (hit.transform == player) return true;
        }
        return false;
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        // Continuously steer towards player via NavMesh
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    private void Patrol()
    {
        if (!agent.isOnNavMesh) return;

        if (!agent.hasPath || agent.remainingDistance <= waypointReachThreshold)
        {
            // Small delay to avoid constant resampling on sharp curvature
            StartCoroutine(RepickAfterDelay());
        }
    }

    private IEnumerator RepickAfterDelay()
    {
        yield return new WaitForSeconds(repickDelay);
        PickNewPatrolPoint();
    }

    private IEnumerator HideThenResumePatrol()
    {
        isHidden = true;
        agent.ResetPath();
        yield return new WaitForSeconds(hideTime);
        isHidden = false;
        PickNewPatrolPoint();
    }

    private void PickNewPatrolPoint()
    {
        if (gravityController.currentPlanet == null) return;

        // Sample a target on the planet’s NavMesh near a random radial direction
        Vector3 planetCenter = gravityController.currentPlanet.position;
        Vector3 radialDir = Random.onUnitSphere.normalized;
        Vector3 worldGuess = planetCenter + radialDir * patrolRadiusOnPlanet;

        if (NavMesh.SamplePosition(worldGuess, out NavMeshHit navHit, patrolRadiusOnPlanet, NavMesh.AllAreas))
        {
            currentPatrolTarget = navHit.position;
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(currentPatrolTarget);
            }
        }
        else
        {
            // Fallback: try closer to current position
            Vector3 fallback = transform.position + Vector3.ProjectOnPlane(Random.insideUnitSphere * patrolRadiusOnPlanet, gravityController.GetSurfaceNormal());
            if (NavMesh.SamplePosition(fallback, out navHit, patrolRadiusOnPlanet, NavMesh.AllAreas))
            {
                currentPatrolTarget = navHit.position;
                if (agent.isOnNavMesh)
                {
                    agent.SetDestination(currentPatrolTarget);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(currentPatrolTarget, 0.3f);

        // FOV visualization (projected)
        if (gravityController != null)
        {
            Vector3 up = gravityController.GetSurfaceNormal();
            Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, up).normalized;
            Quaternion left = Quaternion.AngleAxis(-fieldOfView * 0.5f, up);
            Quaternion right = Quaternion.AngleAxis(fieldOfView * 0.5f, up);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + left * fwd * detectionDistance);
            Gizmos.DrawLine(transform.position, transform.position + right * fwd * detectionDistance);
        }
    }
}
