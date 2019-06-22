using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Describes a piece in the game, its colour, sprite etc.
[CreateAssetMenu(fileName = "Piece", menuName = "Match Tree/Piece data", order = 1)]
public class PieceData : ScriptableObject
{
    public Color color;
    public Color highlightColor;
    public Sprite sprite;
}
