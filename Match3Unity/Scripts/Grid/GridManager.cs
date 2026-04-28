using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Match3.Anim;
using Match3.Data;
using Match3.Level;
using Match3.Matching;
using Match3.Pooling;
using UnityEngine;

namespace Match3.Grid
{
    public class GridManager : MonoBehaviour
    {
        public System.Action<TileType,int> OnTilesCleared;
        [Header("Refs")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private MatchFinder matchFinder;
        [SerializeField] private TilePool tilePool;
        [SerializeField] private AnimationController animationController;
        [SerializeField] private RectTransform gridRoot;

        [Header("Visuals")]
        [SerializeField] private float cellSize = 96f;
        [SerializeField] private Sprite[] normalTileSprites;
        [SerializeField] private Sprite rocketH;
        [SerializeField] private Sprite rocketV;
        [SerializeField] private Sprite bombSprite;
        [SerializeField] private Sprite colorBombSprite;

        public Tile[,] Tiles { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private System.Random _rng;

        public void BuildGrid()
        {
            Width = levelManager.ActiveLevel.width;
            Height = levelManager.ActiveLevel.height;
            _rng = new System.Random(levelManager.ActiveLevel.seed == 0 ? Random.Range(1, int.MaxValue) : levelManager.ActiveLevel.seed);

            Tiles = new Tile[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    SpawnTileAt(x, y, GetRandomType());
                }
            }

            StartCoroutine(ClearInitialAutoMatches());
        }

        public bool AreAdjacent(Tile a, Tile b)
        {
            return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y) == 1;
        }

        public IEnumerator TrySwap(Tile a, Tile b, System.Action<bool, int> onResolved)
        {
            SwapInternal(a, b);
            yield return animationController.Swap(a, b);

            var matches = matchFinder.FindAllMatches(Tiles, Width, Height);
            if (matches.Count == 0)
            {
                SwapInternal(a, b);
                yield return animationController.RevertSwap(a, b);
                onResolved?.Invoke(false, 0);
                yield break;
            }

            int combo = 0;
            int scoreGain = 0;
            while (matches.Count > 0)
            {
                combo++;
                scoreGain += 50 * matches.Sum(m => m.Count) * combo;
                yield return ResolveMatches(matches);
                yield return CollapseAndRefill();
                matches = matchFinder.FindAllMatches(Tiles, Width, Height);
            }

            onResolved?.Invoke(true, scoreGain);
        }

        public Vector3 GridToLocal(int x, int y)
        {
            float ox = -(Width - 1) * cellSize * 0.5f;
            float oy = -(Height - 1) * cellSize * 0.5f;
            return new Vector3(ox + x * cellSize, oy + y * cellSize, 0f);
        }

        private TileType GetRandomType()
        {
            return (TileType)_rng.Next(0, 6);
        }

        private void SpawnTileAt(int x, int y, TileType type)
        {
            var tile = tilePool.Get();
            tile.transform.SetParent(gridRoot, false);
            tile.transform.localScale = Vector3.one;
            tile.Initialize(x, y, type, normalTileSprites[(int)type]);
            tile.transform.localPosition = GridToLocal(x, y);
            Tiles[x, y] = tile;
        }

        private IEnumerator ClearInitialAutoMatches()
        {
            var matches = matchFinder.FindAllMatches(Tiles, Width, Height);
            while (matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    foreach (var tile in match.Tiles)
                    {
                        tilePool.Return(tile);
                        Tiles[tile.X, tile.Y] = null;
                    }
                }

                yield return CollapseAndRefill();
                matches = matchFinder.FindAllMatches(Tiles, Width, Height);
            }
        }

