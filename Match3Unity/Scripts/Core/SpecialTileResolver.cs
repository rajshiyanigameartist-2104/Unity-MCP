using System.Collections;
using Match3.Data;
using Match3.Grid;
using UnityEngine;

namespace Match3.Core
{
    public class SpecialTileResolver : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;

        public IEnumerator Resolve(Tile a, Tile b)
        {
            if (a.SpecialType == SpecialTileType.ColorBomb || b.SpecialType == SpecialTileType.ColorBomb)
            {
                var color = a.SpecialType == SpecialTileType.ColorBomb ? b.TileType : a.TileType;
                for (int x = 0; x < gridManager.Width; x++)
                {
                    for (int y = 0; y < gridManager.Height; y++)
                    {
                        var t = gridManager.GetTileAt(x, y);
                        if (t != null && t.TileType == color)
                            t.SetHighlight(true);
                    }
                }
                yield return new WaitForSeconds(0.25f);
            }

            // Full special effect cleanup is driven by normal match cascade in this baseline.
            yield return null;
        }
    }
}
