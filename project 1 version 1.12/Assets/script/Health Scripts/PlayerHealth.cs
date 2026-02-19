using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")] 
    public int maxHealth = 100; 
    public int currentHealth; 
    public int damagePerSecond = 10; 

    [Header("UI Settings")] 
    public Image healthBarFill; // fillimage healthbar
    public GameObject deathPanel;
    public GameObject HUDPanel;

    private bool isOnFloor = false;
    private Coroutine damageRoutine;

    private void Start() 
    { 
        currentHealth = maxHealth; 
        UpdateHealthUI(); 

        if(deathPanel != null) 
        { 
            deathPanel.SetActive(false); // hide panel at start
        }
    } 

    private void Update() 
    {
        if(deathPanel != null && deathPanel.activeSelf ) 
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
    private void OnCollisionEnter(Collision collision) 
    { 
        if (collision.gameObject.CompareTag("Floor")) 
        { 
            isOnFloor = true; 
            if (damageRoutine == null) 
            { 
                damageRoutine = StartCoroutine(DamageOverTime()); 
            } 
        } 
    } 
    private void OnCollisionExit(Collision collision) 
    { 
        if (collision.gameObject.CompareTag("Floor")) 
        { 
            isOnFloor = false; 
            if (damageRoutine != null) 
            { 
                StopCoroutine(damageRoutine); 
                damageRoutine = null; 
            } 
        } 
    } 
    private IEnumerator DamageOverTime() 
    { 
        while (isOnFloor) 
        { 
            TakeDamage(damagePerSecond); 
            yield return new WaitForSeconds(1f); // every second
                                                  
        } 
    } 
    private void TakeDamage(int amount) 
    { 
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 
        UpdateHealthUI(); 
        if (currentHealth <= 0) 
        { 
            Debug.Log("Player is dead!"); 
            Time.timeScale = 0f; // pause game on death
            if (deathPanel != null) 
            { 
                HUDPanel.SetActive(false); // hide HUD on death
                deathPanel.SetActive(true); // show panel on death
            }

        } 
    } 
    private void UpdateHealthUI() 
    { 
        if (healthBarFill != null) 
        { 
            float fillAmount = (float)currentHealth / maxHealth; 
            healthBarFill.fillAmount = fillAmount; 
        } 
     }
}
