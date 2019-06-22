using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    // Set in editor please
    public Board board;

    // for matching tiles
    Piece startingPiece;
    PieceMatches matches;

    enum State
    {
        Idle,
        Picking
    }

    State state;

    void Start()
    {
        matches = new PieceMatches();
        board.CreateBoard();
        board.FillBoard();
    }
    
    void Update()
    {
    }

    public void OnTileMouseClicked(int x, int y, Piece piece)
    {
        state = State.Picking;
        startingPiece = piece;
        piece.Select();
        matches.Add(piece, x, y);
        // Also instruct all pieces to shrink their hitboxes

    }

    public void OnTileMouseEnter(int x, int y, Piece piece)
    {
        // are we idle?
        if (state != State.Picking) return;
        // does the type match?
        if (!piece.IsSameType(startingPiece)) return;
        // is this tile adjacent to the last one?
        MatchData end = matches.GetEnd();
        Debug.Assert(end != null);
        if (Mathf.Abs(x - end.x) > 1) return;
        if (Mathf.Abs(y - end.y) > 1) return;
        // is it already in the list?
        if (matches.Contains(piece))
        {
            // if so, chop off the rest of the line.

            // I didn't want to put the selection logic into PieceMatches so this is how it gets done here, deselect everything, remove some of it, then reselect them all
            foreach (MatchData match in matches)
            {
                match.piece.Deselect();
            }

            matches.RemoveAfter(piece);

            foreach (MatchData match in matches)
            {
                match.piece.Select();
            }
        }
        else
        {
            // otherwise, add it to the list
            matches.Add(piece, x, y);
            piece.Select();
        }
    }

    public void OnTileMouseReleased(int x, int y, Piece piece)
    {

        if (matches.Count > 2)
        {
            board.OnRemovePiecesSequenceComplete += OnRemoveSequenceComplete;
            // match made - remove the pieces and shuffle everything down.
            // this also fills in spaces at the top of the board with new pieces, by default
            board.RemoveSequence(matches);
        }
    }

    void OnRemoveSequenceComplete()
    {
        state = State.Idle;

        matches.Clear();

        board.OnRemovePiecesSequenceComplete -= OnRemoveSequenceComplete;
    }
}
