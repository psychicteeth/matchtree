using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Plays notes for you. Can play a nice little success chord.
public class SelectionToneAudio : MonoBehaviour
{
    // set in editor
    public AudioClip selectPieceTone;
    
    static readonly float[] pitches = new float[]
    {
        1.0f,
        9.0f/8.0f,
        5.0f/4.0f,
        4.0f/3.0f,
        3.0f/2.0f,
        5.0f/3.0f,
        15.0f/8.0f
    };
    static readonly int numNotes = pitches.Length * 2 + 1;
    public float notePitch = 0.5f;
    public float noteVolume = 0.8f;

    // these are a bit naughtily named but I couldn't resist. 
    // Could do an enum but then people can't pass in whatever int they like to PlayTone
    const int c0 = 0;
    const int d0 = 1;
    const int e0 = 2;
    const int f0 = 3;
    const int g0 = 4;
    const int a0 = 5;
    const int b0 = 6;
    const int c1 = 7;
    const int d1 = 8;
    const int e1 = 9;
    const int f1 = 10;
    const int g1 = 11;
    const int a1 = 12;
    const int b1 = 13;
    const int c2 = 14;
    public const float chordNoteDelay = 0.06f;

    public AudioSource[] toneSources;

    WaitForSeconds chordWaiter = new WaitForSeconds(chordNoteDelay);

    void Start()
    {
        // register a bunch of audio sources at different pitches becuase you can't play one-shots at different pitches from the same audio source
        toneSources = new AudioSource[numNotes];
        for (int i = 0; i < numNotes; i++)
        {
            toneSources[i] = gameObject.AddComponent<AudioSource>();
            toneSources[i].spatialBlend = 0;
            int octave = (i / pitches.Length) + 1;
            if (octave == 3) octave = 4; // there's prob some maths here but I don't have time
            int noteIndex = i % pitches.Length;
            float pitch = pitches[noteIndex] * octave;
            pitch *= notePitch;
            toneSources[i].pitch = pitch;
        }
    }

    public void PlayTone(int note)
    {
        note = Mathf.Max(0, Mathf.Min(note, toneSources.Length - 1));
        toneSources[note].PlayOneShot(selectPieceTone, noteVolume);
    }

    public void PlaySuccessChord()
    {
        StartCoroutine(PlaySuccessChordCoroutine(false));
    }

    IEnumerator PlaySuccessChordCoroutine(bool greatSuccess)
    {
        PlayTone(c0);
        yield return chordWaiter;
        PlayTone(e0);
        yield return chordWaiter;
        PlayTone(g0);
        yield return chordWaiter;
        PlayTone(c1);
        if (!greatSuccess) yield break;
        yield return chordWaiter;
        PlayTone(e1);
        yield return chordWaiter;
        PlayTone(g1);
        yield return chordWaiter;
        PlayTone(c2);
    }

    public void PlayGreatSuccessChord()
    {
        StartCoroutine(PlaySuccessChordCoroutine(true));
    }
}
