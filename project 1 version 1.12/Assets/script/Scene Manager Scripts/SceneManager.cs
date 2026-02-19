using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private int sceneIndexToLoad = 0; 

    private static bool hasLoaded = false;

    private void Start()
    {
        if (!hasLoaded)
        {
            hasLoaded = true;
            SceneManager.LoadScene(sceneIndexToLoad, LoadSceneMode.Single);
        }
    }
}
