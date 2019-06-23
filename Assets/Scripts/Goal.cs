using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for storing goal data as levels will have different ideas of what actually wins
[System.Serializable]
public class Goal 
{
    public enum Type
    {
        ReachScore,
        FillLeafMeters,
        ReachScoreInTime,
    }

    public float timeLimit;
    public int scoreLimit;

    // returns true if this goal has been fulfilled
    public bool Evaluate(PlayerState state)
    {
        return true;
    }
}
