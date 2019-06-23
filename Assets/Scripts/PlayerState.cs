using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    // please set in editor
    public LeafData leafData;
    

    int[] leafScore;
    int numLeafTypes;

    // prefs save/load stuff
    List<string> leafKeys = new List<string>();
    const string gameExistsKey = "MatchTreeSaveGameExists";

    void Start()
    {
        numLeafTypes = leafData.leaves.Count;
        leafScore = new int[numLeafTypes];

        for (int i = 0; i < numLeafTypes; i++)
        {
            string key = "leafScore" + i;
            leafKeys.Add(key);
        }
    }

    public void AddLeaves(int leafType, int amount)
    {
        leafScore[leafType] += amount;
        // write it immediately to playerPrefs
        PlayerPrefs.SetInt(leafKeys[leafType], leafScore[leafType]);
    }

    public int GetLeafCount(int leafType)
    {
        return leafScore[leafType];
    }

    public void Reset()
    {
        for (int i = 0; i < numLeafTypes; i++)
        {
            leafScore[i] = 0;
        }
        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetInt(gameExistsKey, 1);
        for (int i = 0; i < numLeafTypes; i++)
        {
            PlayerPrefs.SetInt(leafKeys[i], leafScore[i]);
        }
    }

    public bool IsSaveGameAvailable()
    {
        return PlayerPrefs.GetInt(gameExistsKey) == 1;
    }

    public void OnStartNewGame()
    {
        Reset();
        Save();
    }

    public void OnContinueGame()
    {
        if (IsSaveGameAvailable())
        {
            // load from profile
            for (int i = 0; i < numLeafTypes; i++)
            {
                if (PlayerPrefs.HasKey(leafKeys[i]))
                {
                    leafScore[i] = PlayerPrefs.GetInt(leafKeys[i]);
                }
                else
                {
                    leafScore[i] = 0;
                }
            }
        }
    }
}
