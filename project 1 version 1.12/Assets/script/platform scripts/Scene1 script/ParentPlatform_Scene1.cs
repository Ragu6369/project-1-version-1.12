using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentPlatform_Scene1 : MonoBehaviour
{
    public float parent;

    private void OnTriggerEnter(Collider other)
    {
        other.transform.SetParent(transform);
        //if(other.CompareTag("Player"))
        {
            // other.transform.SetParent(transform);
            // parent++;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        other.transform.SetParent(null);
        // if (other.CompareTag("Player"))
        {
            // other.transform.SetParent(null);
            // parent = 0;
        }

    }



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.transform.SetParent(transform);
            parent++;
        }


    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.transform.SetParent(null);
            parent = 0;
        }

    }

    /*
    private void SetParentWithoutScaleChange(Transform child,Transform newParent)
    {
        // 1. store the current world scale of the child object 
        Vector3 currentWorldScale = child.lossyScale;

        // 2. Set the new parent (true keeps world position/roatation)
        child.SetParent(newParent, true);

        // 3. Calculate the correctlocal scale to preserve world scale
        Vector3 newLocalScale;
        if(newParent != null)
        {
            Vector3 parentWorldScale = newParent.lossyScale;
            newLocalScale = new Vector3(currentWorldScale.x / parentWorldScale.x, 
                currentWorldScale.y / parentWorldScale.y, 
                currentWorldScale.z / parentWorldScale.z);
        }
        else
        {
            // if no parent,local scale = world scale 
            newLocalScale = currentWorldScale;
        }

        //4.Assign the calculated LOlacl Scale
        child.localScale = newLocalScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            SetParentWithoutScaleChange(collision.transform, transform.transform);
            parent++;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            SetParentWithoutScaleChange(collision.transform, null);
            parent = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if this platform entered a Planet trigger
        if(other.CompareTag("Planet"))
        {
            //If thye Player is currently parented to the platform, detach the player from platform
            foreach(Transform child in transform)
            {
                if(child.CompareTag("Player"))
                {
                    //Detach from platform
                    child.SetParent(null);


                    // Update the player's currentPlanet reference
                    DummyScrip_mario playerController = child.GetComponent<DummyScrip_mario>();
                    Rigidbody playerRb = child.GetComponent<Rigidbody>();

                    if(playerController != null && playerRb != null)
                    {
                        // 1. Make the player Jump before switching planets
                        Vector3 jumpDir = child.up; // Jump along current up
                        playerRb.velocity = Vector3.zero; // Clear old Velocity
                        playerRb.AddForce(jumpDir * playerController.jumpForce * 2 , ForceMode.Impulse);

                        //Set Planet as to Parent
                        //child.SetParent(other.transform);
                        playerController.currentPlanet = other.transform;
                        playerController.isTouchingPlanetSurface = true;
                        playerController.EnterNewGravityField();
                    }
                }
            }
        }
    }
    */
}
