using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/*
 * Authors
 * Jack Hoffman
 * Jake Zaia
 * Rachel Wong
 * Duncan Mott
 */
 
namespace Grov
{
    class Player: Creature
    {
        #region fields
        // ************* Fields ************* //

        private float maxMP;
        private float currMP;
        private int cooldown;
        private bool stoppedFiring;
        private Weapon secondary;
        private int keys;
        private int bombs;
        private Vector2 aimDirection;
        private bool isInputKeyboard;
        private int Iframes;
        private Weapon lastWeaponFired;
        #endregion

        #region properties
        // ************* Properties ************* //

        public float CurrMP
        {
            get => currMP;
            set
            {
                if (value >= 0)
                {
                    currMP = value;
                }
                else
                {
                    currMP = 0;
                }
            }
        }

        public float MaxMP { get => maxMP; set => maxMP = value; }
        public int Keys { get => keys; set => keys = value; }
        public int Bombs { get => bombs; set => bombs = value; }
        public Weapon Secondary { get => secondary; set => secondary = value; }
        public Weapon LastWeaponFired { get => lastWeaponFired; set => lastWeaponFired = value; }
        public int IFrames { get => Iframes; set => Iframes = value; }
        #endregion

        #region constructors
        // ************* Constructor ************* //

        public Player(float maxHP, float maxMP, float fireRate, float moveSpeed, float attackDamage, float projectileSpeed, Rectangle drawPos, Rectangle hitbox, Vector2 velocity, AnimatedTexture texture) : base(maxHP, false, fireRate, moveSpeed, attackDamage, projectileSpeed, drawPos, hitbox, new Vector2(drawPos.X, drawPos.Y), velocity, true, texture)
        {
            this.maxMP = maxMP;
            this.currMP = maxMP;
            keys = 0;
            bombs = 0;
            isInputKeyboard = true;  
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        /// <summary>
        /// Updates location and initiates all needed calculations
        /// </summary>
        public override void Update()
        {
            Point point = DrawPos.Location;
            base.Update();
            this.Aim();
            point = drawPos.Location - point;
            this.hitbox.Location += point;
            if(weapon != null)
                weapon.Update();
            if (secondary != null)
                secondary.Update();

            //If you're not shooting/weapon isn't in cooldown, recharge mana
            if(currMP < MaxMP && cooldown == 0 && (weapon == null || weapon.ReadyToFire(fireRate)))
            {
                currMP += .5f;
            }

            //If you're invinceable for whatever reason, increment time
            if(this.Iframes > 0)
            {
                Iframes--;
            }

            //Weapon cooldown
            if (cooldown > 0 && stoppedFiring) cooldown--;

        }

        /// <summary>
        /// Draw the player
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (this.isActive && this.Iframes == 0)
            {
                base.Draw(spriteBatch);
            }
            //Hitstun
            else if (this.isActive)
            {
                if (texture != null)
                    if (Iframes % 9 <= 4)
                    {
                        spriteBatch.Draw(texture.GetNextTexture(), drawPos, Color.White);
                    }
                    else spriteBatch.Draw(texture.GetNextTexture(), drawPos, Color.Black);
            }
        }

        /// <summary>
        /// Handles all movement-based calculations and inputs
        /// </summary>
        protected override void Move()
        {
           Vector2 direction = new Vector2(0f, 0f);

            // Handles keyboard input
            if (isInputKeyboard)
            {
                if (GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
                {
                    direction += new Vector2(0f, -1f);
                }
                if (GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                {
                    direction += new Vector2(0f, 1f);
                }
                if (GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
                {
                    direction += new Vector2(-1f, 0f);
                }
                if (GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                {
                    direction += new Vector2(1f, 0f);
                }
                if(weapon != null && ((GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space)
                    || GameManager.CurrentMouseState.LeftButton.Equals(ButtonState.Pressed)) && this.currMP > 0 && lastWeaponFired.ReadyToFire(fireRate) && cooldown == 0))
                {
                    this.Attack();
                    lastWeaponFired = weapon;
                    if (currMP <= 0) cooldown += weapon.Cooldown;
                    stoppedFiring = false;
                }
                //Switch primary weapon
                if(GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F) && !GameManager.PreviousKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F) && secondary != null)
                {
                    Weapon holder = weapon;
                    weapon = secondary;
                    secondary = holder;
                }
                //Stop the cooldown from recharging until button is released
                if(stoppedFiring == false && !(GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space)
                    || GameManager.CurrentMouseState.LeftButton.Equals(ButtonState.Pressed)))
                {
                    stoppedFiring = true;
                }

                direction.Normalize();

                if (GameManager.CurrentKeyboardState == GameManager.PreviousKeyboardState && GameManager.CurrentGamePadState != GameManager.PreviousGamePadState)
                {
                    isInputKeyboard = false;
                }
            }
            // Handles gamepad input
            else
            {
                direction = GameManager.CurrentGamePadState.ThumbSticks.Left;
                direction.Y = -direction.Y;

                if (GameManager.CurrentGamePadState.IsButtonDown(Buttons.RightTrigger) && this.currMP >= 0 && lastWeaponFired.ReadyToFire(fireRate) && cooldown == 0)
                {
                    this.Attack();
                    if (currMP <= 0) cooldown += weapon.Cooldown;
                    stoppedFiring = false;
                }

                //Stop the cooldown from recharging until button is released
                if (stoppedFiring == false && !GameManager.CurrentGamePadState.IsButtonDown(Buttons.RightTrigger))
                {
                    stoppedFiring = true;
                }
                //Switch primary and secondary weapons
                if(GameManager.CurrentGamePadState.IsButtonDown(Buttons.Y) && GameManager.PreviousGamePadState.IsButtonUp(Buttons.Y) && secondary != null)
                {
                    Weapon holder = weapon;
                    weapon = secondary;
                    secondary = holder;
                }

                if (GameManager.CurrentKeyboardState != GameManager.PreviousKeyboardState)
                {
                    isInputKeyboard = true;
                }
            }

            velocity = moveSpeed * direction;

            if (float.IsNaN(direction.X))
            {
                direction.X = 0;
            }
            if (float.IsNaN(direction.Y))
            {
                direction.Y = 0;
            }

            velocity = moveSpeed * direction;

            position += velocity;

            if (velocity.X > 0)
                this.facingRight = true;
            else if (velocity.X < 0)
                this.facingRight = false;
            
            velocity = new Vector2(0f, 0f);
        }

