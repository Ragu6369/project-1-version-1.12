using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScreen : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject tutorialPanel; // assign your tutorial panel in Inspector 
    private static bool hasShownTutorial = false; // static flag makes the bool value persist across scene reloads
    public GameObject HUDpanel; // reference to the HUD panel

    private void Start() 
    { 
        if (tutorialPanel != null) 
        { 
            if (!hasShownTutorial) 
            { 
                tutorialPanel.SetActive(true);
                Time.timeScale = 0f; // pause game
                HUDpanel.SetActive(false); // hide HUD while tutorial is active

            } 
            else 
            { 
                tutorialPanel.SetActive(false); 
                HUDpanel.SetActive(true); // show HUD if tutorial has already been shown
            } 
        }
    } 
    private void Update() 
    { 
        if (tutorialPanel != null && tutorialPanel.activeSelf) 
        { // Hide tutorial when pressing A (controller) or Space (keyboard)
          if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space)) 
          { 
                tutorialPanel.SetActive(false); 
                Time.timeScale = 1f; // resume game
                HUDpanel.SetActive(true); // show HUD again
                hasShownTutorial = true; // mark tutorial as shown
                                         
          }
        } 
    }
}
