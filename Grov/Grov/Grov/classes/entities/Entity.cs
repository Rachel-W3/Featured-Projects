using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * Authors:
 * Jack Hoffman
 * Jake Zaia
 */

namespace Grov
{
    class Entity
    {
        #region fields
        // ************* Fields ************* //

        protected Rectangle drawPos;
        protected Rectangle hitbox;
        protected Vector2 position;
        protected Vector2 velocity;
        protected bool isActive;
        protected AnimatedTexture texture;
        protected Color drawColor;

        #endregion

        #region properties
        // ************* Properties ************* //

        public Rectangle DrawPos { get => drawPos; set => drawPos = value; }
        public Rectangle Hitbox { get => hitbox; set => hitbox = value; }
        public Vector2 Position { get => position; set => position = value; }
        public Vector2 Velocity { get => velocity; set => velocity = value; }
        public bool IsActive { get => isActive; set => isActive = value; }
        public AnimatedTexture Texture { get => texture; set => texture = value; }
        #endregion

        #region constructors
        // ************* Constructors ************* //

        public Entity(Rectangle drawPos, AnimatedTexture texture)
        {
            this.drawPos = drawPos;
            this.hitbox = drawPos;
            this.position = new Vector2(drawPos.X, drawPos.Y);
            this.velocity = new Vector2(0, 0);
            this.isActive = true;
            this.texture = texture;
        }

        public Entity(Rectangle drawPos, Rectangle hitbox, Vector2 position, Vector2 velocity, bool isActive, AnimatedTexture texture)
        {
            this.drawColor = Color.White;
            this.drawPos = drawPos;
            this.hitbox = hitbox;
            this.position = position;
            this.velocity = velocity;
            this.isActive = isActive;
            this.texture = texture;
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        /// <summary>
        /// Updates draw position to equal position
        /// </summary>
        public virtual void Update()
        {
            drawPos = new Rectangle((int)(position.X + .5f), (int)(position.Y + .5f), drawPos.Width, drawPos.Height);
        }

        /// <summary>
        /// Draws the object to its corresponding draw position
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                spriteBatch.Draw(texture.GetNextTexture(), drawPos, drawColor);
        }

        /// <summary>
        /// Checks to see if an entity's hitbox collides with another
        /// </summary>
        /// <param name="entity">The entity to check collision with</param>
        /// <returns>True if collided, False if not</returns>
        public virtual bool CollidesWith(Entity entity)
        {
            return Hitbox.Intersects(entity.Hitbox);
        }
        #endregion
    }
}
