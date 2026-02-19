using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyGravityController))]
public class EnemyAIController : MonoBehaviour
{
    [Header("References")]
    public EnemyGravityController gravityController;
    public Transform player;

    [Header("Patrol")]
    public float waypointReachThreshold = 1.5f;
    public float repickDelay = 1f;

    [Header("Detection")]
    public float detectionDistance = 20f;
    public float fieldOfView = 90f;
    public LayerMask detectionMask;

    private EnemyWaypoint wpScript;
    private bool isChasing = false;
    private bool isHidden = false;
    private Coroutine repickRoutine;

    private void Awake()
    {
        gravityController = GetComponent<EnemyGravityController>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Start()
    {
        if (gravityController.currentPlanet != null)
        {
            wpScript = gravityController.currentPlanet.GetComponent<EnemyWaypoint>();
            gravityController.currentTarget = wpScript.GetNearestWayPoint(transform.position);
        }
    }

    private void Update()
    {
        if (isHidden) return;

        if (PlayerVisibleByRayAndFOV())
        {
            isChasing = true;
            gravityController.currentTarget = player;
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                StartCoroutine(HideThenResumePatrol());
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

        // Line-of-sight raycast
        if (Physics.Raycast(transform.position, toPlayer.normalized, out RaycastHit hit, detectionDistance, detectionMask))
        {
            if (hit.transform == player) return true;
        }
        return false;
    }

    private void Patrol()
    {
        if (gravityController.currentTarget == null && wpScript != null)
        {
            gravityController.currentTarget = wpScript.GetRandomWayPoint();
        }

        if (gravityController.currentTarget != null &&
            Vector3.Distance(transform.position, gravityController.currentTarget.position) <= waypointReachThreshold)
        {
            if (repickRoutine == null)
                repickRoutine = StartCoroutine(RepickAfterDelay());
        }
    }

    private IEnumerator RepickAfterDelay()
    {
        yield return new WaitForSeconds(repickDelay);

        // Fisrt Check Nearest WayPoint 
        Transform nearest = wpScript.GetNearestWayPoint(transform.position);

        // If nearest is the same as current ,pick a random one to keep moving
        if(nearest == gravityController.currentTarget)
        {
            gravityController.currentTarget = wpScript.GetRandomWayPoint();
        }
        else
        {
            gravityController.currentTarget = nearest;
        }   
        repickRoutine = null;
    }

    private IEnumerator HideThenResumePatrol()
    {
        isHidden = true;
        gravityController.currentTarget = null;
        yield return new WaitForSeconds(2f);
        isHidden = false;
        gravityController.currentTarget = wpScript.GetRandomWayPoint();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        // current target (player or waypoint)
        if(gravityController != null && gravityController.currentTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(gravityController.currentTarget.position, 4f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, gravityController.currentTarget.position);

        }
       
        // Draw all waypoints on current planet
        if(gravityController != null && gravityController.currentPlanet != null)
        {
            EnemyWaypoint wpScript = gravityController.currentPlanet.GetComponent<EnemyWaypoint>();
            if(wpScript != null && wpScript.waypoints.Length > 0)
            {
                Gizmos.color = Color.magenta;
                foreach(Transform wp in  wpScript.waypoints)
                {
                    Gizmos.DrawSphere(wp.position, 0.5f);
                    Gizmos.DrawLine(transform.position, wp.position);
                }
            }
        }

        if(wpScript != null)
        {
            Transform nearest = wpScript.GetNearestWayPoint(transform.position);
            if(nearest != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(nearest.position, 4f);
                Gizmos.DrawLine(transform.position, nearest.position);
            }
        }
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



    