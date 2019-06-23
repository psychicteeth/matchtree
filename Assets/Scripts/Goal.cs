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
        ReachScoreInTime,
    }
    public Type type;

    public float timeLimit;
    public long scoreLimit;

    // returns true if this goal has been fulfilled
    public bool Evaluate(PlayerState state)
    {
        switch (type)
        {
            case Type.ReachScore:
                return (state.score >= scoreLimit);
            case Type.ReachScoreInTime:
                return (state.score >= scoreLimit);
            default:
                break;
        }
        return true;
    }
}
