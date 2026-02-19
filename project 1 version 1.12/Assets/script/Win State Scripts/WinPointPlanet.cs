using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPointPlanet : MonoBehaviour
{
    private bool hasBeenTouched = false; // to prevent multiple triggers

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenTouched && collision.gameObject.CompareTag("Player"))
        {
            hasBeenTouched = true; // mark as touched to prevent multiple triggers
            WinPointPlayer playerWinScript = collision.gameObject.GetComponent<WinPointPlayer>();
            if (playerWinScript != null)
            {
                playerWinScript.IncrementWin(); // increment player's win count
            }
        }
    }
}


