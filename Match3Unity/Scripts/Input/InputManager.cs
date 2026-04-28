using System.Collections;
using Match3.Core;
using Match3.Grid;
using UnityEngine;

namespace Match3.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private float swipeThreshold = 40f;
        [SerializeField] private float hintDelay = 5f;

        private Tile _selected;
        private Vector2 _pressPos;
        private float _lastInputTime;

        private void Update()
        {
            if (!gameManager.CanInput) return;

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _pressPos = UnityEngine.Input.mousePosition;
                _lastInputTime = Time.time;
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                HandleRelease(UnityEngine.Input.mousePosition);
                _lastInputTime = Time.time;
            }

            if (Time.time - _lastInputTime > hintDelay)
            {
                gameManager.ShowHint();
                _lastInputTime = Time.time + 999f;
            }
        }

        private void HandleRelease(Vector2 releasePos)
        {
            var originTile = RaycastTile(_pressPos);
            if (originTile == null) return;

            var delta = releasePos - _pressPos;
            if (delta.magnitude < swipeThreshold)
            {
                SelectTile(originTile);
                return;
            }

            Vector2Int dir = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? new Vector2Int(delta.x > 0 ? 1 : -1, 0)
                : new Vector2Int(0, delta.y > 0 ? 1 : -1);

            var target = gridManager.GetTileAt(originTile.X + dir.x, originTile.Y + dir.y);
            if (target != null)
                StartCoroutine(gameManager.ProcessSwap(originTile, target));
        }

        private void SelectTile(Tile tile)
        {
            if (_selected == null)
            {
                _selected = tile;
                _selected.SetHighlight(true);
                return;
            }

            if (_selected == tile)
            {
                _selected.SetHighlight(false);
                _selected = null;
                return;
            }

            if (gridManager.AreAdjacent(_selected, tile))
            {
                _selected.SetHighlight(false);
                StartCoroutine(gameManager.ProcessSwap(_selected, tile));
                _selected = null;
                return;
            }

            _selected.SetHighlight(false);
            _selected = tile;
            _selected.SetHighlight(true);
        }

        private Tile RaycastTile(Vector2 screenPos)
        {
            float best = float.MaxValue;
            Tile bestTile = null;
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    var tile = gridManager.GetTileAt(x, y);
                    var p = RectTransformUtility.WorldToScreenPoint(uiCamera, tile.transform.position);
                    float d = Vector2.SqrMagnitude((Vector2)p - screenPos);
                    if (d < best)
                    {
                        best = d;
                        bestTile = tile;
                    }
                }
            }

            return best < 2500f ? bestTile : null;
        }
    }
}
