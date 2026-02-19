using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planetNuller : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        DummyScrip_mario playerController = other.GetComponent<DummyScrip_mario>();
        if(playerController != null)
        {
            playerController.currentPlanet = null;
        }
    }
}
