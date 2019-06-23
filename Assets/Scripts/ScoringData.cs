using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Helps describe how score goes up.

[CreateAssetMenu(fileName = "Scoring", menuName = "Match Tree/Scoring data", order = 1)]
public class ScoringData : ScriptableObject
{
    public List<int> bubblePopChainScores = new List<int>();

    internal int GetPopScore(int scoreIndex)
    {
        if (bubblePopChainScores.Count == 0) return 10;
        if (scoreIndex >= bubblePopChainScores.Count) return bubblePopChainScores[bubblePopChainScores.Count - 1];
        if (scoreIndex < 0) return bubblePopChainScores[0];
        return bubblePopChainScores[scoreIndex];
    }
}
