using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


// Authors: Jake Zaia, Jack Hoffman

namespace Grov
{
    enum ProjectileType
    {
        Fire,
        Bubble,
        Star,
        Twig,
        Leaf
    }

    class Projectile : Entity
    {
        #region fields
        // ************* Fields ************* //

        private float lifespan;
        private bool isFromPlayer;
        private bool noclip;
        private float damage;
        private ProjectileType type;
        #endregion

        #region properties
        // ************* Properties ************* //

        public float Lifespan { get => lifespan; set => lifespan = value; }
        public bool IsFromPlayer { get => isFromPlayer; set => isFromPlayer = value; }
        public bool Noclip { get => noclip; set => noclip = value; }
        public float Damage { get => damage; }
        public ProjectileType Type { get => type; }
        #endregion

        #region constructor
        // ************* Constructor ************* //

        public Projectile(float damage, float lifespan, bool isFromPlayer, bool noclip, Rectangle drawPos, Vector2 velocity, ProjectileType type) : base(drawPos, drawPos,new Vector2(drawPos.X, drawPos.Y), velocity, true, null)
        {
            this.damage = damage;
            this.lifespan = lifespan;
            this.isFromPlayer = isFromPlayer;
            this.noclip = noclip;
            this.type = type;
            this.texture = (AnimatedTexture) DisplayManager.ProjectileTextureMap[type].Clone();
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        public override void Update()
        {
            if (this.isActive)
            {
                this.lifespan -= 1;
                this.position += velocity;
                this.drawPos = new Rectangle((int)(this.position.X), (int)(this.position.Y), this.drawPos.Width, this.drawPos.Height);
                this.hitbox = this.drawPos;
                if(this.lifespan == 0)
                {
                    this.isActive = false;
                }
            }
            //Delete it
            else
            {
                if (this.drawPos.Width > 0)
                {
                    this.IsActive = false;
                    this.position = new Vector2(0f, 0f);
                    this.drawPos.Width = 0;
                    this.drawPos.Height = 0;
                }
                return;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (this.isActive)
            {
                if (velocity.X != 0 || velocity.Y != 0) //if velocity is not zero
                    spriteBatch.Draw(texture.GetNextTexture(), new Rectangle(drawPos.X + drawPos.Width/2, drawPos.Y + drawPos.Height/2, drawPos.Width, drawPos.Height), null, Color.White, (float)Math.Atan2(this.velocity.Y, this.velocity.X), new Vector2(drawPos.Width/2.0f, drawPos.Height/2.0f), SpriteEffects.None, 0f);
                else
                    spriteBatch.Draw(texture.GetNextTexture(), drawPos, Color.White);
            }
        }
        #endregion
    }
}
