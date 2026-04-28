using System.Collections;
using System.Collections.Generic;
using Match3.Data;
using Match3.Grid;
using Match3.Level;
using Match3.UI;
using UnityEngine;

namespace Match3.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private SpecialTileResolver specialTileResolver;

        private int _movesLeft;
        private int _score;
        private readonly Dictionary<TileType, int> _clearedCounts = new();

        public bool CanInput { get; private set; }

        private void OnEnable()
        {
            gridManager.OnTilesCleared += HandleTilesCleared;
        }

        private void OnDisable()
        {
            gridManager.OnTilesCleared -= HandleTilesCleared;
        }

        private void Start()
        {
            StartLevel();
        }

        public void StartLevel()
        {
            _score = 0;
            _movesLeft = levelManager.ActiveLevel.moveLimit;
            _clearedCounts.Clear();
            gridManager.BuildGrid();
            CanInput = true;
            uiManager.Refresh(_score, _movesLeft);
            uiManager.HideComplete();
        }

        public IEnumerator ProcessSwap(Tile a, Tile b)
        {
            if (!CanInput) yield break;
            CanInput = false;

            int gained = 0;

            if (a.SpecialType != SpecialTileType.None || b.SpecialType != SpecialTileType.None)
            {
                yield return specialTileResolver.Resolve(a, b);
                gained = 500;
                yield return gridManager.TrySwap(a, b, (success, score) => gained += score);
            }
            else
            {
                bool valid = false;
                yield return gridManager.TrySwap(a, b, (success, score) =>
                {
                    valid = success;
                    gained = score;
                });

                if (!valid)
                {
                    CanInput = true;
                    yield break;
                }
            }

            _movesLeft--;
            _score += gained;
            uiManager.Refresh(_score, _movesLeft);

            if (IsComplete())
            {
                uiManager.ShowComplete(true);
                CanInput = false;
            }
            else if (_movesLeft <= 0)
            {
                uiManager.ShowComplete(false);
                CanInput = false;
            }
            else
            {
                CanInput = true;
            }
        }

        private bool IsComplete()
        {
            foreach (var goal in levelManager.ActiveLevel.goals)
            {
                if (goal.goalType == GoalType.Score && _score < goal.target)
                    return false;

                if (goal.goalType == GoalType.ClearColor)
                {
                    _clearedCounts.TryGetValue(goal.tileType, out int cleared);
                    if (cleared < goal.target)
                        return false;
                }
            }
            return true;
        }

        public void Restart()
        {
            levelManager.RestartLevel();
            StartLevel();
        }

        public void ShowHint()
        {
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    var a = gridManager.GetTileAt(x, y);
                    var right = gridManager.GetTileAt(x + 1, y);
                    if (a != null && right != null)
                    {
                        a.SetHighlight(true);
                        right.SetHighlight(true);
                        StartCoroutine(ClearHint(a, right));
                        return;
                    }
                }
            }
        }

        private IEnumerator ClearHint(Tile a, Tile b)
        {
            yield return new WaitForSeconds(0.8f);
            if (a != null) a.SetHighlight(false);
            if (b != null) b.SetHighlight(false);
        }

        private void HandleTilesCleared(TileType tileType, int amount)
        {
            if (!_clearedCounts.ContainsKey(tileType))
                _clearedCounts[tileType] = 0;
            _clearedCounts[tileType] += amount;
        }
    }
}
