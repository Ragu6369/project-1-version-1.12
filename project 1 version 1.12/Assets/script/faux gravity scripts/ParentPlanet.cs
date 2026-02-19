using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentPlanet : MonoBehaviour
{
    public  float parent;

    private void SetParentWithoutScaleChange(Transform child,Transform newParent)
    {
        //1.store the current world Scale of the child objcet 
        Vector3 currentWorldScale = child.lossyScale;

        // 2. Set the new parent(true keeps World position/rotation)
        child.SetParent(newParent, true);

        // 3. Calculate the correct loacl scxale to preserve world Scale
        Vector3 newLocalScale;
        if(newParent != null)
        {
            Vector3 parentWorldSacle = newParent.lossyScale;
            newLocalScale = new Vector3(currentWorldScale.x/parentWorldSacle.x,
                currentWorldScale.y/ parentWorldSacle.y,
                currentWorldScale.z / parentWorldSacle.z);
        }
        else
        {
            // if no parent, loacl scale = world scale
            newLocalScale = currentWorldScale;
        }

        // 4. Assign the calculated local scale
        child.localScale = newLocalScale;

    }

    private void OnCollisionEnter(Collision collision)
    {
        DummyScrip_mario playerController = collision.gameObject.GetComponent<DummyScrip_mario>();
        if ((collision.gameObject.CompareTag("Player")) || (collision.gameObject.CompareTag("Enemy")))
        {
            SetParentWithoutScaleChange(collision.transform, transform);
            if (playerController != null)
            {
                playerController.OnceParented = 0;
            }
            parent++;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
       
        if ((collision.gameObject.CompareTag("Player")) || (collision.gameObject.CompareTag("Enemy")))
        {
            SetParentWithoutScaleChange(collision.transform, null);
            parent = 0;
        }
    }
    /*
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player" )
        {
            collision.transform.SetParent(transform);
            parent++;
        }
        
    }
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            collision.transform.SetParent(null);
            parent = 0;
        }
        
    }
    */

}
