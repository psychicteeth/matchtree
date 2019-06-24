using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int maxBoardWidth = 7;
    public const int maxBoardHeight = 9;
    public const int maxTilesCount = maxBoardWidth * maxBoardHeight;

    // delay between pieces appearing for fancy appearing at board reset
    const float delayIncrementMin = 0.02f;
    const float delayIncrementMax = 0.04f;
    const float delayIncrementColumn = 0.06f;

    // delays for the sequences
    const float removeTileDelayMax = 0.2f;
    const float removeTileDelayMin = 0.05f;
    const int removeWaiterArraySize = 16;

    // board size
    public int width { get; private set; }
    public int height { get; private set; }

    // board 
    GameObjectPool tilesPool = null;
    GameObject[,] boardGOs;
    BoardTile[,] tiles;

    // pool of pieces
    GameObjectPool piecesPool = null;

    // Assign in editor please >>>>
    // scale the board object to fit on screen based on its size
    public float minScale = 0.28f;
    public Vector3 screenOffset;
    public GameObject tilePrefab;
    public GameObject piecePrefab;
    // all the different types of piece we can have
    public List<PieceData> pieceTypes;
    // for the pieces exploding.
    public ParticleSystem particles;
    public AudioClip pieceExplodeSound;
    public AudioClip dripSound;
    public Game game;


    // These allocate garbage in coroutines so it's best to preallocate them.
    // unfortunately you can't set the time after creating them so you gotta make one for every different time value you want
    WaitForSeconds[] removeTileDelayWaiters = new WaitForSeconds[removeWaiterArraySize];
    public System.Action OnRemovePiecesSequenceComplete;
    public System.Action OnBoardStateChanged;

    // I don't know how mobile audio performance is. Might be better performance-wise to have a set of variously-pitched sound clips
    // instead of having a set of variously-pitched sources.
    List<AudioSource> randomPitchSources = new List<AudioSource>();
    public float popVolume = 1.0f;
    public float dripVolume = 1.0f;

    // dripping sounds for when the pieces bounce - only play this once per N frames max
    const int dripSoundFrameDelayMin = 0;
    const int dripSoundFrameDelayMax = 3;
    int dripSoundFrameCounter = 0;
    bool playDripSound = false;

    // keep ahold of the current level 
    LevelDescriptor currentLevel;

    void Awake()
    {
        GameObject container = new GameObject();
        container.transform.SetParent(transform);
        container.name = "Tiles";
        tilesPool = new GameObjectPool(tilePrefab, maxTilesCount, container);
        tiles = new BoardTile[maxTilesCount, maxTilesCount];
        boardGOs = new GameObject[maxTilesCount, maxTilesCount];

        // allocate enough pieces to fill the board, and a second lot so things can animate, like if we want to clear the board and animate a new lot of pieces coming in
        container = new GameObject();
        container.transform.SetParent(transform);
        container.name = "Pieces";
        piecesPool = new GameObjectPool(piecePrefab, maxTilesCount * 2, container);

        // waiters
        for(int i = 0; i < removeWaiterArraySize; i++)
        {
            removeTileDelayWaiters[i] = new WaitForSeconds(Mathf.SmoothStep(removeTileDelayMax, removeTileDelayMin, (float)i / (float)removeWaiterArraySize));
        }

        // Sounds
        for (int i = 0; i < 12; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            randomPitchSources.Add(source);
            source.pitch = Random.Range(0.9f, 1.3f);
        }

        gameObject.SetActive(false);
    }

    // do the drip sound in late update as the pieces update in update and we want to play the sounds on the same frame if poss
    private void LateUpdate()
    {
        if (dripSoundFrameCounter > 0) dripSoundFrameCounter--;

        if (playDripSound && dripSoundFrameCounter == 0)
        {
            playDripSound = false;
            dripSoundFrameCounter = Random.Range(dripSoundFrameDelayMin, dripSoundFrameDelayMax);
            randomPitchSources.GetRandom().PlayOneShot(dripSound, dripVolume);
        }
    }

    public void CreateBoard(LevelDescriptor level)
    {
        currentLevel = level;

        DestroyBoard();

        width = level.width;
        height = level.height;

        // scale and offset to centre
        float scale = minScale; // easier just to use the min scale - also changing piece size might feel bad for the player's muscle memory?
        transform.localScale = Vector3.one * scale;
        transform.position = new Vector3(-(width - 1) / 2.0f, -(height - 1) / 2.0f, 0) * scale + screenOffset;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (level.GetTile(x, y))
                {
                    boardGOs[x, y] = tilesPool.Get();
                    tiles[x, y] = boardGOs[x, y].GetComponent<BoardTile>();
                    tiles[x, y].Reset(x, y);
                    boardGOs[x, y].SetActive(true);
                    // assume board tiles are 1x1 world units in size
                    boardGOs[x, y].transform.localPosition = new Vector3(x, y, 0);
                }
            }
        }

        FillBoard();
    }

    public void DestroyBoard()
    {
        for (int x = 0; x < maxBoardWidth; x++)
        {
            for (int y = 0; y < maxBoardHeight; y++)
            {
                if (boardGOs[x, y] != null)
                {
                    // assumes that the piece component is in the root gameobject of the prefab
                    if (tiles[x, y].contents != null)
                    {
                        piecesPool.Return(tiles[x, y].contents.gameObject);
                        tiles[x, y].contents = null;
                    }
                    tilesPool.Return(boardGOs[x, y]);
                }
                tiles[x, y] = null;
                boardGOs[x, y] = null;
            }
        }
    }

    // Fills the board with random pieces.
    public void FillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            float delay = x * delayIncrementColumn;
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] != null)
                {
                    AddPiece(x, y, delay);
                    delay += Random.Range(delayIncrementMin, delayIncrementMax);
                }
            }
        }
    }

    // adds a piece to the board on tile at (x, y). optional delay allows for an artful entry for many simultaneous pieces
    private void AddPiece(int x, int y, float delay = 0)
    {
        if (tiles[x, y].contents == null)
        {
            // get the piece, reset it and set its location
            GameObject pieceGO = piecesPool.Get();
            Piece piece = pieceGO.GetComponent<Piece>();

            pieceGO.SetActive(true);

            piece.Reset(x, y, pieceTypes.GetRandom(0, currentLevel.numColors), Random.Range(0, currentLevel.numLeaves), delay);

            // put the piece in the tile
            tiles[x, y].contents = piece;
        }
    }

    // instantly remove all pieces in the given match set
    public void RemovePieces(PieceMatches matches)
    {
        foreach(MatchData match in matches)
        {
            RemovePiece(match);
        }
    }

    private void RemovePiece(MatchData match)
    {
        match.piece.Deselect();
        int x = match.x;
        int y = match.y;
        piecesPool.Return(tiles[x, y].contents.gameObject);
        tiles[x, y].contents = null;
    }

    public void ShuffleDown(bool refill = true)
    {
        for (int x = 0; x < width; x++)
        {
            float delay = 0;
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] != null && tiles[x, y].contents == null)
                {
                    // walk up and shuffle everything down
                    // I looked at fetching tiles from adjacent columns and tried to implement it
                    // and ran out of time.
                    int cursor = y + 1;
                    int target = y;
                    while (cursor < height)
                    {
                        // if there is no tile there, Fill the tile with a new piece.
                        if (tiles[x, cursor] == null)
                        {
                            AddPiece(x, target, delay);
                            // we can break out of the loop here. Algorithm will visit any squares above.
                            break;                            
                        }
                        else
                        {
                            Piece piece = tiles[x, cursor].contents;
                            if (piece != null)
                            {
                                tiles[x, target].contents = piece;
                                tiles[x, cursor].contents = null;
                                piece.Fall(target, delay);
                                delay += Random.Range(delayIncrementMin, delayIncrementMax);
                                target++;
                            }
                            // search further up for something to move into this tile.
                            // if we get to the top without finding anything, we'll use target 
                            // as a reference to fill up the holes we left
                            cursor++;
                        }
                    }

                    if (refill)
                    {
                        // walk up and pop in a new piece to remaining empty cells
                        while (target < height)
                        {
                            AddPiece(x, target, delay);
                            target++;
                            if (tiles[x, target] == null) break;
                        }
                    }
                }
            }
        }
    }

    // the colliders change size depending on selection state - it's to make diagonal selection easier
    public void SetSmallColliders()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] != null)
                {
                    tiles[x, y].SetSmallCollider();
                }
            }
        }
    }

    public void SetLargeColliders()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] != null)
                {
                    tiles[x, y].SetLargeCollider();
                }
            }
        }
    }

    public void RemoveSequence(PieceMatches matches)
    {
        StartCoroutine(RemoveSequenceCoroutine(matches));
    }

    // Remove a set of pieces from the board in a timed sequence
    IEnumerator RemoveSequenceCoroutine(PieceMatches matches)
    {
        int i = 0;
        int scoreIndex = 0;
        foreach (MatchData match in matches)
        {
            Debug.Assert(match.piece != null);

            // inform game of this so that scores can be added etc.
            game.OnPiecePopped(match.piece, scoreIndex);

            int x = match.x;
            int y = match.y;
            Debug.Assert(tiles[x, y].contents != null);
            
            // return the piece to the pile - it'll get deactivated here, and reset when next requested
            piecesPool.Return(tiles[x, y].contents.gameObject);

            // tile no longer contains a piece
            tiles[x, y].contents = null;

            // To cause the particles to burst again they need to be stopped / moved / started
            particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            particles.transform.position = tiles[x, y].transform.position;
            particles.Play();

            // pop sound
            randomPitchSources.GetRandom().PlayOneShot(pieceExplodeSound, popVolume);

            yield return removeTileDelayWaiters[i];

            // wait gets shorter with each pop so it speeds up
            i = Mathf.Min(i + 1, removeWaiterArraySize - 1);

            // next score
            scoreIndex++;
        }

        // drop all the floating pieces now
        ShuffleDown(true);

        // all done - let whoever wants to know know
        if (OnBoardStateChanged != null) OnBoardStateChanged.Invoke();
        if (OnRemovePiecesSequenceComplete != null) OnRemovePiecesSequenceComplete.Invoke();
    }

    public BoardTile GetTile(int x, int y)
    {
        return tiles[x, y];
    }

    public void PlayDripSound()
    {
        playDripSound = true;
    }

    public Piece GetPiece(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= width) return null;
        BoardTile tile = GetTile(x, y);
        if (tile == null) return null;
        return tile.contents;
    }
}