        private IEnumerator ResolveMatches(List<MatchData> matches)
        {
            var toClear = new HashSet<Tile>();
            foreach (var match in matches)
            {
                CreateSpecialFromMatch(match, toClear);
                foreach (var tile in match.Tiles)
                    toClear.Add(tile);
            }

            ExpandSpecialChain(toClear);

            foreach (var grp in toClear.GroupBy(t => t.TileType))
                OnTilesCleared?.Invoke(grp.Key, grp.Count());

            foreach (var tile in toClear)
            {
                yield return animationController.Pop(tile);
                Tiles[tile.X, tile.Y] = null;
                tilePool.Return(tile);
            }
        }

        private void CreateSpecialFromMatch(MatchData match, HashSet<Tile> toClear)
        {
            if (match.Count < 4) return;
            var pivot = match.Tiles[match.Tiles.Count / 2];
            toClear.Remove(pivot);

            bool multiAxis = match.Tiles.Select(t => t.X).Distinct().Count() > 1 &&
                             match.Tiles.Select(t => t.Y).Distinct().Count() > 1;

            if (match.Count >= 5 && !multiAxis)
            {
                pivot.SetSpecial(SpecialTileType.ColorBomb, colorBombSprite);
                return;
            }

            if (multiAxis)
            {
                pivot.SetSpecial(SpecialTileType.Bomb, bombSprite);
                return;
            }

            pivot.SetSpecial(match.IsHorizontal ? SpecialTileType.RocketHorizontal : SpecialTileType.RocketVertical,
                match.IsHorizontal ? rocketH : rocketV);
        }

        private void ExpandSpecialChain(HashSet<Tile> clear)
        {
            var queue = new Queue<Tile>(clear);
            while (queue.Count > 0)
            {
                var t = queue.Dequeue();
                if (t == null) continue;

                void Add(Tile c)
                {
                    if (c != null && clear.Add(c)) queue.Enqueue(c);
                }

                switch (t.SpecialType)
                {
                    case SpecialTileType.RocketHorizontal:
                        for (int x = 0; x < Width; x++) Add(Tiles[x, t.Y]);
                        break;
                    case SpecialTileType.RocketVertical:
                        for (int y = 0; y < Height; y++) Add(Tiles[t.X, y]);
                        break;
                    case SpecialTileType.Bomb:
                        for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                            Add(GetTileAt(t.X + dx, t.Y + dy));
                        break;
                    case SpecialTileType.ColorBomb:
                        var targetType = TileType.Red;
                        foreach (var other in clear)
                        {
                            if (other != t)
                            {
                                targetType = other.TileType;
                                break;
                            }
                        }
                        for (int x = 0; x < Width; x++)
                        for (int y = 0; y < Height; y++)
                            if (Tiles[x, y] != null && Tiles[x, y].TileType == targetType) Add(Tiles[x, y]);
                        break;
                }
            }
        }

        private IEnumerator CollapseAndRefill()
        {
            for (int x = 0; x < Width; x++)
            {
                int writeY = 0;
                for (int y = 0; y < Height; y++)
                {
                    if (Tiles[x, y] == null) continue;

                    if (writeY != y)
                    {
                        var t = Tiles[x, y];
                        Tiles[x, writeY] = t;
                        Tiles[x, y] = null;
                        t.SetCoordinates(x, writeY);
                        yield return animationController.Fall(t, GridToLocal(x, writeY), y - writeY);
                    }
                    writeY++;
                }

                for (int y = writeY; y < Height; y++)
                {
                    SpawnTileAt(x, y, GetRandomType());
                    var t = Tiles[x, y];
                    t.transform.localPosition = GridToLocal(x, Height + (y - writeY + 1));
                    yield return animationController.Fall(t, GridToLocal(x, y), Height);
                }
            }
        }

        private void SwapInternal(Tile a, Tile b)
        {
            int ax = a.X; int ay = a.Y;
            int bx = b.X; int by = b.Y;

            Tiles[ax, ay] = b;
            Tiles[bx, by] = a;
            a.SetCoordinates(bx, by);
            b.SetCoordinates(ax, ay);
        }

        public Tile GetTileAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
            return Tiles[x, y];
        }
    }
}
