using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

//debugging
using System.Diagnostics;

// Authors: Jake Zaia, Rachel Wong, Duncan Mott, Jack Hoffman

namespace Grov
{ 
    class FloorManager
    {

        #region fields
        // ************* Fields ************* //

		private Room currRoom;
		private int numRooms;
		private int floorNumber;
        private static FloorManager instance;
        private Room[,] floorRooms;

		#endregion

		#region properties
		// ************* Properties ************* //
		public static FloorManager Instance { get => instance; }
		public static int TileWidth { get => 1920 / 32; }
		public static int TileHeight { get => 1080 / 18; }
        public Room CurrRoom { get => currRoom; set => currRoom = value; }
        public int FloorNumber { get => floorNumber; set => floorNumber = value; }
        public Room this[int x, int y]
        {
            get => floorRooms[x,y];
        }

        #endregion

        #region constructor
        // ************* Constructor ************* //

        private FloorManager() { }

        public static void Initialize()
        {
            if (instance == null)
            {
                instance = new FloorManager();
            }
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        /// <summary>
        /// This code creates a new floor. Always call this method.
        /// </summary>
        public void GenerateFloor()
        {
            RoomNode[,] floor = new RoomNode[11, 11];
            //In a do while loop; if the floor we make is deemed unusable, we need to make a new one
            do
            {
                floor = new RoomNode[11, 11];
                this.floorRooms = new Room[11, 11];
                List<RoomNode> currentNodes = new List<RoomNode>();

                //Spawn room
                floor[5, 5] = new RoomNode(5, 5);

                //Top
                floor[5, 4] = new RoomNode(5, 4, DoorGen.top);
                currentNodes.Add(floor[5, 4]);

                //Left
                floor[4, 5] = new RoomNode(4, 5, DoorGen.left);
                currentNodes.Add(floor[4, 5]);

                //Bottom
                floor[5, 6] = new RoomNode(5, 6, DoorGen.bottom);
                currentNodes.Add(floor[5, 6]);

                //Right
                floor[6, 5] = new RoomNode(6, 5, DoorGen.right);
                currentNodes.Add(floor[6, 5]);

                //Calculations based on "instance" means as we move away from spawn things get more rare
                int instance = 1;
                //While there are rooms we just made that don't have doors yet
                while (currentNodes.Count > 0)
                {
                    List<RoomNode> newNodes = new List<RoomNode>();


                    //Randomly decide to add doors to each new room
                    for (int i = 0; i < currentNodes.Count; i++)
                    {
                        int chanceModifier = GameManager.RNG.Next(0, 100);

                        //Randomly decrease the odds of adding a door
                            //Odds decrease further as instance increases
                        if (chanceModifier > 80 + (instance * 7))
                        {
                            chanceModifier = 10;
                        }
                        else if (chanceModifier > 60 + (instance * 7))
                        {
                            chanceModifier = 15;
                        }
                        else if (chanceModifier > 20 + (instance * 8))
                        {
                            chanceModifier = 30;
                        }
                        else
                        {
                            chanceModifier = 50;
                        }

                        byte door = 0;

                        //Actually add the doors
                        for (int x = 0; x < 4; x++)
                        {
                            door <<= 1;
                            if (GameManager.RNG.Next(0, 100) < 65 - chanceModifier)
                                door |= 1;
                        }

                        currentNodes[i].Doors = (DoorGen)((byte)currentNodes[i].Doors | door);
                    }
                    // Loop through all current Nodes and adds rooms where there are open doors
                    for (int i = 0; i < currentNodes.Count; i++)
                    {

                        //If there is a top door
                        if (((byte)currentNodes[i].Doors & 8) == 8)
                        {
                            //Add a room on top of it
                            //If the room would be out of bounds, remove the door facing that way
                            if (currentNodes[i].Y - 1 < 0)
                            {
                                currentNodes[i].Doors = (DoorGen)((byte)currentNodes[i].Doors & (byte)7);
                            }
                            //If there is no room in that direction, make a new one
                            else if (floor[currentNodes[i].X, currentNodes[i].Y - 1] == null)
                            {
                                floor[currentNodes[i].X, currentNodes[i].Y - 1] = new RoomNode(currentNodes[i].X, currentNodes[i].Y - 1, DoorGen.top);
                                newNodes.Add(floor[currentNodes[i].X, currentNodes[i].Y - 1]);
                            }
                            //If there is a room, give it a door pointing at us
                            else
                            {
                                floor[currentNodes[i].X, currentNodes[i].Y - 1].Doors = (DoorGen)((byte)floor[currentNodes[i].X, currentNodes[i].Y - 1].Doors | (byte)2);
                            }
                        }

                        //If there is a left door
                        if (((byte)currentNodes[i].Doors & 4) == 4)
                        {
                            if (currentNodes[i].X - 1 < 0)
                            {
                                currentNodes[i].Doors = (DoorGen)((byte)currentNodes[i].Doors & (byte)11);
                            }
                            else if (floor[currentNodes[i].X - 1, currentNodes[i].Y] == null)
                            {
                                floor[currentNodes[i].X - 1, currentNodes[i].Y] = new RoomNode(currentNodes[i].X - 1, currentNodes[i].Y, DoorGen.left);
                                newNodes.Add(floor[currentNodes[i].X - 1, currentNodes[i].Y]);
                            }
                            else
                            {
                                floor[currentNodes[i].X - 1, currentNodes[i].Y].Doors = (DoorGen)((byte)floor[currentNodes[i].X - 1, currentNodes[i].Y].Doors | (byte)1);
                            }
                        }
                        //If there is a bottom door
                        if (((byte)currentNodes[i].Doors & 2) == 2)
                        {
                            if (currentNodes[i].Y + 1 > 10)
                            {
                                currentNodes[i].Doors = (DoorGen)((byte)currentNodes[i].Doors & (byte)13);
                            }
                            else if (floor[currentNodes[i].X, currentNodes[i].Y + 1] == null)
                            {
                                floor[currentNodes[i].X, currentNodes[i].Y + 1] = new RoomNode(currentNodes[i].X, currentNodes[i].Y + 1, DoorGen.bottom);
                                newNodes.Add(floor[currentNodes[i].X, currentNodes[i].Y + 1]);
                            }
                            else
                            {
                                floor[currentNodes[i].X, currentNodes[i].Y + 1].Doors = (DoorGen)((byte)floor[currentNodes[i].X, currentNodes[i].Y + 1].Doors | (byte)8);
                            }
                        }
                        //If there is a right door
                        if (((byte)currentNodes[i].Doors & 1) == 1)
                        {
                            if (currentNodes[i].X + 1 > 10)
                            {
                                currentNodes[i].Doors = (DoorGen)((byte)currentNodes[i].Doors & (byte)14);
                            }
                            else if (floor[currentNodes[i].X + 1, currentNodes[i].Y] == null)
                            {
                                floor[currentNodes[i].X + 1, currentNodes[i].Y] = new RoomNode(currentNodes[i].X + 1, currentNodes[i].Y, DoorGen.right);
                                newNodes.Add(floor[currentNodes[i].X + 1, currentNodes[i].Y]);
                            }
                            else
                            {
                                floor[currentNodes[i].X + 1, currentNodes[i].Y].Doors = (DoorGen)((byte)floor[currentNodes[i].X + 1, currentNodes[i].Y].Doors | (byte)4);
                            }
                        }
                    }

                    //We made new rooms
                    currentNodes = newNodes;
                    instance++;

                }
            } while (!AssembleRooms(floor)); //See if this floor is acceptable, then add actual rooms to our map
        }

        /// <summary>
        /// This method links actual rooms together using the randomly generated map. This should ONLY EVER BE CALLED by the GenerateFloor() method
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        private bool AssembleRooms(RoomNode[,] floor)
        { 
            numRooms = 0;
            //Get the number of rooms on the floor
            for(int x = 0; x < floor.GetLength(0); x++)
            {
                for(int y = 0; y < floor.GetLength(1); y++)
                {
                    if(floor[x,y] != null)
                    {
                        numRooms++;
                    }
                }
            }

            //If it's too large or small, throw it out
            if (floorNumber < 17)
            {
                if (numRooms < 8 + ((floorNumber - 1) * 2.7))
                {
                    return false;
                }
                if (numRooms > 12 + ((floorNumber - 1) * 3.5))
                {
                    return false;
                }
            }
            //Formula becomes impossible to appease after about floor 17, so use set values
            else
            {
                if (numRooms < 60)
                {
                    return false;
                }
                if (numRooms > 90)
                {
                    return false;
                }
            }

            //Make a list of all the dead ends
            List<Point> deadEnds = new List<Point>();
            for (int x = 0; x < floor.GetLength(0); x++)
            {
                for (int y = 0; y < floor.GetLength(1); y++)
                {
                    if (floor[x, y] != null)
                    {
                        if (floor[x, y].Doors == DoorGen.top || floor[x, y].Doors == DoorGen.bottom || floor[x, y].Doors == DoorGen.left || floor[x, y].Doors == DoorGen.right)
                        {
                            deadEnds.Add(new Point(x, y));
                        }
                    }
                }
            }

            //There aren't enough dead ends to have both a boss and treasure room, so throw it out
            if(deadEnds.Count < 2)
            {
                return false;
            }
            //Randomly make one of the dead ends a boss and one a treasure room
            else
            {
                int rng = GameManager.RNG.Next(0, deadEnds.Count);
                floor[deadEnds[rng].X, deadEnds[rng].Y].Type = RoomType.Boss;
                deadEnds.RemoveAt(rng);
                rng = GameManager.RNG.Next(0, deadEnds.Count);
                floor[deadEnds[rng].X, deadEnds[rng].Y].Type = RoomType.Treasure;
            }

            //We don't want the boss room to be adjacent to the boss room
            if(floor[4,5].Type == RoomType.Boss || floor[6, 5].Type == RoomType.Boss || floor[5, 4].Type == RoomType.Boss || floor[5, 6].Type == RoomType.Boss)
            {
                return false;
            }

            //Add the spawn room
            this.currRoom = new Room(RoomType.Normal, "resources/rooms/spawn.grovlev");
            this.floorRooms[5, 5] = currRoom;

            //Populate the Room array
            List<string> files = new List<string>();
            for (int x = 0; x < floorRooms.GetLength(0); x++)
            {
                for(int y = 0; y < floorRooms.GetLength(1); y++)
                {
                    //See if there is a room at these coordinates
                    if(floor[x,y] != null)
                    {
                        //Retrieve the door configuration
                        byte temp = (byte)floor[x, y].Doors;
                        string path = "";
                        for (byte checker = 8; checker > 0; checker >>= 1)
                        {
                            //if the bit matches the checker, then it is a 1
                            if ((temp & checker) == checker)
                            {
                                path += '1';
                            }
                            else
                            {
                                path += '0';
                            }
                        }

                        //Add all the level files from the specific path
                        if (floor[x, y].Type == RoomType.Boss) //Boss rooms are in a specific subfolder; also picks a specific boss
                        {
                            string tempPath = @"resources\rooms\boss\";
                            List<string> bosses = new List<string>();
                            bosses.AddRange(System.IO.Directory.GetDirectories(tempPath));
                            tempPath = bosses[GameManager.RNG.Next(0, bosses.Count)];
                            path = tempPath + "\\" + path;
                        }
                        else if (floor[x, y].Type == RoomType.Treasure) //Treasure rooms need a specific directory
                            path = @"resources\rooms\treasure\" + path;
                        else
                            path = @"resources\rooms\" + path;
                        files.AddRange(System.IO.Directory.GetFiles(path));
                        //If there isn't already a room there, make one (basically only skips the spawn room)
                        if(floorRooms[x,y] == null)
                        {
                            floorRooms[x, y] = new Room(floor[x,y].Type, files[GameManager.RNG.Next(0, files.Count)]);
                        }
                    }
                    files.Clear();
                }
            }

            //Link all the rooms together
            for (int x = 0; x < floorRooms.GetLength(0); x++)
            {
                for (int y = 0; y < floorRooms.GetLength(1); y++)
                {
                    //if there is a room here
                    if(floorRooms[x,y] != null)
                    {
                        //If the room has a top door
                        if(floorRooms[x,y].Top != null)
                        {
                            floorRooms[x, y].Top.NextRoom = floorRooms[x, y - 1];
                        }
                        //If the room has a left door
                        if (floorRooms[x, y].Left != null)
                        {
                            floorRooms[x, y].Left.NextRoom = floorRooms[x - 1, y];
                        }
                        //If the room has a bottom door
                        if (floorRooms[x, y].Bottom != null)
                        {
                            floorRooms[x, y].Bottom.NextRoom = floorRooms[x, y + 1];
                        }
                        //If the room has a right door
                        if (floorRooms[x, y].Right != null)
                        {
                            floorRooms[x, y].Right.NextRoom = floorRooms[x + 1, y];
                        }
                    }
                }
            }

            currRoom.Visited = true;
            //Spawn this room's enemies
            currRoom.SpawnEnemies();

            return true;
        }

        //Prints the binary representation of a given byte
        private static void PrintBinary(byte b)
        {
            //start with the leftmost bit
            for (byte checker = 128; checker > 0; checker >>= 1)
            {

                //if the bit matches the checker, then it is a 1
                if ((b & checker) == checker)
                {
                    Console.Write('1');
                }
                else
                {
                    Console.Write('0');
                }
            }
            Console.WriteLine();
        }

        public void Update()
        {
            currRoom.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            currRoom.Draw(spriteBatch);
        }

		/// <summary>
		/// Checks to see if an entity's hitbox collides with a tile
		/// </summary>
		/// <param name="entity"> The entity to check collision with </param>
		/// <returns> List of tiles that entity passes through </returns>
		public List<Tile> CollidesWith(Entity entity)
		{
			List<Tile> tilesTouched = new List<Tile>();
			List<Tile> cornerTiles = new List<Tile>(); // Tiles touched by entity corners

            //Death
            if (entity.Hitbox.X / TileWidth < 0 || (entity.Hitbox.X + entity.Hitbox.Width) / TileWidth > 31 || entity.Hitbox.Y / TileWidth < 0 || (entity.Hitbox.Y + entity.Hitbox.Height) / TileWidth > 17)
            {
                tilesTouched.Add(new Tile(TileType.Death, 0, 0));
                return tilesTouched;
            }

			cornerTiles.Add(currRoom[entity.Hitbox.X / TileWidth, entity.Hitbox.Y / TileHeight]);
			cornerTiles.Add(currRoom[(entity.Hitbox.X + entity.Hitbox.Width) / TileWidth, entity.Hitbox.Y / TileHeight]);
			cornerTiles.Add(currRoom[entity.Hitbox.X / TileWidth, (entity.Hitbox.Y + entity.Hitbox.Height) / TileHeight]);
			cornerTiles.Add(currRoom[(entity.Hitbox.X + entity.Hitbox.Width) / TileWidth, (entity.Hitbox.Y + entity.Hitbox.Height) / TileHeight]);

			foreach(Tile cornerTile in cornerTiles)
			{
				int curTilesTouchedCount = tilesTouched.Count;

				if(tilesTouched.Count == 0)
				{
					tilesTouched.Add(cornerTile);
				}
				else
				{
                    // Checks if current cornerTile is already added into tilesTouched list
                    if(!tilesTouched.Contains(cornerTile)) tilesTouched.Add(cornerTile);
				}
			}
            return tilesTouched;
		}

        /// <summary>
        /// Returns whether or not a given point on the screen is in a passable tile
        /// </summary>
        public bool BlocksLineOfSight(Vector2 v)
        {
            return BlocksLineOfSight(v.X, v.Y);
        }
        /// <summary>
        /// Returns whether or not a given point on the screen is in a passable tile
        /// </summary>
        public bool BlocksLineOfSight(float x, float y)
        {
            return GetTileAt(x, y).BlocksLineOfSight;
        }

        public bool BlocksProjectiles(Vector2 v)
        {
            return BlocksLineOfSight(v.X, v.Y);
        }
        public bool BlocksProjectiles(float x, float y)
        {
            return GetTileAt(x, y).BlocksProjectiles;
        }

        public bool BlocksPathing(Vector2 v)
        {
            return BlocksPathing(v.X, v.Y);
        }
        public bool BlocksPathing(float x, float y)
        {
            return !GetTileAt(x, y).IsPassable;
        }

        public Tile GetTileAt(Vector2 position)
        {
            return GetTileAt(position.X, position.Y);
        }
        public Tile GetTileAt(float x, float y)
        {
            return currRoom[(int)(x / TileWidth), (int)(y / TileHeight)];
        }
        #endregion
    }
}
