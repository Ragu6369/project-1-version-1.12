using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoad : MonoBehaviour
{
    [SerializeField] private int index = 1;

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Loader();
        }
    }

    public void Loader()
    {
        SceneManager.LoadScene(index);
    }

}
