using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour
{
    float scale = 0;
    float targetScale = Board.spriteScale;
    int x;
    int y;
    float ySpeed = 0;
    float delayTimer = 0;
    PieceData type;

    static float gravity = -1.0f;
    static float bounceFriction = 0.2f;
    const float minSpeed = 0.1f;

    // Set in prefab please
    public SpriteRenderer spriteRenderer;
    
    public enum State
    {
        Delay,
        Falling,
        Idle
    }

    State state = State.Delay;

    private void Awake()
    {
    }

    void Update()
    {
        switch (state)
        {
            case State.Delay:
                {
                    delayTimer -= Time.deltaTime;

                    if (delayTimer <= 0)
                    {
                        state = State.Falling;
                    }
                }
                break;
            case State.Falling:
                {
                    // subtract excess delay timer for subframe timing.
                    float time = Time.deltaTime - delayTimer;

                    // transition in. To-do: use animation or at least an AnimationCurve for this
                    scale = Mathf.Lerp(scale, targetScale, Mathf.Clamp(time, 0.05f, 0.1f) * 3);
                    transform.localScale = Vector3.one * scale;

                    // animate bouncing to destination
                    ySpeed += gravity * time;
                    Vector3 pos = transform.localPosition;
                    pos.y += ySpeed;
                    if (pos.y < y)
                    {
                        // hit the target tile, bounce up
                        ySpeed = -ySpeed * bounceFriction;
                        pos.y = y;
                        if (ySpeed < minSpeed)
                        {
                            // done fallin
                            state = State.Idle;
                        }
                    }
                    transform.localPosition = pos;

                    delayTimer = 0;
                }
                break;
            case State.Idle:
                {

                }
                break;
            default:
                Debug.LogError("Unsupported state in Piece!");
                break;
        }
    }

    public void Reset(int x, int y, PieceData type, float delay)
    {
        // was gonna use a coroutine for the delay but decided to use state instead
        // lets me do subframe counting
        delayTimer = delay;
        state = State.Delay;
        SetType(type);
        scale = 0;
        transform.localScale = Vector3.one * scale;
        ySpeed = 0;
        SetPosition(x, y);
        // set actual position to above so they bounce in from the top
        transform.localPosition = new Vector3(x, y + 10, 0);
    }

    void SetState(State state)
    {
        this.state = state;
    }

    void SetType(PieceData type)
    {
        // update the graphics and all that.
        this.type = type;
        spriteRenderer.sprite = type.sprite;
        spriteRenderer.color = type.color;
    }

    public void SetPosition(int x, int y)
    {
        this.x = x; // this will snap the position in x if different
        this.y = y; // we don't support moving tiles upwards at the moment but don't assert here because it can happen in Reset
        // kick off a falling state
        state = State.Delay;
    }

    // wrapping this up in case I change how pieces record their type
    public bool IsSameType(Piece other)
    {
        return type == other.type;
    }

    public void Select()
    {
        spriteRenderer.color = type.highlightColor;
    }
    public void Deselect()
    {
        spriteRenderer.color = type.color;
    }

}
