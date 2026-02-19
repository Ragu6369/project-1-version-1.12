using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Physics")]
    public float speed = 10f;
    public float gravity = 25f;
    public float rotationSpeed = 8f;
    public float rayCastLength = 3.5f;
    public LayerMask planetMask;

    private Vector3 normal;
    private Vector3 direction;
    private Transform currentPlanet;
    private Rigidbody rb;
    private RaycastHit[] hits;

    public void Initialize(Vector3 planetNormal, Vector3 tangentDirection)
    {
        normal = planetNormal;
        direction = tangentDirection;

        // Estimate planet center from normal
        currentPlanet = FindClosestPlanet();

    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    private void FixedUpdate()
    {
        UpdateSurfaceNormalWithRays();
        ApplyCustomGrvaity();
        AlignToSurfaceNormal();
        MoveOnSurafce();
    }

    private void MoveOnSurafce()
    {
        Vector3 tangent = Vector3.ProjectOnPlane(direction, normal).normalized;
        rb.MovePosition(transform.position + tangent * speed * Time.fixedDeltaTime);
    }

    private void ApplyCustomGrvaity()
    {
        if (currentPlanet == null) return;
        rb.AddForce(-normal * gravity, ForceMode.Acceleration);
    }

    private void AlignToSurfaceNormal()
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction, normal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void UpdateSurfaceNormalWithRays()
    {
        if (currentPlanet == null)
        {
            normal = Vector3.up;
            return;
        }

        hits = Physics.RaycastAll(transform.position, -transform.up, rayCastLength, planetMask);
        if (hits.Length == 0) hits = Physics.RaycastAll(transform.position, transform.forward, rayCastLength, planetMask);
        if (hits.Length == 0) hits = Physics.RaycastAll(transform.position, -transform.forward, rayCastLength, planetMask);
        if (hits.Length == 0) hits = Physics.RaycastAll(transform.position, -transform.right, rayCastLength, planetMask);
        if (hits.Length == 0) hits = Physics.RaycastAll(transform.position, transform.right, rayCastLength, planetMask);

        if (hits.Length > 0)
        {
            bool replaced = false;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform == currentPlanet)
                {
                    normal = hits[i].normal.normalized;
                    replaced = true;
                    break;
                }
            }
            if (!replaced)
            {
                normal = hits[0].normal.normalized;
            }
        }
        else
        {
            normal = (transform.position - currentPlanet.position).normalized;
        }
        hits = System.Array.Empty<RaycastHit>();
    }

    private Transform FindClosestPlanet()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 100f, planetMask);
        float closestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (Collider col in colliders)
        {
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = col.transform;
            }
        }
        return nearest;
    }
}

