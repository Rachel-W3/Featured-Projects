using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


// Authors: Jake Zaia, Duncan Mott, Rachel Wong

namespace Grov
{
    enum RoomType
    {
        Normal,
        Boss,
        //Shop,
        Treasure,
        //Secret
    }

	class Room
	{
        #region fields
        // ************* Fields ************* //

        private Entrance top, bottom, left, right;
		private RoomType type;
		private bool isCleared;
        private string filename;
        private bool visited;

		private Tile[][] tiles;
        private List<Pickup> pickupsInRoom;
        #endregion

        #region properties
        // ************* Properties ************* //

        public Entrance Top { get => top; set => top = value; }
        public Entrance Bottom { get => bottom; set => bottom = value; }
        public Entrance Left { get => left; set => left = value; }
        public Entrance Right { get => right; set => right = value; }
        public RoomType Type { get => type; set => type = value; }
        public bool IsCleared { get => isCleared; set => isCleared = value; }
        public List<Pickup> PickupsInRoom { get => pickupsInRoom; }
        public bool Visited { get => visited; set => visited = value; }

        public Tile this[int x, int y]
        {
            get => tiles[x][y];
            set => tiles[x][y] = value;
        }
        #endregion

        #region constructor
        // ************* Constructor ************* //

        public Room(RoomType type, string fileName)
        {
            isCleared = false;
            visited = false;
            tiles = new Tile[32][];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new Tile[18];
            }
            this.type = type;

            this.filename = fileName;
            ReadFromFile(fileName);

            pickupsInRoom = new List<Pickup>();

            if(this.Type == RoomType.Treasure)
            {
                //Pick a weapon
                string weaponFileName = Weapon.GenRandomFilename();
                    //Don't bother if we've seen all of them
                if(GameManager.Instance.SpawnedWeapons.Count < Weapon.GetAllFilenames().Length)
                {
                    while(GameManager.Instance.SpawnedWeapons.Contains(weaponFileName))
                    {
                        weaponFileName = Weapon.GenRandomFilename();
                    }
                }

                GameManager.Instance.SpawnedWeapons.Add(weaponFileName);

                //pickupsInRoom.Add(new Pickup(PickupType.Heart, new Rectangle(DisplayManager.GraphicsDevice.Viewport.Width/2 - 30, DisplayManager.GraphicsDevice.Viewport.Height / 2 - 30, 60, 60)));
                pickupsInRoom.Add(new Weapon(weaponFileName, new Rectangle(DisplayManager.GraphicsDevice.Viewport.Width / 2 - 30, DisplayManager.GraphicsDevice.Viewport.Height / 2 - 30, 60, 60), true, true));
            }

