using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int maxBoardWidth = 10;
    public const int maxBoardHeight = 15;
    public const int maxTilesCount = maxBoardWidth * maxBoardHeight;
    // delay between pieces appearing for fancy appearing at board reset
    const float delayIncrement = 0.01f;
    // unity's regular sprite is a weird size?
    public const float spriteScale = 0.38f;

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

    // game code: for matching tiles
    Piece referencePiece;
    PieceMatches matches;

    void Start()
    {
        matches = new PieceMatches();
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

        CreateBoard();
        FillBoard();
    }

    void Update()
    {
    }

    void CreateBoard()
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
                boardGOs[x, y].transform.localScale = Vector3.one * spriteScale;
            }
        }

        // tie up handy references
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x > 1) tiles[x, y].left = tiles[x - 1, y];
                if (y > 1) tiles[x, y].below = tiles[x, y - 1];
                if (x < width - 1) tiles[x, y].right = tiles[x + 1, y];
                if (y < height - 1) tiles[x, y].above = tiles[x, y + 1];
            }
        }
    }

    void DestroyBoard()
    {
        for (int x = 0; x < maxBoardWidth; x++)
        {
            for (int y = 0; y < maxBoardHeight; y++)
            {
                if (boardGOs[x, y] != null)
                {
                    // assumes that the piece component is in the root gameobject of the prefab
                    if (tiles[x, y].contents != null) piecesPool.Return(tiles[x, y].contents.gameObject);
                    tilesPool.Return(boardGOs[x, y]);
                }
                tiles[x, y] = null;
                boardGOs[x, y] = null;
            }
        }
    }

    void FillBoard()
    {
        float delay = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x,y] != null)
                {
                    // get the piece, reset it and set its location
                    GameObject pieceGO = piecesPool.Get();
                    Piece piece = pieceGO.GetComponent<Piece>();
                    piece.Reset(x, y, pieceTypes.GetRandom(), delay);
                    delay += delayIncrement;

                    pieceGO.SetActive(true);
                    
                    // put the piece in the tile
                    tiles[x, y].contents = piece;
                }
            }
        }
    }


    // I wanted to put the game/pieces code in a different class, but had v little time
    void CheckForMatches()
    {
        // operates a kind of floodfill to check pieces for 3 or more hits
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CheckForMatches(x, y);
            }
        }
    }

    public void CheckForMatches(int x, int y)
    {
        if (tiles[x, y] != null && tiles[x, y].contents != null)
        {
            referencePiece = tiles[x, y].contents;

            // recursively go through the neighbours and mark them all as matched
            tiles[x, y].CollectSameTypeNeighbours(referencePiece, matches);

            Debug.Log("Matched " + matches.Count + " tiles.");

            if (matches.Count >= 3)
            {
                // got a match
            }

            matches.Clear();
        }
    }

}
