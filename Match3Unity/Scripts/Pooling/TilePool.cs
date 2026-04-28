using System.Collections.Generic;
using Match3.Grid;
using UnityEngine;

namespace Match3.Pooling
{
    public class TilePool : MonoBehaviour
    {
        [SerializeField] private Tile tilePrefab;
        [SerializeField] private int initialSize = 96;

        private readonly Queue<Tile> _pool = new();

        private void Awake()
        {
            WarmPool(initialSize);
        }

        private void WarmPool(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var tile = Instantiate(tilePrefab, transform);
                tile.gameObject.SetActive(false);
                _pool.Enqueue(tile);
            }
        }

        public Tile Get()
        {
            if (_pool.Count == 0)
                WarmPool(16);

            return _pool.Dequeue();
        }

        public void Return(Tile tile)
        {
            tile.gameObject.SetActive(false);
            tile.transform.SetParent(transform);
            _pool.Enqueue(tile);
        }
    }
}
