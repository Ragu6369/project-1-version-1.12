using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlanetManager : MonoBehaviour
{
    [Header("Planets & enemy spawning")]
    public List<Transform> planets = new List<Transform>();
    public GameObject enemyPrefab;
    public int enemiesPerPlanet = 3;
    public float spawnSearchRadius = 15f;
    public LayerMask planetMask;

    private void Start()
    {
        SpawnEnemiesOnPlanets();
    }

    private void SpawnEnemiesOnPlanets()
    {
        foreach (Transform planet in planets)
        {
            for (int i = 0; i < enemiesPerPlanet; i++)
            {
                // Instead of guesssing inside a sphere , pick a point on the planet surface 
                // Replace this line:
                // Vector3 guess = planet.position + dir * spawnSearchRadius;
                // With:
                SphereCollider sphere = planet.GetComponent<SphereCollider>();
                float radius = sphere != null ? sphere.radius * planet.localScale.x : spawnSearchRadius;
                Vector3 dir = Random.onUnitSphere.normalized;
                Vector3 surfacePoint = planet.position + dir * radius;

                // Sample NavMesh at the surface point 
                Vector3 spawnPos;
                if(NavMesh.SamplePosition(surfacePoint, out NavMeshHit hit,20f , NavMesh.AllAreas))
                {
                    spawnPos = hit.position;
                }
                else
                {
                    Debug.LogWarning("No NavMesh found near surface point,skipping spawn");
                    spawnPos = surfacePoint; // fallback if no NavMesah;
                    continue;
                }

                /*
                if (!NavMesh.SamplePosition(guess, out NavMeshHit hit, spawnSearchRadius, NavMesh.AllAreas))
                {
                    // Fallback: try closer to the surface under the guess using a ray onto the planet
                    if (Physics.Raycast(guess, (planet.position - guess).normalized, out RaycastHit rh, spawnSearchRadius, planetMask))
                    {
                        Vector3 nearSurface = rh.point + rh.normal * 0.5f;
                        NavMesh.SamplePosition(nearSurface, out hit, 5f, NavMesh.AllAreas);
                    }
                }
                

                Vector3 spawnPos = hit.position != Vector3.zero ? hit.position : guess;

                */

                if(float.IsNaN(spawnPos.x) || float.IsInfinity(spawnPos.x) ||
                    float.IsNaN(spawnPos.y) || float.IsInfinity(spawnPos.y) ||
                    float.IsNaN(spawnPos.z) || float.IsInfinity(spawnPos.z))
                {
                    Debug.LogWarning("Invalid spawn position detected, skipping enemy spawn");
                    continue; // skip this enemy 
                }
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                var gravity = enemy.GetComponent<EnemyGravityControllerNavmesh>();
                var ai = enemy.GetComponent<EnemyNavAIController>();
                gravity.currentPlanet = planet;

                // Optional: point forward along tangent to start
                Vector3 normal = (spawnPos - planet.position).normalized;
                Vector3 tangentFwd = Vector3.ProjectOnPlane(Random.insideUnitSphere, normal).normalized;

                if(tangentFwd.sqrMagnitude > 0.0001f)
                {
                    enemy.transform.rotation = Quaternion.LookRotation(tangentFwd, normal);
                }
                else
                {
                    enemy.transform.rotation = Quaternion.identity; // fallback
                }


                // Ensure agent is on navmesh
                var agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null && !agent.isOnNavMesh)
                {
                    // small reposition attempt
                    if (NavMesh.SamplePosition(spawnPos, out NavMeshHit snap, 5f, NavMesh.AllAreas))
                    {
                        enemy.transform.position = snap.position;
                    }
                    else
                    {
                        Debug.LogWarning("Enemy not close Enough to NavMesh,Destroying the enemy");
                        Destroy(enemy);
                        continue; // skip to next enemy
                    }
                }
            }
        }
    }
}
