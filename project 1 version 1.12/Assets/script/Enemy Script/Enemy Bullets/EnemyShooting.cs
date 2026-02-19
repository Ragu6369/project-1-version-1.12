using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [Header("References")]
    public EnemyGravityController gravityController; // reference to enemy graviy script 
    public Transform player;
    public GameObject bulletprefab; // To assign in Inspector
    public Transform firePoint; // To assign a child Transform where bullets spawn

    [Header("Shooting Settings")]
    public float fireRate = 1.5f; // seconds between each shots 
    private float fireTimer = 0f;
    public float destroyTime = 10f;

    private void Update()
    {
        if (gravityController == null || player == null) return;

        //Only shoot if chasing Player
        if(gravityController.currentTarget == player)
        {
            fireTimer += Time.deltaTime;
            if(fireTimer >= fireRate)
            {
                Shoot();
                fireTimer = 0f;
            }
        }
    }

    private void Shoot()
    {
        // Instantiate bullet
        GameObject bulletobj = Instantiate(bulletprefab, firePoint.position, firePoint.rotation);

        // Align bullet "Up" to planet normal
        Vector3 normal = gravityController.GetSurfaceNormal();
        Vector3 toPlayer = player.position - firePoint.position;
        Vector3 tangentDir = Vector3.ProjectOnPlane(toPlayer, normal).normalized;

        bulletobj.transform.rotation = Quaternion.LookRotation(tangentDir, normal);

        // Gives Bullet its movement script
        BulletController bullet = bulletobj.GetComponent<BulletController>();
        if (bullet != null)
        {
            bullet.Initialize(normal, tangentDir);
        }

        // Destroy after 4 seconds
        Destroy(bulletobj,destroyTime);
    
    }
}
