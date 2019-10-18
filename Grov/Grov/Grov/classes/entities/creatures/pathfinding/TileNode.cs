using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grov
{
    class TileNode
    {
        #region properties
        // ************* Properties ************* //

        public Tile Tile { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public TileNode PathNeighbor { get; set; }
        public int F { get => G + H; }
        public int G { get; set; }
        public int H { get; set; }
        public bool IsPassable { get => Tile.IsPassable; }
        public bool Checked { get; set; }
        #endregion

        #region constructor
        // ************* Constructor ************* //

        public TileNode(int x, int y, Tile tile)
        {
            Tile = tile;
            X = x;
            Y = y;
            Reset();
            //TODO get actual tile
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        public void Reset()
        {
            Checked = false;
            PathNeighbor = null;
        }

        public override string ToString()
        {
            return String.Format("Tile at ({0}, {1}): Neighbor at ({2})", X, Y, (PathNeighbor != null ? PathNeighbor.X + ", " + PathNeighbor.Y : "null"));
        }
        #endregion
    }
}
