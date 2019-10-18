using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// Authors: Rachel Wong, Duncan Mott, Jake Zaia

namespace Grov
{
	enum EntranceState
	{
		Open,
		Closed
	}

    class Entrance
    {
        #region fields
        // ************* Fields ************* //

        private EntranceState state;
        private Room nextRoom;
        private List<Tile> tiles;
        private Point location;
        #endregion

        #region properties
        // ************* Properties ************* //

        public EntranceState State { get => state; set => state = value; }
		public Room NextRoom { get => nextRoom; set => nextRoom = value; }
        public Point Location { get => location; }
        #endregion

        #region constructors
        // ************* Constructors ************* //

        public Entrance()
        {
            state = EntranceState.Closed;
            tiles = new List<Tile>();
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        /// <summary>
        /// Adds a tile to the list of tiles considered by this entrance
        /// </summary>
        /// <param name="tile">The tile to add</param>
        public void AddTile(Tile tile)
        {
            tiles.Add(tile);
        }

        /// <summary>
        /// Open this entrance and allow the player to move through it
        /// </summary>
        public void OpenDoor()
        {
            this.State = EntranceState.Open;
            foreach(Tile tile in tiles)
            {
                tile.IsPassable = true;
            }
        }

        /// <summary>
        /// Update the location of this Entrance such that it is not null
        /// </summary>
        /// <param name="point">The new location of the Entrance</param>
        public void UpdateLocation(Point point)
        {
            if(location != null)
                this.location = point;
        }
        #endregion
    }
}
