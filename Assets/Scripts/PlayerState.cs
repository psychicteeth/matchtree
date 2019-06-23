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


    void Start()
    {
        numLeafTypes = leafData.leaves.Count;
        leafScore = new int[numLeafTypes];

        // load from profile
        for (int i = 0; i < numLeafTypes; i++)
        {
            string key = "leafScore" + i;
            leafKeys.Add(key);
            if (PlayerPrefs.HasKey(key))
            {
                leafScore[i] = PlayerPrefs.GetInt(key);
            }
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
}
