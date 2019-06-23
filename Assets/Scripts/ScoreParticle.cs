using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreParticle : MonoBehaviour
{
    // set in prefab
    public TMPro.TMP_Text text;
    public Animator animator;

    public ScoreParticles parent;
    static int appearHash = Animator.StringToHash("AppearDisappear");

    private void Awake()
    {
        // apparently this can't be set in the inspector
        text.GetComponent<Renderer>().sortingLayerName = "Score Particles";   
    }

    public void Finished()
    {
        parent.Done(gameObject);
    }

    internal void Spawn(Vector3 position, int score)
    {
        // keep them in front of the other stuff
        Vector3 pos = position;
        pos.z = transform.position.z;
        transform.position = pos;
        text.text = "+" + score;
        animator.Play(appearHash);
    }
}
