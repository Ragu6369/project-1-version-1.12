using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaypoint : MonoBehaviour
{
    [Header("WayPoints for enemies on this planet")]
    public Transform[] waypoints; // assign in Inspector (children or empties on the planet surface )

    
    // Get a random wayPoint from this planet
    public Transform GetRandomWayPoint()
    {
        if (waypoints == null || waypoints.Length == 0) return null;
        return waypoints[Random.Range(0, waypoints.Length)];
    }

    // Get the nearest waypoint to a given position
    public Transform GetNearestWayPoint(Vector3 position)
    {
        if (waypoints == null || waypoints.Length == 0) return null;

        float closestDist = Mathf.Infinity;
        Transform nearest = null;

        foreach(Transform wp in waypoints)
        {
            float dist = Vector3.Distance(position, wp.position);
            if(dist < closestDist)
            {
                closestDist = dist;
                nearest = wp;
            }
        }

        return nearest;
    }

    // Get WayPoints By Index(safe wrap-Around)
    public Transform GetWayPointByIndex(int index)
    {
        if (waypoints == null || waypoints.Length == 0) return null;
        return waypoints[index % waypoints.Length];
    }
   
}
