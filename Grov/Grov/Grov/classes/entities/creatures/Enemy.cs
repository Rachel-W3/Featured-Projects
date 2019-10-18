using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// Authors: Jake Zaia, Rachel Wong, Jack Hoffman, Duncan Mott

namespace Grov
{
    enum EnemyType
    {
        //Bosses
        Grot = -1,
        ForestGiant = -2,

        //Normies
		RedShroomlet = 0,
        FireShroomlet = 1,
        PurpleShroomlet = 2
    }

    class Enemy : Creature
    {
        #region fields
        // ************* Fields ************* //

        protected EnemyType enemyType;
        protected int hitstun;
        protected int lungeTime;
        protected int timeSinceLunge;
        protected bool sturdy;
        protected Pathfinder pathfinder;
        protected List<Tile> currentPath;
        #endregion

        #region properties
        // ************* Properties ************* //

        public int Hitstun { get => hitstun; set => hitstun = value; }
        public bool Sturdy { get => sturdy; }
        public EnemyType EnemyType { get => enemyType; }
        #endregion

        #region constructor
        // ************* Constructor ************* //

        public Enemy(EnemyType enemyType, int maxHP, bool melee, float fireRate, float attackDamage, float moveSpeed, float projectileSpeed, Rectangle drawPos, Vector2 velocity, string weaponName, int lungeTime, bool sturdy) : base(maxHP, melee, fireRate, moveSpeed, attackDamage, projectileSpeed, drawPos, drawPos, new Vector2(drawPos.X, drawPos.Y), velocity, true, DisplayManager.EnemyTextureMap[enemyType])
        {
            this.enemyType = enemyType;
            this.sturdy = sturdy;
            pathfinder = new Pathfinder();
            if (melee)
            {
                this.lungeTime = lungeTime;
            }
            else
            {
                if (weaponName != null && weaponName != "null")
                {
                    this.weapon = new Weapon(@"enemy\" + weaponName, drawPos, false, false);
                }
            }
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        /// <summary>
        /// Updates all necessary values of the enemy class
        /// </summary>
        public override void Update()
        {
            if (IsActive)
            {
                //If the ennemy needs to die
                if (this.currentHP <= 0)
                {
                    this.IsActive = false;
                    //Remove the hitbox immediately
                    this.position = new Vector2(0f, 0f);
                    this.drawPos.Width = 0;
                    this.drawPos.Height = 0;
                    return;
                }

                //Target the player
                Entity target = EntityManager.Player;

                //Enemy will freeze if it's been hit recently, but not if it's sturdy
                if (hitstun == 0 || this.Sturdy)
                {
                    //Attempt to attack the target
                    this.Attack(target);
                    //If we're not touching the player, move closer to it
                    if (!this.hitbox.Intersects(target.Hitbox))
                    {
                        this.Move(target);
                    }
                }
                //Decrease remaining hitstun time
                if(hitstun != 0 || (hitstun != 0 && this.Sturdy))
                {
                    hitstun--;
                }

                base.Update();
                hitbox = drawPos;

                //Weapon needs to update, updates firerate
                if (!melee && this.Weapon != null)
                {
                    weapon.Update();
                }
            }
        }

        /// <summary>
        /// Draws the enemy at its position on the screen
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (this.isActive && this.hitstun == 0)
                drawColor = Color.White;
            //If it's in hitstun, draw it red
            else if(this.isActive)
                drawColor = Color.Red;
            base.Draw(spriteBatch);
        }

        /// <summary>
        /// Use the enemy's attck if it is prepared
        /// </summary>
        /// <param name="target">The entity the enemy is attacking</param>
        private void Attack(Entity target)
        {
            //If we can see the player
            if (LineOfSight(target))
            {
                //If this is a melee enemy, lunge at him in bursts of time
                if (melee)
                {
                    if (timeSinceLunge == lungeTime)
                        moveSpeed /= 1.5f;
                    else if (timeSinceLunge == 0)
                        moveSpeed *= 1.5f;
                    else if (timeSinceLunge >= 2 * lungeTime)
                        timeSinceLunge = -1;
                    timeSinceLunge++;
                }
                //If we have a weapon, shoot it
                else if (weapon != null && weapon.ReadyToFire(fireRate))
                {
                    Vector2 fireDirection = Vector2.Normalize(new Vector2(EntityManager.Player.Position.X + EntityManager.Player.DrawPos.Width / 2 - this.Position.X - this.Hitbox.Width / 2, EntityManager.Player.Position.Y + EntityManager.Player.DrawPos.Height / 2 - this.Position.Y - this.Hitbox.Height / 2));
                    Weapon.Use(fireDirection);
                }
            }
        }

		/// <summary>
		/// Handles enemy movement and actions
		/// </summary>
		protected void Move(Entity target)
		{
            Vector2 selfPos = new Vector2(this.Hitbox.X + this.Hitbox.Width / 2, this.Hitbox.Y + this.Hitbox.Height / 2);
            Vector2 targetPos = new Vector2(target.Hitbox.X + target.Hitbox.Width / 2, target.Hitbox.Y + target.Hitbox.Height / 2);
            Vector2 direction = targetPos - selfPos;

            // If I'm ranged and can fire at you, don't bother moving
            if ((!melee && enemyType != EnemyType.ForestGiant) && LineOfFire(target) && direction.Length() < weapon.ProjectileLifeSpan * weapon.ShotSpeed)
                return;

            // If you can walk in a straight line to the target, don't bother pathfinding
            if (LineOfPathing(target))
            {
                direction.Normalize();
                velocity = direction * moveSpeed;
                position += velocity;
            }

            //Otherwise pathfinding is necessary
            else
            {
                // Only run the pathfinding algo again if the player has moved
                if (currentPath == null || currentPath.Count <= 0 || currentPath[0] != FloorManager.Instance.GetTileAt(targetPos))
                {
                    currentPath = pathfinder.GetPathToTarget(position, targetPos);
                }

                // If I've reached the next tile I'm looking for, pop it off the list of tiles to walk to
                if (FloorManager.Instance.GetTileAt(selfPos) == currentPath[currentPath.Count - 1])
                {
                    currentPath.RemoveAt(currentPath.Count - 1);
                }

                //Start walking towards a tile that will bring me closer to my target
                Point targetPoint = currentPath[currentPath.Count - 1].Location;
                direction = new Vector2((targetPoint.X + .5f) * FloorManager.TileWidth - selfPos.X, (targetPoint.Y + .5f) * FloorManager.TileHeight - selfPos.Y);
                direction.Normalize();

                velocity = direction * moveSpeed;
                position += velocity;

                this.fireDelay = FireRate;
            }

            if (velocity.X < 0)
                this.facingRight = false;
            else if(velocity.X > 0)
                this.facingRight = true;

        }
        #endregion
    }
}
