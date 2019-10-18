using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


// Authors: Jake Zaia

namespace Grov
{
    enum ShotType
    {
        Normal,   // symmetrical cone in one direction
        Radial,   // laterally from all sides of the character
        Spread,   // in a cone, but more randomly spread out
        Clump,     // a straight line of packets/clumps of projectiles
        Bubble      //Shield
    }

    class Weapon : Pickup
    {
        #region fields
        // ************* Fields ************* //

        private string name;
        private string filename;
        private int fireRate;
        private float atkDamage;
        private float manaCost;
        private int numProjectiles;
        private ShotType shotType;
        private ProjectileType projectileType;
        private float shotSpeed;
        private int fireDelay;
        private int cooldown = 60;
        private int hitstun = 7;
        private int projectileLifeSpan;
        private bool noclip;
        private bool playerWeapon;
        #endregion

        #region properties
        // ************* Properties ************* //

        public string Filename { get => filename; }
        public string Name { get => name; }
        public float FireRate { get => fireRate; }
        public float AttackDamage { get => atkDamage; }
        public float ManaCost { get => manaCost; }
        public int NumProjectiles { get => numProjectiles; }
        public float ShotSpeed { get => shotSpeed; set => shotSpeed = value; }
        public ShotType ShotType { get => shotType; }
        public int Cooldown { get => cooldown; }
        public int Hitstun { get => hitstun; }
        public int ProjectileLifeSpan { get => projectileLifeSpan; }
        public ProjectileType ProjectType { get => projectileType; }
        #endregion

        #region constructor
        // ************* Constructor ************* //