            // Testing - adds either a random weapon or a heart into the room
            //int pickUpType = FloorManager.RNG.Next(0, 2);
            //pickupsInRoom.Add(new Pickup((PickupType)pickUpType, new Rectangle(new Point(100, 100), new Point(50, 50)), ))
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        /// <summary>
        /// Determines state of entrances (open or closed)
        /// </summary>
        public void Update()
        {
            //If the room is cleared, make sure the doors are opened
            if (isCleared)
            {
                if (left != null)
                {
                    left.OpenDoor();
                }
                if (right != null)
                {
                    right.OpenDoor();
                }
                if (top != null)
                {
                    top.OpenDoor();
                }
                if (bottom != null)
                {
                    bottom.OpenDoor();
                }

                foreach(Tile[] tileArray in tiles)
                {
                    foreach(Tile tile in tileArray)
                    {
                        if(tile.Type == TileType.BossDoor)
                        {
                            tile.IsPassable = true;
                        }
                    }
                }
            }

            //If there are pickups in the room, check if they're still there and remove them if they aren't
            if(pickupsInRoom.Count > 0)
            {
                for(int i = 0; i < pickupsInRoom.Count; i++)
                {
                    if (!pickupsInRoom[i].IsActive)
                        pickupsInRoom.RemoveAt(i);
                }
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Draw every tile
			for (int x = 0; x < tiles.Length; x++)
			{
				for (int y = 0; y < tiles[x].Length; y++)
				{
                    //Current tile
					Tile cur = tiles[x][y];

                    //Handles all non-door tiles
                    if(cur.Type != TileType.Entrance && cur.Type != TileType.BossDoor)
					    spriteBatch.Draw(DisplayManager.TileTextureMap[(int)cur.Type].GetNextTexture(), 
						                 new Rectangle(FloorManager.TileWidth * x, FloorManager.TileHeight * y, FloorManager.TileWidth, FloorManager.TileHeight), 
									     Color.White);
                    //If the room is cleared, draw doors as "grass"
                    else if(isCleared)
                        //Doors leading to boss rooms should be tinted red to signify danger
                        if(cur.Type == TileType.Entrance && ((x == 0 && left.NextRoom.Type == RoomType.Boss) || (x == 31 && right.NextRoom.Type == RoomType.Boss) || (y == 0 && top.NextRoom.Type == RoomType.Boss) || (y == 17 && bottom.NextRoom.Type == RoomType.Boss)))
                            spriteBatch.Draw(DisplayManager.TileTextureMap[(int)TileType.Floor].GetNextTexture(),
                                         new Rectangle(FloorManager.TileWidth * x, FloorManager.TileHeight * y, FloorManager.TileWidth, FloorManager.TileHeight),
                                         Color.Red);
                        //Normal doors are normal
                        else
                        spriteBatch.Draw(DisplayManager.TileTextureMap[(int)TileType.Floor].GetNextTexture(),
                                         new Rectangle(FloorManager.TileWidth * x, FloorManager.TileHeight * y, FloorManager.TileWidth, FloorManager.TileHeight),
                                         Color.White);
                    //If the room isn't cleared, draw doors as "walls"
                    else
                        spriteBatch.Draw(DisplayManager.TileTextureMap[(int)TileType.Wall].GetNextTexture(),
                                         new Rectangle(FloorManager.TileWidth * x, FloorManager.TileHeight * y, FloorManager.TileWidth, FloorManager.TileHeight),
                                         Color.White);
                }
			}

            if (isCleared)
            {

                //foreach(Pickup pickup in pickupsInRoom)
                //{
                //    pickup.Draw(spriteBatch);
                //}
            }
		}

        /// <summary>
        /// Read room arrays from the room file
        /// </summary>
        /// <param name="filename"></param>
        private void ReadFromFile(string filename)
        {
            FileStream stream = File.OpenRead(filename);
            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(stream);

                //Handle tiles
                //Height
                for (int y = 0; y < 18; y++)
                {
                    //Width
                    for(int x = 0; x < 32; x++)
                    {
                        int thisTile = reader.ReadInt32();

                        tiles[x][y] = new Tile((TileType)thisTile, x, y);
                    }
                }

                //Check for doors, and create a new entrance object (linked list) pointing to the neighboring room
                //Check top/bottom for door
                for(int i = 0; i < 32; i++)
                {
                    //Top
                    if(tiles[i][0].Type == TileType.Entrance)
                    {
                        if (top == null)
                            top = new Entrance();
                        top.AddTile(tiles[i][0]);
                        top.UpdateLocation(new Point(i, 0));
                    }
                    //Bottom
                    if (tiles[i][17].Type == TileType.Entrance)
                    {
                        if (bottom == null)
                            bottom = new Entrance();
                        bottom.AddTile(tiles[i][17]);
                        bottom.UpdateLocation(new Point(i, 17));
                    }
                }
                //Check left/right for door
                for (int i = 0; i < 18; i++)
                {
                    //Left
                    if (tiles[0][i].Type == TileType.Entrance)
                    {
                        if (left == null)
                            left = new Entrance();
                        left.AddTile(tiles[0][i]);
                        left.UpdateLocation(new Point(0, i));
                    }
                    //Right
                    if (tiles[31][i].Type == TileType.Entrance)
                    {
                        if (right == null)
                            right = new Entrance();
                        right.AddTile(tiles[31][i]);
                        right.UpdateLocation(new Point(31, i));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        //Read enemies from the level file and spawn them when you enter the room
        public void SpawnEnemies()
        {
            FileStream stream = File.OpenRead(filename);
            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(stream);

                //Skip the tile info
                for(int i = 0; i < 32 * 18; i++)
                {
                    reader.ReadInt32();
                }

                //How many enemies are in the room?
                int enemyCount = reader.ReadInt32();

                //Create each enemy in its designated position
                for (int i = 0; i < enemyCount; i++)
                {
                    //Read coordinates, and enemy type
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    EnemyType type = (EnemyType)reader.ReadInt32();

                    EntityManager.Instance.SpawnEnemies(type, new Vector2(x * FloorManager.TileWidth, y * FloorManager.TileHeight));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        //Called when the player enters the room; takes every pickup in the list of pickups and spawns it
        public void SpawnPickups()
        {
            foreach(Pickup pickup in pickupsInRoom)
            {
                //Center the pickup in treasure rooms
                if (this.type == RoomType.Treasure)
                {
                    pickup.Position = new Vector2(DisplayManager.GraphicsDevice.Viewport.Width / 2 - 30, DisplayManager.GraphicsDevice.Viewport.Height / 2 - 30);
                    pickup.DrawPos = new Rectangle(DisplayManager.GraphicsDevice.Viewport.Width / 2 - 30, DisplayManager.GraphicsDevice.Viewport.Height / 2 - 30, 60, 60);
                    pickup.Hitbox = new Rectangle(DisplayManager.GraphicsDevice.Viewport.Width / 2 - 30, DisplayManager.GraphicsDevice.Viewport.Height / 2 - 30, 60, 60);
                }
                EntityManager.Instance.SpawnPickup(pickup);
            }
        }

        #endregion
    }
}
