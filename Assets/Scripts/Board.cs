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
    // unity's regular sprite is a weird size?
    public const float spriteScale = 0.38f;
    // delays for the sequences
    public const float removeTileDelay = 0.2f;


    // board size
    public int minWidth;
    public int maxWidth;
    public int minHeight;
    public int maxHeight;
    int width;
    int height;

    // scale the board object to fit on screen based on its size
    public float minScale = 0.28f;
    //public float maxScale = 0.53f;

    // board 
    GameObjectPool tilesPool = null;
    public GameObject tilePrefab;
    GameObject[,] boardGOs;
    BoardTile[,] tiles;
    
    // pieces
    public GameObject piecePrefab;
    GameObjectPool piecesPool = null;
    // all the different types of piece we can have
    public List<PieceData> pieceTypes;

    WaitForSeconds removeTileDelayWaiter = new WaitForSeconds(removeTileDelay);

    public System.Action OnRemovePiecesSequenceComplete;

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

    }

    void Update()
    {
    }

    public void CreateBoard()
    {
        DestroyBoard();

        maxWidth = Mathf.Min(maxBoardWidth, maxWidth);
        maxHeight = Mathf.Min(maxBoardHeight, maxHeight);

        width = Random.Range(minWidth, maxWidth);
        height = Random.Range(minHeight, maxHeight);

        // scale and offset to centre
        float scale = minScale; // just use the min scale - changing piece size might feel bad for the player's muscle memory
        transform.localScale = Vector3.one * scale;
        transform.position = new Vector3(-(width - 1) / 2.0f, -(height - 1) / 2.0f, 0) * scale;

        // later we'll refer to a scriptable object to define interesting shapes or do it procedurally
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                boardGOs[x, y] = tilesPool.Get();
                tiles[x, y] = boardGOs[x, y].GetComponent<BoardTile>();
                tiles[x, y].Reset(x, y);
                boardGOs[x, y].SetActive(true);
                // assume board tiles are 1x1 units
                boardGOs[x, y].transform.localPosition = new Vector3(x, y, 0);
            }
        }
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

            piece.Reset(x, y, pieceTypes.GetRandom(), delay);

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
        // this is still column first but should be ok
        for (int x = 0; x < width; x++)
        {
            float delay = 0;
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y].contents == null)
                {
                    // walk up and shuffle everything down
                    int cursor = y + 1;
                    int target = y;
                    while (cursor < height)
                    {
                        Piece piece = tiles[x, cursor].contents;
                        if (piece != null)
                        {
                            tiles[x, target].contents = piece;
                            tiles[x, cursor].contents = null;
                            piece.Fall(target, delay);
                            target++;
                        }
                        cursor++;
                    }

                    if (refill)
                    {
                        // walk up and pop in a new piece to remaining empty cells
                        while (target < height)
                        {
                            AddPiece(x, target, delay);
                            target++;
                        }
                    }

                }

                delay += Random.Range(delayIncrementMin, delayIncrementMax);
            }
        }
    }

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

    IEnumerator RemoveSequenceCoroutine(PieceMatches matches)
    {
        foreach (MatchData match in matches)
        {
            Debug.Assert(match.piece != null);
            match.piece.Deselect();
            int x = match.x;
            int y = match.y;
            Debug.Assert(tiles[x, y].contents != null);
            piecesPool.Return(tiles[x, y].contents.gameObject);
            tiles[x, y].contents = null;

            yield return removeTileDelayWaiter;
        }

        ShuffleDown(true);

        if (OnRemovePiecesSequenceComplete != null) OnRemovePiecesSequenceComplete.Invoke();
    }

    public BoardTile GetTile(int x, int y)
    {
        return tiles[x, y];
    }
}
