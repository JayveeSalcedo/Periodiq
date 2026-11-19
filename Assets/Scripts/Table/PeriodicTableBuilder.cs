using System.Collections.Generic;
using Periodiq.Data;
using UnityEngine;

namespace Periodiq.Table
{
    /// <summary>
    /// Spawns all 118 tiles in the standard layout, standing upright like a wall chart.
    /// The table's pivot is bottom-centre so it sits nicely on a detected plane.
    /// </summary>
    public class PeriodicTableBuilder : MonoBehaviour
    {
        [SerializeField] ElementTile tilePrefab;
        [Tooltip("Tile size + gap, in metres. 0.03 makes the whole table ~55 cm wide.")]
        [SerializeField] float cellSize = 0.03f;
        [Tooltip("Extra gap above the lanthanide/actinide rows.")]
        [SerializeField] float fBlockGap = 0.5f;
        [Tooltip("Lean the table back slightly so it's easier to read from above.")]
        [SerializeField] float tiltDegrees = 15f;
        [SerializeField] bool buildOnStart = true;

        readonly Dictionary<int, ElementTile> _tiles = new();
        public IReadOnlyDictionary<int, ElementTile> Tiles => _tiles;

        void Start()
        {
            if (buildOnStart) Build();
        }

        public void Build()
        {
            Clear();
            const int columns = 18;
            // Data puts the f-block on rows 9-10 (row 8 empty). We pull it up so the
            // gap is just fBlockGap, giving 9 visual rows.
            const int rows = 9;
            float width = columns * cellSize;
            float height = (rows + fBlockGap) * cellSize;

            var root = new GameObject("Tiles").transform;
            root.SetParent(transform, false);
            root.localRotation = Quaternion.Euler(-tiltDegrees, 0f, 0f);

            foreach (var e in ElementDatabase.All)
            {
                float row = e.ypos >= 9 ? e.ypos - 1 + fBlockGap : e.ypos;
                var local = new Vector3(
                    (e.xpos - 0.5f) * cellSize - width / 2f,
                    height - (row - 0.5f) * cellSize,
                    0f);

                var tile = Instantiate(tilePrefab, root);
                tile.transform.localPosition = local;
                float s = cellSize * 0.92f;
                tile.transform.localScale = new Vector3(s, s, s * 0.35f); // thin slab
                tile.Setup(e);
                _tiles[e.number] = tile;
            }
        }

        public void Clear()
        {
            _tiles.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }
    }
}
