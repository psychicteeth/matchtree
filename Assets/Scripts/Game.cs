﻿using System.Collections;
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
    public TMPro.TMP_Text scoreValueText;
    // really need to make a UI state controller instead of having all these disparate things turning objects on and off
    public GameObject levelCompleteUI;
    public GameObject levelFailedUI;

    // for matching tiles
    Piece startingPiece;
    PieceMatches matches;
    List<Piece> pieceSearchCache = new List<Piece>();

    // stores results from the piece matcher
    List<MatchData> removedItems = new List<MatchData>();

    // nothing to do with the PieceMatches class, this searches for the longest possible match for a given piece
    PieceMatchSearchTree longestMatchFinder = new PieceMatchSearchTree();

    // store current level for evaluating goals
    LevelDescriptor currentLevel;

    enum State
    {
        Idle,
        Picking,
        WaitingForTransition,
        Fail
    }

    State state;

    // score lerper
    long scoreLerp;

    // Fail waiter
    WaitForSeconds failWaitDelay = new WaitForSeconds(2.0f);

    void Start()
    {
        longestMatchFinder.board = board;
        matches = new PieceMatches();
        board.OnBoardStateChanged += OnTurn;
        board.OnBoardStateChanged += EvaluateFailState;
    }

    public void StartNewGame()
    {
        playerState.OnStartNewGame();
    }

    public void StartLevel(int index)
    {
        playerState.OnStartedPlayingLevel(index);
        StartLevel(levelData.levels[index]);
    }

    public void StartLevel(LevelDescriptor level)
    {
        state = State.Idle;
        currentLevel = level;
        scoreLerp = 0;
        scoreValueText.text = scoreLerp.ToString("D6");
        playerState.OnContinueGame();
        board.CreateBoard(level);
    }

    void Update()
    {

        if (playerState.score != scoreLerp)
        {
            // lerp score and update display
            long diff = playerState.score - scoreLerp;
            diff /= 2;
            // assumes score always goes up
            if (diff < 1) diff = 1;
            scoreLerp += diff;
            scoreValueText.text = scoreLerp.ToString("D6");
        }

        // evaluate goals - can do this on a callback when the game state changes
        if (state == State.Idle)
        {
            bool complete = true;
            foreach (Goal goal in currentLevel.goals)
            {
                if (!goal.Evaluate(playerState))
                    complete = false;
            }
            if (complete)
            {
                OnLevelComplete();
            }
        }
    }

    public void OnTileMouseClicked(int x, int y, Piece piece)
    {
        if (state != State.Idle) return;
        // is there even a piece here?
        if (piece == null) return;
        if (piece.hp <= 0)
        {
            // to-do: make a bad noise or something
            return;
        }
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
        if (!piece.Matches(startingPiece)) return;
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

    void OnLevelComplete()
    {
        playerState.SaveScore();
        levelCompleteUI.SetActive(true);
    }

    void EvaluateFailState()
    {
        Debug.Assert(state != State.Fail);
        int longestMatch = 0;
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                Piece piece = board.GetPiece(x, y);
                if (piece != null && piece.hp > 0)
                {
                    longestMatch = Mathf.Max(longestMatch, longestMatchFinder.Search(piece, 3));
                }
            }
        }
        //Debug.Log("Longest match found (max 3): " + longestMatch);
        if (longestMatch < 3)
        {
            // set the fail screen on but only after a little while
            StartCoroutine(OpenFailScreen());
            // to prevent anything else happening, put the game into a non-interactive state
            state = State.Fail;
        }
    }

    IEnumerator OpenFailScreen()
    {
        yield return failWaitDelay;
        levelFailedUI.SetActive(true);
    }

    void OnTurn()
    {
        if (state == State.Fail) return;
        // all the pieces get to take a turn
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                Piece piece = board.GetPiece(x, y);
                if (piece != null)
                {
                    piece.OnTurn();
                }
            }
        }
    }
}
