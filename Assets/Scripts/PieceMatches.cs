using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores a list of matched pieces along with their locations on the board.
public class PieceMatches : IEnumerable
{
    PreallocatedList<MatchData> matchDataPool = new PreallocatedList<MatchData>(50);

    List<MatchData> matches = new List<MatchData>();
    
    public bool Contains(Piece piece)
    {
        foreach (MatchData md in matches)
        {
            if (md.piece == piece) return true;
        }
        return false;
    }

    public void Add(Piece piece, int x, int y)
    {
        MatchData md = matchDataPool.Get();
        md.piece = piece;
        md.x = x;
        md.y = y;
        matches.Add(md);
    }

    public void Clear()
    {
        foreach(MatchData match in matches)
        {
            match.piece.Deselect();
        }
        matches.Clear();
        matchDataPool.Reset();
    }

    public int Count {  get { return matches.Count; } }

    public MatchData GetEnd()
    {
        if (matches.Count == 0) return null;
        return matches[matches.Count - 1];
    }

    public void RemoveAfter(Piece piece)
    {
        int index = matches.FindIndex((MatchData match) => { return match.piece == piece; });
        index++;
        if (index == matches.Count) return;
        matches.RemoveRange(index, matches.Count - index);
    }

    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable)matches).GetEnumerator();
    }
}
