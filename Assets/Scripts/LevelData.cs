using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Levels", menuName = "Match Tree/Level data", order = 1)]
public class LevelData : ScriptableObject
{
    public List<LevelDescriptor> levels;
}

// the data making up a level
[System.Serializable]
public class LevelDescriptor
{
    // levels can have fewer than the maximum piece types and leaves
    public int numColors = 5;
    public int numLeaves = 9;
    public int width = Board.maxBoardWidth;
    public int height = Board.maxBoardHeight;
    public bool[] map = new bool[Board.maxBoardWidth * Board.maxBoardHeight];
    public LevelDescriptor()
    {
        for (int i = 0; i < Board.maxBoardWidth * Board.maxBoardHeight; i++)
        {
            map[i] = true;
        }
    }

    internal bool GetTile(int x, int y)
    {
        return map[x + y * Board.maxBoardWidth];
    }
}