        public Weapon(string filename, Rectangle drawPos, bool isActive, bool playerWeapon) : base(PickupType.Weapon, drawPos)
        {
            this.filename = filename;
            ReadFromFile(@"resources\weapons\" + filename + ".txt");
            texture = (AnimatedTexture) DisplayManager.WeaponTextureMap[projectileType].Clone();
            this.isActive = isActive;
            this.playerWeapon = playerWeapon;
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        private void ReadFromFile(string filename)
        {
            StreamReader reader = null;
            try {
                reader = new StreamReader(filename);

                name = reader.ReadLine();
                fireRate = int.Parse(reader.ReadLine());
                atkDamage = float.Parse(reader.ReadLine());
                manaCost = float.Parse(reader.ReadLine());
                numProjectiles = int.Parse(reader.ReadLine());
                shotSpeed = float.Parse(reader.ReadLine());
                shotType = (ShotType) Enum.Parse(typeof(ShotType), reader.ReadLine(), true);
                projectileLifeSpan = int.Parse(reader.ReadLine());
                hitstun = int.Parse(reader.ReadLine());
                projectileType = (ProjectileType) Enum.Parse(typeof(ProjectileType), reader.ReadLine(), true);
                noclip = bool.Parse(reader.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        public override void Update()
        {
            if (!isActive)
            {
                //Handles time innetween shots
                if (fireDelay < fireRate)
                {
                    fireDelay++;
                }
            }

            base.Update();
        }

        //Shoots projectiles in specified direction, behaviour dependant on weapon type
        public void Use(Vector2 direction)
        {
            float speedModifier = Math.Abs(direction.Length());
            int projectileSize = 30;
            Rectangle origin = new Rectangle(drawPos.X + (drawPos.Width - projectileSize) / 2, drawPos.Y + (drawPos.Height - projectileSize) / 2, projectileSize, projectileSize);

            //Audio stuff
            switch (projectileType)
            {
                case ProjectileType.Fire:
                    AudioManager.Instance.PlayEffect("FireShot");
                    break;

                case ProjectileType.Bubble:
                    AudioManager.Instance.PlayEffect("BubbleCast");
                    break;
            }

            //Create projectiles
            switch (shotType)
            {
                //Shots are fired in a straight line, or in a cone if there are multiple projectiles per shot
                case ShotType.Normal:
                    float offset = (float) Math.PI / (numProjectiles+1);
                    float playerOffset = (float) Math.Atan2(direction.X, direction.Y);
                    
                    for (int i = 1; i < numProjectiles + 1; i++)
                    {
                        Vector2 projVelocity = shotSpeed * speedModifier * (new Vector2(-1 * (float) Math.Cos(playerOffset + i * offset), (float) Math.Sin(playerOffset + i * offset)));
                        EntityManager.AddProjectile(new Projectile(atkDamage, projectileLifeSpan, playerWeapon, noclip, origin, projVelocity, projectileType));
                    }
                    break;
                //Shots are fired in a circle around the player
                case ShotType.Radial:
                    float originTheta = (float) Math.Atan2(direction.Y, direction.X);
                    float angleOffset = (float) Math.PI * 2 / numProjectiles;

                    for (int i = 0; i < numProjectiles; i++)
                    {
                        Vector2 projVelocity = shotSpeed * speedModifier * (new Vector2( (float) Math.Cos(originTheta + i*angleOffset), (float) Math.Sin(originTheta + i * angleOffset)));
                        EntityManager.AddProjectile(new Projectile(atkDamage, projectileLifeSpan, playerWeapon, noclip, origin, projVelocity, projectileType));
                    }
                    break;
                //Projectiles are shot in a cone, but their direction is totally random within the cone
                case ShotType.Spread:
                    float theta = (float)Math.Atan2(direction.Y, direction.X);

                    for (int i = 0; i < numProjectiles; i++) {
                        float phi = theta + GameManager.RNG.Next(-128, 128) / 244.46199f;  // Random "percent" * PI/6   (but simplified)
                        Vector2 projVelocity = shotSpeed * speedModifier * (new Vector2((float) Math.Cos(phi), (float) Math.Sin(phi)));
                        EntityManager.AddProjectile(new Projectile(atkDamage, projectileLifeSpan, playerWeapon, noclip, origin, projVelocity, projectileType));
                    }
                    break;
                //Creates a bubble around the player
                case ShotType.Bubble:
                    Projectile bubble = new Projectile(atkDamage, projectileLifeSpan, playerWeapon, noclip, new Rectangle(EntityManager.Player.DrawPos.X - (170 - EntityManager.Player.DrawPos.Width) / 2, EntityManager.Player.DrawPos.Y - (170 - EntityManager.Player.DrawPos.Height) / 2, 170, 170), new Vector2(0f, 0f), projectileType);
                    EntityManager.AddProjectile(bubble);
                    break;
                default:
                    throw new NotImplementedException();
            }

            this.fireDelay = 0;
        }

        /// <summary>
        /// Checks to see if the weapon is ready to be fired before
        /// </summary>
        /// <param name="multiplier">The entity's firerate mulitplier</param>
        /// <returns>True if ready to fire, False if not</returns>
        public bool ReadyToFire(float multiplier)
        {
            return (this.fireRate <= this.fireDelay * multiplier);
        }

        /// <summary>
        /// Gets all weapon file names
        /// </summary>
        /// <returns>An array of all weapon filenames</returns>
        public static string[] GetAllFilenames(bool isPlayerWeapon = true)
        {
            string[] filenames = Directory.GetFiles(@"resources\weapons\" + (isPlayerWeapon? @"player\" : @"enemy\"));
            for (int i = 0; i < filenames.Length; i++)
            {
                filenames[i] = filenames[i].Split('\\')[3];
                filenames[i] = filenames[i].Split('.')[0];
                filenames[i] = (isPlayerWeapon ? @"player\" : @"enemy\") + filenames[i];
            }

            return filenames;
        }

        /// <summary>
        /// Generates the filename of a random weapon excluding the development Weapon
        /// </summary>
        /// <returns>A random weapon's filename </returns>
        public static string GenRandomFilename(bool isPlayerWeapon = true)
        {
            string[] filenames = GetAllFilenames(isPlayerWeapon);

            return filenames[GameManager.RNG.Next(filenames.Length)];
        }
        #endregion
    }
}
