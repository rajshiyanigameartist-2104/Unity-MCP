using System.Collections.Generic;
using System.Linq;
using Match3.Grid;
using UnityEngine;

namespace Match3.Matching
{
    public class MatchFinder : MonoBehaviour
    {
        public List<MatchData> FindAllMatches(Tile[,] grid, int width, int height)
        {
            var results = new List<MatchData>();
            FindHorizontal(grid, width, height, results);
            FindVertical(grid, width, height, results);
            return MergeOverlaps(results);
        }

        private static void FindHorizontal(Tile[,] grid, int width, int height, List<MatchData> results)
        {
            for (int y = 0; y < height; y++)
            {
                int x = 0;
                while (x < width - 2)
                {
                    var first = grid[x, y];
                    if (first == null)
                    {
                        x++;
                        continue;
                    }

                    int run = 1;
                    while (x + run < width && grid[x + run, y] != null &&
                           grid[x + run, y].TileType == first.TileType)
                    {
                        run++;
                    }

                    if (run >= 3)
                    {
                        var match = new MatchData { IsHorizontal = true };
                        for (int i = 0; i < run; i++)
                            match.Tiles.Add(grid[x + i, y]);
                        results.Add(match);
                    }

                    x += Mathf.Max(run, 1);
                }
            }
        }

        private static void FindVertical(Tile[,] grid, int width, int height, List<MatchData> results)
        {
            for (int x = 0; x < width; x++)
            {
                int y = 0;
                while (y < height - 2)
                {
                    var first = grid[x, y];
                    if (first == null)
                    {
                        y++;
                        continue;
                    }

                    int run = 1;
                    while (y + run < height && grid[x, y + run] != null &&
                           grid[x, y + run].TileType == first.TileType)
                    {
                        run++;
                    }

                    if (run >= 3)
                    {
                        var match = new MatchData { IsHorizontal = false };
                        for (int i = 0; i < run; i++)
                            match.Tiles.Add(grid[x, y + i]);
                        results.Add(match);
                    }

                    y += Mathf.Max(run, 1);
                }
            }
        }

        private static List<MatchData> MergeOverlaps(List<MatchData> matches)
        {
            var merged = new List<MatchData>();
            foreach (var match in matches)
            {
                var existing = merged.FirstOrDefault(m => m.Tiles.Intersect(match.Tiles).Any());
                if (existing == null)
                {
                    merged.Add(match);
                    continue;
                }

                foreach (var tile in match.Tiles)
                {
                    if (!existing.Tiles.Contains(tile))
                        existing.Tiles.Add(tile);
                }
            }

            return merged;
        }
    }
}
