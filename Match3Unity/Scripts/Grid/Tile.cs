using Match3.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Match3.Grid
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private GameObject highlight;

        public int X { get; private set; }
        public int Y { get; private set; }
        public TileType TileType { get; private set; }
        public SpecialTileType SpecialType { get; private set; }

        public void Initialize(int x, int y, TileType tileType, Sprite sprite)
        {
            X = x;
            Y = y;
            TileType = tileType;
            SpecialType = SpecialTileType.None;
            icon.sprite = sprite;
            SetHighlight(false);
            gameObject.SetActive(true);
        }

        public void SetCoordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void SetSpecial(SpecialTileType specialTileType, Sprite sprite)
        {
            SpecialType = specialTileType;
            icon.sprite = sprite;
        }

        public void SetHighlight(bool value)
        {
            if (highlight != null)
                highlight.SetActive(value);
        }
    }
}
