using System.Collections.Generic;
using Match3.Grid;

namespace Match3.Matching
{
    public class MatchData
    {
        public readonly List<Tile> Tiles = new();
        public bool IsHorizontal;

        public int Count => Tiles.Count;
    }
}
