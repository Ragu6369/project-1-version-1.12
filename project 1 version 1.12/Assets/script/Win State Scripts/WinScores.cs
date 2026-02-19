using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinScores : MonoBehaviour
{
    [Header("UI Settings")] 
    public Image targetImage; // assign the UI Image in Inspector
    public Sprite[] winSprites; // assign sprites in order (0,1,2,3...)
    
    [Header("Player Reference")] 
    public WinPointPlayer playerWin; // Player (with PlayerWin script)
    
    private void Update() 
    {
        if (playerWin != null && targetImage != null && winSprites.Length > 0) 
        {
            int index = Mathf.Clamp(playerWin.winValue, 0, winSprites.Length - 1);
            targetImage.sprite = winSprites[index];
        }
    }
}
