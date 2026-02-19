using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPointPlayer : MonoBehaviour
{

    [Header("WindSetting")]
    public int winValue = 0; // current win count
    public int targetWinValue = 3; // needed points to win
    public GameObject winpanel; // win panel to show when player wins
    public GameObject HUDpanel; // reference to the HUD panel
    void Start()
    {
        if (winpanel != null)
        {
            winpanel.SetActive(false); // hide win panel at start
        }
    }


    void Update()
    {
        if (winValue >= targetWinValue)
        {
            Time.timeScale = 0f; // pause the game 
            if (winpanel != null)
            {
                HUDpanel.SetActive(false); // hide HUD when win panel is active
                winpanel.SetActive(true); // show win panel
            }
        }

        if (winpanel != null && winpanel.activeSelf)
        {
            // restart scene with "A" or "R" Key
            if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f; // resume time before reloading
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            // Exit game with B or E key
            if (Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.E))
            {
                Application.Quit();
                Debug.Log("Game exited!");
            }
        }
    }

    // called by VictoryPlanet to increment winValue
    public void IncrementWin()
    {
        winValue++; // increase win count by 1
    }
}
