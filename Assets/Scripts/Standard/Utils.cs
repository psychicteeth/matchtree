using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class Utils
{

    public static GameObject TryGetChild(this GameObject gameObject, string name)
    {
        Transform t = gameObject.transform.Find(name);
        if (t != null) return t.gameObject;
        return null;
    }

    public static float TicksToSeconds(long ticks)
    {
        return (float)((double)ticks / 10000000.0);
    }

    public static long GetTicks() { return System.DateTime.Now.Ticks; }

    public static int Clamp(int value, int min, int max)
    {
        if (value < min) value = min;
        if (value > max) value = max;
        return value;
    }
}

public static class Extensions
{
    public static Vector3 Flat(this Vector3 original)
    {
        original.y = 0;
        return original;
    }
    public static T GetRandom<T>(this List<T> source)
    {
        return source[UnityEngine.Random.Range(0, source.Count)];
    }
    public static T GetRandom<T>(this List<T> source, int min, int max)
    {
        Debug.Assert(min >= 0 && min <= source.Count, "min: " + min + " limit: " + source.Count);
        Debug.Assert(max >= 0 && max <= source.Count, "max: " + max + " limit: " + source.Count);
        Debug.Assert(min <= max);
        // probably don't want to crash in this situ
        min = Utils.Clamp(min, 0, source.Count);
        max = Utils.Clamp(max, 0, source.Count);
        return source[UnityEngine.Random.Range(min, max)];
    }
    public static int GetRandomIndex<T>(this List<T> source)
    {
        return UnityEngine.Random.Range(0, source.Count);
    }
}

// C# does not allow static extension methods so we can't make UnityEngine.Random.FromEnum :(
public static class RandomExt
{
    public static T FromEnum<T>()
    {
        var v = Enum.GetValues(typeof(T));
        return (T)v.GetValue(UnityEngine.Random.Range(0, v.Length));
    }
}

public static class RectTransformExtensions
{

    public static bool Overlaps(this RectTransform a, RectTransform b)
    {
        return a.WorldRect().Overlaps(b.WorldRect());
    }
    public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse)
    {
        return a.WorldRect().Overlaps(b.WorldRect(), allowInverse);
    }

    public static Rect WorldRect(this RectTransform rectTransform)
    {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
        float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

        Vector3 position = rectTransform.position;
        return new Rect(position.x - rectTransformWidth / 2f, position.y - rectTransformHeight / 2f, rectTransformWidth, rectTransformHeight);
    }
}