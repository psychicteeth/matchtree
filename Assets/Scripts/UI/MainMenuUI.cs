using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public GameObject playButton;
    public GameObject continueButton;
    public GameObject confirmUI;
    public List<GameObject> toEnableOnGameStart;
    public Game game;

    public PlayerState playerState;

    private void Start()
    {
        if (!IsSaveGameAvailable())
        {
            continueButton.SetActive(false);
        }
    }

    bool IsSaveGameAvailable()
    {
        return playerState.IsSaveGameAvailable();
    }

    public void OnPlayPressed()
    {
        // need to check if there's a save game and if so open the confirmation dialog
        if (IsSaveGameAvailable())
        {
            confirmUI.SetActive(true);
        }
        else
        {
            // start a new game
            foreach (GameObject go in toEnableOnGameStart)
            {
                go.SetActive(true);
            }
            // disable this menu
            gameObject.SetActive(false);
            
            game.StartNewGame();
        }
    }
    
}
