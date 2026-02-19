using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartPos : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint; // Reference to the spawn point transform
    // Start is called before the first frame update
    void Start()
    {
        transform.position = spawnPoint.transform.position;
    }

}
