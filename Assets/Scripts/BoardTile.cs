using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardTile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    public Piece contents;
    // rather not keep this here but need to for matching, the way I've done it
    public int x;
    public int y;

    static Board board = null;
    static Game game = null;

    public float colliderRadiusLarge;
    public float colliderRadiusSmall;

    // set in prefab please!
    new public CircleCollider2D collider;

    void Start()
    {
        // assume one of each in scene
        if (board == null) board = FindObjectOfType<Board>();
        if (game == null) game = FindObjectOfType<Game>();
    }

    public void Reset(int x, int y)
    {
        this.x = x;
        this.y = y;
        // We don't keep track of the pieces
        Debug.Assert(contents == null, "Please return the piece before resetting the tile!");
        contents = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        game.OnTileMouseClicked(x, y, contents);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        game.OnTileMouseReleased(x, y, contents);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        game.OnTileMouseEnter(x, y, contents);
    }

    public void SetSmallCollider()
    {
        collider.radius = colliderRadiusSmall;
    }
    public void SetLargeCollider()
    {
        collider.radius = colliderRadiusLarge;
    }
}
