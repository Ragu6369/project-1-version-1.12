using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyGravityController : MonoBehaviour
{
    [Header("Planet & gravity")]
    public Transform currentPlanet;
    public LayerMask planetMask;
    public float gravity = 25f;
    public float rotationSpeed = 8f;
    public float rayCastLength = 3.5f;

    [Header("Movement")]
    public float speed = 2f;
    public Transform currentTarget; // WayPoint or Player
    public bool chasingPlayer = false;

    private Rigidbody rb;
    private Vector3 normalVector = Vector3.up;
    private RaycastHit[] hits;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        UpdateSurfaceNormalWithRays();
        ApplyCustomGravity();
        AlignToSurfaceNormal();
        MoveOnSurface();
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

   private void MoveOnSurface()
    {
        if (currentPlanet == null) return;

        // Tangent direction towards target
        Vector3 toTarget = currentTarget.position - transform.position;
        Vector3 tangent = Vector3.ProjectOnPlane(toTarget, normalVector).normalized;

        // Move enemy
        rb.MovePosition(transform.position + tangent * speed * Time.fixedDeltaTime);

        // Face movement direction
        if(tangent.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(tangent, normalVector);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
