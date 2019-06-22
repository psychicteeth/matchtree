using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardTile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // These references are for floodfills.
    public BoardTile above;
    public BoardTile below;
    public BoardTile left;
    public BoardTile right;
    public Piece contents;
    // rather not keep this here but need to for matching, the way I've done it
    public int x;
    public int y;

    public void Reset(int x, int y)
    {
        this.x = x;
        this.y = y;
        above = below = left = right = null;
        // We don't keep track of the pieces
        Debug.Assert(contents == null, "Please return the piece before resetting the tile!");
        contents = null;
    }

    public void CollectSameTypeNeighbours(Piece referencePiece, PieceMatches matchedTiles)
    {        
        // if the type doesn't match or there's no piece then we don't need to go further
        if (contents == null) return;
        if (!contents.IsSameType(referencePiece)) return;
        // if we've already been here then we can also stop
        if (matchedTiles.Contains(contents)) return;
        matchedTiles.Add(contents, x, y);
        if (above != null) above.CollectSameTypeNeighbours(referencePiece, matchedTiles);
        if (below != null) below.CollectSameTypeNeighbours(referencePiece, matchedTiles);
        if (left != null) left.CollectSameTypeNeighbours(referencePiece, matchedTiles);
        if (right != null) right.CollectSameTypeNeighbours(referencePiece, matchedTiles);
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
