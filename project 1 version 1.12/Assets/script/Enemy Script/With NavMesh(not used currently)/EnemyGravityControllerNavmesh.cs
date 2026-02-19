using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyGravityControllerNavmesh : MonoBehaviour
{
    [Header("Planet & gravity")]
    public Transform currentPlanet;
    public LayerMask planetMask;
    public float gravity = 25f;
    public float rotationSpeed = 8f;
    public float rayCastLength = 3.5f;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private Vector3 normalVector = Vector3.up;
    private RaycastHit[] hits;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        // Let us control rotation; position remains agent-driven
        agent.updateRotation = false;
        agent.updateUpAxis = false; // if using AI Navigation package that exposes this; otherwise ignored
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        UpdateSurfaceNormalWithRays();
        ApplyCustomGravity();
        AlignToSurfaceNormal();
        ConstrainAgentMovementToSurface();
    }

    public Vector3 GetSurfaceNormal() => normalVector;

    private void UpdateSurfaceNormalWithRays()
    {
        if (currentPlanet == null)
        {
            normalVector = Vector3.up;
            return;
        }

        // Sweep rays to find a reliable surface normal (similar to your player script)
        hits = Physics.RaycastAll(transform.position, -transform.up, rayCastLength, planetMask);
        if (hits.Length == 0) hits = Physics.RaycastAll(transform.position, transform.forward, rayCastLength, planetMask);
        if (hits.Length == 0) hits = Physics.RaycastAll(transform.position, -transform.forward, rayCastLength, planetMask);
        if (hits.Length == 0) hits = Physics.RaycastAll(transform.position, transform.right, rayCastLength, planetMask);
        if (hits.Length == 0) hits = Physics.RaycastAll(transform.position, -transform.right, rayCastLength, planetMask);

        if (hits.Length > 0)
        {
            // Prefer hits against current planet collider
            bool replaced = false;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform == currentPlanet)
                {
                    normalVector = hits[i].normal.normalized;
                    replaced = true;
                    break;
                }
            }
            if (!replaced)
            {
                normalVector = hits[0].normal.normalized;
            }
        }
        else
        {
            // Fallback: radial normal from planet center
            normalVector = (transform.position - currentPlanet.position).normalized;
        }

        hits = System.Array.Empty<RaycastHit>();
    }

    private void ApplyCustomGravity()
    {
        if (currentPlanet == null) return;
        // Gravity pulls towards the center (negative along radial normal)
        rb.AddForce(-normalVector * gravity, ForceMode.Acceleration);
    }

    private void AlignToSurfaceNormal()
    {
        if (currentPlanet == null) return;

        // Align local up to the surface normal
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, normalVector) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void ConstrainAgentMovementToSurface()
    {
        // Project agent’s steering onto the tangent plane to avoid lifting off or digging in
        if (agent.hasPath || agent.desiredVelocity.sqrMagnitude > 0.0001f)
        {
            Vector3 desired = agent.desiredVelocity;
            Vector3 tangent = Vector3.ProjectOnPlane(desired, normalVector);

            // Rotate visual to face tangent motion
            if (tangent.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(tangent.normalized, normalVector);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.fixedDeltaTime);
            }

            // Optional: nudge towards tangent direction to reduce agent drift on curved surfaces
            rb.AddForce(tangent, ForceMode.Acceleration);
        }
    }
}
