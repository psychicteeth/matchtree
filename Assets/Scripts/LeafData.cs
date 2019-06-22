using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Leaves", menuName = "Match Tree/Leaf data", order = 1)]
public class LeafData : ScriptableObject
{
    public List<Sprite> leaves;
}
