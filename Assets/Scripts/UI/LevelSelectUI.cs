using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    // set in editor
    public PlayerState playerState;
    public TMP_Text levelNameLabel;
    public GameObject prevButton;
    public GameObject nextButton;
    public LevelData levelData;
    public Game game;
    public List<GameObject> disableOnPlay = new List<GameObject>();
    public List<GameObject> enableOnPlay = new List<GameObject>();

    int selectedLevel;
    
    // when the level select is activated, go to the right level (the last one played)
    void OnEnable()
    {
        selectedLevel = playerState.lastPlayedLevel;

        UpdateNextPrevButtons();
        UpdateLevel();
    }

    private void UpdateLevel()
    {
        levelNameLabel.text = "Level " + (selectedLevel + 1);
        // to-do: load high score from player state
    }

    public void Play()
    {
        // disable this menu and the main menu
        foreach(GameObject go in disableOnPlay)
        {
            go.SetActive(false);
        }
        // enable the game and the board, enable the game UI
        foreach (GameObject go in enableOnPlay)
        {
            go.SetActive(true);
        }
        // start playing
        game.StartLevel(selectedLevel);
    }

    public void Next()
    {
        selectedLevel++;
        if (selectedLevel >= levelData.levels.Count) selectedLevel = levelData.levels.Count - 1;
        UpdateNextPrevButtons();
        UpdateLevel();
    }

    private void UpdateNextPrevButtons()
    {
        nextButton.SetActive(selectedLevel < levelData.levels.Count - 1);
        prevButton.SetActive(selectedLevel > 0);
    }

    public void Prev()
    {
        selectedLevel--;
        if (selectedLevel < 0) selectedLevel = 0;
        UpdateNextPrevButtons();
        UpdateLevel();
    }
}
