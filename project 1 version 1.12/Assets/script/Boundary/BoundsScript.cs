using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsScript : MonoBehaviour
{
    [Header("Boundary Settings")] 
    public Transform player;
    public GameObject boundaryObject; // To assign the GameObject with SphereCollider (Is Trigger = true) ]
    private SphereCollider sphereCollider; 
    private void Start() 
    { 
        if (boundaryObject != null)
        { 
            sphereCollider = boundaryObject.GetComponent<SphereCollider>(); 
            if (sphereCollider == null) 
            { 
                Debug.LogError("Boundary object must have a SphereCollider!"); 
            } 
        } 
    } 
    private void LateUpdate()
    { 
        if (player != null && sphereCollider != null) 
        { 
            
            Vector3 center = sphereCollider.transform.position; 
            float radius = sphereCollider.radius * sphereCollider.transform.localScale.x; 
            Vector3 offset = player.position - center; 
            // If player is outside the sphere, clamp the player back to the surface
            if (offset.magnitude > radius) 
            {
               player.position = center + offset.normalized * radius;
            } 
        } 
    } 
    // Draw gizmo outline in Scene view
    private void OnDrawGizmos() 
    { 
        if (boundaryObject != null) 
        { 
            SphereCollider sc = boundaryObject.GetComponent<SphereCollider>(); 
            if (sc != null) 
            { 
                Gizmos.color = Color.green; 
                float radius = sc.radius * boundaryObject.transform.localScale.x; 
                Gizmos.DrawWireSphere(boundaryObject.transform.position, radius); 
            } 
        } 
    }
}
