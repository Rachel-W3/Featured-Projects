using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

/*
 * Authors
 * Jack Hoffman
 * Duncan Mott
 */
namespace Grov
{
    /// <summary>
    /// This class defines a Grot boss and their behaviors
    /// </summary>
    class Grot : Enemy
    {
        #region fields
        enum BehaviorMode
        {
            Taunt = 0,
            SpinAttack = 1,
            PulseAttack = 2,
            Summon = 3,
            Idle = 4
        }
 
        private BehaviorMode currentBehavior;
        private int currentFrame;
        private int sign;
        #endregion

        #region constructors
        /// <summary>
        /// Creates a new Grot boss
        /// </summary>
        public Grot(EnemyType enemyType, int maxHP, bool melee, float fireRate, float attackDamage, float moveSpeed, float projectileSpeed, Rectangle drawPos, Vector2 velocity, string weaponName, int lungeTime, bool sturdy) : base(enemyType, 75, melee, fireRate, attackDamage, moveSpeed, projectileSpeed, new Rectangle(drawPos.X - FloorManager.TileWidth * 1, drawPos.Y - FloorManager.TileHeight * 3, FloorManager.TileWidth * 3, FloorManager.TileHeight * 5), velocity,"Grot", 0, true)
        {
            currentBehavior = BehaviorMode.Taunt;
            currentFrame = 0;
            sign = 0;
            this.hitbox = new Rectangle(this.drawPos.X, this.drawPos.Y + FloorManager.TileHeight * 2, FloorManager.TileWidth * 3, FloorManager.TileHeight * 3);
        }
        #endregion

        #region methods
        /// <summary>
        /// Determines what behaviors Grot should perform
        /// </summary>
        public override void Update()
        {
            if(this.currentHP <= 0)
            {
                this.isActive = false;
            }
            currentFrame++;
            if(this.hitstun > 0)
            {
                hitstun--;
            }
            switch (currentBehavior)
            {
                case BehaviorMode.Taunt:
                    this.Taunt();
                    break;
                case BehaviorMode.Idle:
                    this.Idle();
                    break;
                case BehaviorMode.SpinAttack:
                    this.SpinAttack();
                    break;
                case BehaviorMode.PulseAttack:
                    this.PulseAttack();
                    break;
                case BehaviorMode.Summon:
                    this.Summon();
                    break;
            }
        }

        /// <summary>
        /// 2 second taunt animation
        /// </summary>
        private void Taunt()
        {
            if(currentFrame >= 120)
            {
                this.currentBehavior = (BehaviorMode)GameManager.RNG.Next(2, 5); // Generates a new attack
                currentFrame = 0;
            }
        }

        /// <summary>
        /// 1 second idle animation
        /// </summary>
        private void Idle()
        {
            if(currentFrame >= 60)
            {
                this.currentBehavior = (BehaviorMode)GameManager.RNG.Next(2, 5); // Generates a new attack
                currentFrame = 0;
            }
        }

        /// <summary>
        /// Causes Grot to shoot beams of twigs and then rotate them around
        /// </summary>
        private void SpinAttack()
        {
            if (currentFrame == 1)
            {
                this.weapon = new Weapon(@"enemy\Grot", new Rectangle(this.drawPos.X + this.drawPos.Width / 2, this.drawPos.Y + this.drawPos.Height / 2, 1, 1), true, false);
            }
                double radians = 0f;

            if(currentFrame == 1)
            {
                sign = GameManager.RNG.Next(0, 2) * 2 - 1; // Generates a random direction
            }

            if (currentFrame < 90)
            {
                radians = (45) * (Math.PI / 180) * sign;
            }
            else
            {
                radians = ((currentFrame / 2) % 360) * (Math.PI / 180) * sign;
            }

            weapon.Use(Vector2.Normalize(new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians))));

            if(currentFrame >= 420)
            {
                currentFrame = 0;
                this.currentBehavior = BehaviorMode.Taunt;
                sign = 0;
            }
        }

        /// <summary>
        /// Creates a pulse of projectiles from the center of Grot
        /// </summary>
        private void PulseAttack()
        {
            if (currentFrame == 1)
            {
                this.weapon = new Weapon(@"enemy\Grot Pulse", new Rectangle(this.drawPos.X + this.drawPos.Width / 2, this.drawPos.Y + this.drawPos.Height / 2, 1, 1), true, false);
            }

            if (currentFrame % 60 == 0)
            {
                weapon.Use(new Vector2(1, 1));
            }

            if (currentFrame >= 200)
            {
                this.currentBehavior = (BehaviorMode)GameManager.RNG.Next(0, 2); // Generates a new attack
                currentFrame = 0;
            }
        }

        /// <summary>
        /// Summons a set of enemies to help Grot fight
        /// </summary>
        private void Summon()
        {
            if (currentFrame >= 0)
            {
                this.currentBehavior = (BehaviorMode)GameManager.RNG.Next(0, 2); // Generates a new attack
                currentFrame = 0;
            }
        }
        #endregion
    }
}
