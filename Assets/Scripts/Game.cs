using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    // when player gets enough leaves they can break all tiles with that leaf on
    public const int maxLeafQuantity = 120;

    // Set in editor please
    public Board board;
    public SelectionToneAudio toneAudioComponent;
    public PlayerState playerState;
    public LevelData levelData;
    public ScoringData scoringData;
    public ScoreParticles scoreParticles;

    // for matching tiles
    Piece startingPiece;
    PieceMatches matches;

    // stores results from the piece matcher
    List<MatchData> removedItems = new List<MatchData>();

    enum State
    {
        Idle,
        Picking,
        WaitingForTransition
    }

    State state;

    void Start()
    {
        matches = new PieceMatches();
    }

    public void StartNewGame()
    {
        playerState.OnStartNewGame();
        StartLevel(0);
    }

    public void StartLevel(int index)
    {
        StartLevel(levelData.levels[index]);
    }

    public void StartLevel(LevelDescriptor level)
    {
        playerState.OnContinueGame();
        board.CreateBoard(level);
    }

    void Update()
    {
        if (state == State.Idle)
        {
            // check if player got enough leaves to cause a damage event

        }
    }

    public void OnTileMouseClicked(int x, int y, Piece piece)
    {
        if (state != State.Idle) return;
        // is there even a piece here?
        if (piece == null) return;
        state = State.Picking;
        startingPiece = piece;
        piece.Select();
        toneAudioComponent.PlayTone(0);
        matches.Add(piece, x, y);
    }

    public void OnTileMouseEnter(int x, int y, Piece piece)
    {
        // is there even a piece here?
        if (piece == null) return;
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

            matches.RemoveAfter(piece, removedItems);

            foreach (MatchData match in removedItems)
            {
                match.piece.Deselect();
            }
            
            // play the tone for the last selected bubble again
            if (removedItems.Count > 0) // but only if we actually removed some other bubbles
            {
                toneAudioComponent.PlayTone(matches.Count - 1);
            }

            removedItems.Clear();
        }
        else
        {
            Debug.Assert(piece != null);
            Debug.Assert(board.GetTile(x,y).contents == piece);
            // otherwise, add it to the list
            toneAudioComponent.PlayTone(matches.Count);
            matches.Add(piece, x, y);
            piece.Select();
        }
    }

    public void OnTileMouseReleased(int x, int y, Piece piece)
    {
        if (state != State.Picking) return;

        if (matches.Count > 2)
        {
            board.OnRemovePiecesSequenceComplete += OnRemoveSequenceComplete;
            // match made - remove the pieces and shuffle everything down.
            // this also fills in spaces at the top of the board with new pieces, by default
            board.RemoveSequence(matches);

            // if you get to the top note we play a longer musical sequence
            if (matches.Count >= 15)
            {
                toneAudioComponent.PlayGreatSuccessChord();
            }
            else
            {
                toneAudioComponent.PlaySuccessChord();
            }

            state = State.WaitingForTransition;
        }
        else
        {
            // still need to clear and deselect all pieces
            foreach (MatchData match in matches)
            {
                match.piece.Deselect();
            }
            matches.Clear();
            state = State.Idle;
        }
    }

    void OnRemoveSequenceComplete()
    {
        state = State.Idle;

        matches.Clear();

        board.OnRemovePiecesSequenceComplete -= OnRemoveSequenceComplete;
    }

    public void OnPiecePopped(Piece piece, int scoreIndex)
    {
        // to-do: add leaf effects flying towards the counters and stuff here
        playerState.AddLeaves(piece.leafIndex, Random.Range(3, 6));
        // scoring
        int addScore = scoringData.GetPopScore(scoreIndex);
        playerState.score += addScore;
        // UI hint
        scoreParticles.SpawnParticle(piece.transform.position, addScore);
    }
}
