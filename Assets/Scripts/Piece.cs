using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour
{
    public int x { get; private set; }
    public int y { get; private set; }
    float ySpeed = 0;
    float delayTimer = 0;
    PieceData type;
    // Each piece also has a random leaf on it. I don't know why yet!
    public int leafIndex { get; private set; }
    // each piece has a hp score, which translates to lifetime in turns, before it turns to garbage and can't be used in matches
    public int hp { get; private set; }

    static float gravity = -0.9f;
    static float bounceFrictionMin = 0.2f;
    static float bounceFrictionMax = 0.3f;
    const float minSpeed = 0.1f;

    // Set in prefab please
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public SpriteRenderer leafSpriteRenderer;
    public LeafData leafDataAsset;

    // state hashes
    static int hiddenStateHash = Animator.StringToHash("Hidden");
    static int appearHash = Animator.StringToHash("Appear");
    static int selectedHash = Animator.StringToHash("Selected");
    static int deselectedHash = Animator.StringToHash("Deselected");

    static Board board = null; // for playing sounds

    public enum State
    {
        Delay,
        Falling,
        Idle
    }

    State state = State.Delay;

    private void Awake()
    {
        if (board == null) board = FindObjectOfType<Board>();
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
                        animator.SetTrigger(appearHash);
                    }
                }
                break;
            case State.Falling:
                {
                    // subtract excess delay timer for subframe timing.
                    float time = Time.deltaTime - delayTimer;

                    // animate bouncing to destination
                    ySpeed += gravity * time;
                    Vector3 pos = transform.localPosition;
                    pos.x = Mathf.Lerp(pos.x, x, time * 12);
                    pos.y += ySpeed;
                    if (pos.y < y)
                    {
                        // hit the target tile, bounce up
                        ySpeed = -ySpeed * Random.Range(bounceFrictionMin, bounceFrictionMax);
                        pos.y = y;

                        board.PlayDripSound();

                        if (ySpeed < minSpeed)
                        {
                            // done fallin
                            state = State.Idle;
                            ySpeed = 0;
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

    public void Reset(int x, int y, PieceData type, int leafIndex, float delay)
    {
        // was gonna use a coroutine for the delay but decided to use state instead
        // lets me do subframe counting
        delayTimer = delay;
        state = State.Delay;
        SetType(type);
        ySpeed = 0;
        SetPosition(x, y);
        // set actual position to above so they bounce in from the top
        transform.localPosition = new Vector3(x, y + 10, 0);
        animator.Play(hiddenStateHash);
        // choose a new leaf
        this.leafIndex = leafIndex;
        leafSpriteRenderer.sprite = leafDataAsset.leaves[leafIndex];
        // temp, just randomixe the hp
        hp = Random.Range(5, 10);
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
        this.x = x; 
        this.y = y; // we don't support moving tiles upwards but don't assert here because it can happen in Reset
    }

    public void Fall(int newY, float delay)
    {
        SetPosition(x, newY);
        state = State.Delay;
        ySpeed = 0;
        delayTimer = delay;
    }

    public bool Matches(Piece other)
    {        
        if (hp <= 0) return false;
        if (other.hp <= 0) return false;
        return IsSameType(other);
    }

    public bool IsSameType(Piece other)
    {
        return type == other.type;
    }

    public void Select()
    {
        spriteRenderer.color = type.highlightColor;
        animator.SetTrigger(selectedHash);
        animator.ResetTrigger(deselectedHash);
    }
    public void Deselect()
    {
        spriteRenderer.color = type.color;
        animator.SetTrigger(deselectedHash);
        animator.ResetTrigger(selectedHash);
    }

    public void OnTurn()
    {
        hp--;
        if (hp <= 0)
        {            
            spriteRenderer.color = Color.black;
        }
    }
}