        /// <summary>
        /// Calculates the vector for the projectile aim
        /// </summary>
        public void Aim()
        {
            Vector2 direction = new Vector2(0f, 0f);
            bool isMouse = true;

            // Handles keyboard input
            if (isInputKeyboard)
            {
                if (GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                {
                    direction += new Vector2(0f, -1f);
                    isMouse = false;
                }
                if (GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                {
                    direction += new Vector2(0f, 1f);
                    isMouse = false;
                }
                if (GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                {
                    direction += new Vector2(-1f, 0f);
                    isMouse = false;
                }
                if (GameManager.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                {
                    direction += new Vector2(1f, 0f);
                    isMouse = false;
                }
                if (isMouse)
                {
                    direction = new Vector2(GameManager.CurrentMouseState.Position.X - (weapon.DrawPos.X + weapon.DrawPos.Width / 2), GameManager.CurrentMouseState.Position.Y - (weapon.DrawPos.Y + weapon.DrawPos.Height / 2));
                }


                if(GameManager.CurrentGamePadState.Triggers.Right != 0f && GameManager.PreviousGamePadState.Triggers.Right == 0f)
                {
                    isInputKeyboard = false;
                }
            }
            // Handles gamepad input
            else
            {
                direction = GamePad.GetState(0).ThumbSticks.Right;
                direction.Y = -direction.Y;
                if (GameManager.CurrentMouseState.LeftButton.Equals(ButtonState.Pressed) && !GameManager.PreviousMouseState.LeftButton.Equals(ButtonState.Pressed))
                {
                    isInputKeyboard = true;
                }
            }

            // Normalize and finish calculations
            direction.Normalize();
            if (!float.IsNaN(direction.Y) || !float.IsNaN(direction.X))
            {
                aimDirection = direction;

                if (float.IsNaN(aimDirection.X))
                {
                    aimDirection.X = 0f;
                }
                if (float.IsNaN(aimDirection.Y))
                {
                    aimDirection.Y = 0f;
                }
            }
            if(aimDirection == null)
            {
                aimDirection = new Vector2(1f, 0f);
            }
        }

        /// <summary>
        /// Creates a new projectile at the center of the player with a velocity based off of (float) projectileVelocity
        /// </summary>
        public void Attack()
        {
            //this.weapon.Position = new Vector2(this.position.X + (this.drawPos.Width - weapon.DrawPos.Width)/2, this.position.Y + (this.drawPos.Height - weapon.DrawPos.Height) / 2);
            this.weapon.Use(aimDirection * projectileSpeed);
            this.currMP -= weapon.ManaCost;
        }

        /// <summary>
        /// Changes player's abilities and stats based on the pick-up item player walks over
        /// </summary>
        public void Interact(Pickup pickup_item)
        {
            switch (pickup_item.PickupType)
            {
                case PickupType.Weapon:
                    if (secondary == null)
                    {
                        secondary = weapon;
                        weapon = (Weapon)pickup_item;
                    }
                    else
                    {
                        //Place the secondary item on the floor, and give it a location
                        weapon.Position = new Vector2(pickup_item.Position.X - 110, pickup_item.Position.Y);
                        weapon.DrawPos = new Rectangle((int)weapon.Position.X, (int)weapon.Position.Y, 60, 60);
                        weapon.Hitbox = weapon.DrawPos;
                        weapon.IsActive = true;
                        EntityManager.NewPickups.Add(weapon);
                        FloorManager.Instance.CurrRoom.PickupsInRoom.Add(weapon);
                        this.weapon = (Weapon)pickup_item;
                    }
                    break;
                case PickupType.Heart:
                    this.CurrHP += 20f;
                    if (this.CurrHP > this.MaxHP)
                        this.CurrHP = this.MaxHP;
                    break;
            }
        }
        #endregion
    }
}
