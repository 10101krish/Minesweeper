using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualText : MonoBehaviour
{
    public Text tilesCountText; 
    public Text congralutionsText; 
    public Text gameOverText; 

    void Start()
    {
        congralutionsText.enabled = false;
        gameOverText.enabled = false;
    }

    public void UpdatRemainingTilesCountText(int newCount)
    {
        tilesCountText.text = newCount.ToString();
    }

    public void EnableCongralutionsText()
    {
        congralutionsText.enabled = true;
    }

    public void EnableGameOverText()
    {
        gameOverText.enabled = true;
    }
}
