using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//Brought to you in part by:
//Duncan Mott
//Rachel Wong
//Jake Zaia

namespace Grov
{
    enum TileType
    {
        Death = -1,
        Floor = 0,
        Wall = 1,
        Entrance = 2,
        Water = 3,
        Rock = 4,
        Stump = 5,
        Bridge = 6,
        Flower = 7,
        BossDoor = 8
    }
	class Tile
	{
        #region fields
        // ************* Fields ************* //

        private Point location;
		private TileType type;
		private bool isPassable;
		private bool blocksProjectiles;
        private bool blocksLineOfSight;

        #endregion

        #region properties
        // ************* Properties ************* //

        public TileType Type { get => type; set => type = value; }
		public bool IsPassable { get => isPassable; set => isPassable = value; }
		public bool BlocksProjectiles { get => blocksProjectiles; }
        public bool BlocksLineOfSight { get => blocksLineOfSight; }
        public Point Location { get => location; set => location = value; }
        #endregion

        #region constructor
        // ************* Constructor ************* //

        /// <summary>
        /// Create a new tile
        /// </summary>
        public Tile(TileType type, int x, int y)
        {
            this.type = type;
            location = new Point(x, y);

            //Depending on what tile type it is, change its collision detection properties
            switch ((int)type)
            {
                //Floor
                case 0:
                    isPassable = true;
                    blocksProjectiles = false;
                    blocksLineOfSight = false;
                    break;
                //Border wall
                case 1:
                    isPassable = false;
                    blocksProjectiles = true;
                    blocksLineOfSight = true;
                    break;
                //Door
                case 2:
					isPassable = false;
					blocksProjectiles = true;
                    blocksLineOfSight = false;
                    break;
                //Water
                case 3:
                    isPassable = false;
                    blocksProjectiles = false;
                    blocksLineOfSight = false;
                    break;
                //Rock
                case 4:
                    isPassable = false;
                    blocksProjectiles = true;
                    blocksLineOfSight = false;
                    break;
                //Stump
                case 5:
                    isPassable = false;
                    blocksProjectiles = false;
                    blocksLineOfSight = false;
                    break;
                //Bridge
                case 6:
                    isPassable = true;
                    blocksProjectiles = false;
                    blocksLineOfSight = false;
                    break;
                //Flowers
                case 7:
                    isPassable = true;
                    blocksProjectiles = false;
                    blocksLineOfSight = false;
                    break;

                //Death
                case -1:
                    isPassable = false;
                    blocksProjectiles = true;
                    blocksLineOfSight = false;
                    break;
                //Boss door
                case 8:
                    isPassable = false;
                    blocksProjectiles = true;
                    blocksLineOfSight = false;
                    break;
            }
        }
        #endregion

        #region methods
        #endregion
    }
}
