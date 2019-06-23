using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Leaves", menuName = "Match Tree/Leaf data", order = 1)]
public class LeafData : ScriptableObject
{
    public List<Sprite> leaves;
}

[System.Serializable]
public class LeafInfo
{
    public Sprite sprite;
    // an arbitrary value representing how relatively abundant this leaf type is. Values are added up and used as a weight in a random roll when spawning
    public float abundance;
}