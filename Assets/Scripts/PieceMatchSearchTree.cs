using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Given a Piece, and a Board, searches the board for the longest match possible from that piece
public class PieceMatchSearchTree 
{
    public Board board;

    List<Piece> currentMatch = new List<Piece>();
    int largestMatch = 0;
    // this calculation can get pretty time-consuming and we don't care above a certain length at the moment
    // so we can early out
    int maxMatchLength = 3;

    // Pass in the maximum length of the chain that you care about. Passing in
    // something like 20, and then running it on a group of pieces with lots of 
    // connections (e.g. a big square of pieces) will take a very long time to
    // evaluate (300ms+)
    public int Search(Piece startingPiece, int maxMatchLength)
    {
        // removed temp for debugging
        //this.maxMatchLength = maxMatchLength;

        if (startingPiece.hp <= 0) return 0;

        currentMatch.Clear();

        largestMatch = 0;
        // recursive function
        SearchRecursive(startingPiece);

        return largestMatch;
    }

    void SearchRecursive(Piece piece)
    {
        //if (largestMatch > maxMatchLength) return;
        // given a Piece
        // add it to the match list
        currentMatch.Add(piece);
        // find all the neighbours
        int px = piece.x;
        int py = piece.y;
        for(int x = px - 1; x <= px + 1; x++)
        {
            for (int y = py - 1; y <= py + 1; y++)
            {
                Piece p = board.GetPiece(x, y);
                if (p != null && p != piece && p.hp > 0 && p.Matches(piece) && !currentMatch.Contains(p))
                {
                    SearchRecursive(p);
                }
            }
        }
        // if there are none left, evaluate the match list length and increase the max length if need be
        largestMatch = Mathf.Max(largestMatch, currentMatch.Count);
        // done - remove me
        currentMatch.RemoveAt(currentMatch.Count-1);
    }

}
