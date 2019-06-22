using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores a list of matched pieces along with their locations on the board.
public class PieceMatches
{
    PreallocatedList<MatchData> matchDataPool = new PreallocatedList<MatchData>(50);

    public class MatchData
    {
        public int x;
        public int y;
        public Piece piece;
    }

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
        matches.Clear();
        matchDataPool.Reset();
    }

    public int Count {  get { return matches.Count; } }
}
