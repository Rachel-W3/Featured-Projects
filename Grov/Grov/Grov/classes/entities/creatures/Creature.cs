using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * Authors
 * Jack Hoffman
 * Rachel Wong
 * Jake Zaia
 */ 

namespace Grov
{
    abstract class Creature: Entity
    {
        #region fields
        // ************* Fields ************* //

        protected float maxHP;
        protected float currentHP;
        protected bool melee;
        protected float fireRate;
        protected float fireDelay;
        protected float moveSpeed;
        protected float attackDamage;
        protected float projectileSpeed;
        protected Weapon weapon;
        protected bool facingRight;
        #endregion

        #region properties
        // ************* Properties ************* //

        public float MaxHP { get => maxHP; set => maxHP = value; }
        public float CurrHP { get => currentHP; set => currentHP = value; }
        public bool Melee { get => melee; set => melee = value; }
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public float AttackDamage { get => attackDamage; set => attackDamage = value; }
        public float ProjectileSpeed { get => projectileSpeed; set => projectileSpeed = value; }
        public Weapon Weapon { get => weapon; set => weapon = value; }
        public bool FacingRight { get => facingRight; }
        #endregion

        #region constructors
        // ************* Constructor ************* //

        public Creature(float maxHP, bool melee, float fireRate, float moveSpeed, float attackDamage, float projectileSpeed, Rectangle drawPos, Rectangle hitbox, Vector2 position, Vector2 velocity, bool isActive, AnimatedTexture texture) : base(drawPos, hitbox, position, velocity, isActive, texture)
        {
            this.maxHP = maxHP;
            this.currentHP = maxHP;
            this.melee = melee;
            this.fireRate = fireRate;
            this.fireDelay = fireRate;
            this.moveSpeed = moveSpeed;
            this.attackDamage = attackDamage;
            this.projectileSpeed = projectileSpeed;
            facingRight = false;
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        public override void Update()
        {
            this.Move();
            base.Update();
            if (weapon != null)
                weapon.Position = new Vector2(this.drawPos.X + (this.drawPos.Width - weapon.DrawPos.Width) / 2, this.drawPos.Y + (this.drawPos.Height - weapon.DrawPos.Height) / 2); ;
        }

        protected virtual void Move(){ }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
            {
                if (this.FacingRight)
                    spriteBatch.Draw(texture.GetNextTexture(), drawPos, null, drawColor, 0f, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 1f);
                else
                    spriteBatch.Draw(texture.GetNextTexture(), drawPos, drawColor);
            }
        }

        /// <summary>
        /// Returns whether or not this creature has line of sight to the target
        /// </summary>
        /// <param name="target">The entity the creature is checking for LoS</param>
        /// <returns>True if there is LoS</returns>
        public bool LineOfSight(Entity target)
        {

            Vector2 direction = new Vector2(target.Hitbox.X + target.DrawPos.Width/2 - this.Hitbox.X - this.Hitbox.Width/2, target.Hitbox.Y + target.Hitbox.Height/2 - this.Hitbox.Y - this.Hitbox.Height/2);

            float stop = direction.Length();
            direction = Vector2.Normalize(direction);
            for (float i = 5; i < stop - 20; i += 15)
            {
                if (FloorManager.Instance.BlocksLineOfSight((i * direction) + new Vector2(this.Hitbox.X + this.Hitbox.Width / 2, this.Hitbox.Y + this.Hitbox.Height / 2)))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether or not the entity can shoot at the target
        /// </summary>
        public bool LineOfFire(Entity target)
        {

            Vector2 direction = new Vector2(target.Hitbox.X + target.DrawPos.Width / 2 - this.Hitbox.X - this.Hitbox.Width / 2, target.Hitbox.Y + target.Hitbox.Height / 2 - this.Hitbox.Y - this.Hitbox.Height / 2);

            float stop = direction.Length();
            direction = Vector2.Normalize(direction);
            for (float i = 5; i < stop - 20; i += 15)
            {
                if (FloorManager.Instance.BlocksProjectiles((i * direction) + new Vector2(this.Hitbox.X + this.Hitbox.Width / 2, this.Hitbox.Y + this.Hitbox.Height / 2)))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether or not the entity can walk in a straight line towards the target
        /// </summary>
        public bool LineOfPathing(Entity target)
        {

            Vector2 direction = new Vector2(target.Hitbox.X + target.DrawPos.Width / 2 - this.Hitbox.X - this.Hitbox.Width / 2, target.Hitbox.Y + target.Hitbox.Height / 2 - this.Hitbox.Y - this.Hitbox.Height / 2);

            float stop = direction.Length();
            direction = Vector2.Normalize(direction);
            for (float i = 5; i < stop - 20; i += 15)
            {
                if (FloorManager.Instance.BlocksPathing((i * direction) + new Vector2(this.Hitbox.X + this.Hitbox.Width / 2, this.Hitbox.Y + this.Hitbox.Height / 2)))
                    return false;
            }

            return true;
        }
        #endregion
    }
}
