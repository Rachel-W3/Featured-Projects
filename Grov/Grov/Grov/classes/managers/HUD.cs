using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

// Authors: Jack Hoffman

namespace Grov
{
    class HUD
    {
        // ************* Fields ************* //

        Texture2D healthBarFull;
        Texture2D healthBarEmpty;
        Texture2D manaBarFull;
        Texture2D manaBarEmpty;
        SpriteFont courierNew16;

        // ************* Constructors ************* //

        public HUD()
        {
            healthBarFull = DisplayManager.ContentManager.Load<Texture2D>("HealthBarFullSprite");
            healthBarEmpty = DisplayManager.ContentManager.Load<Texture2D>("HealthBarEmptySprite");
            manaBarFull = DisplayManager.ContentManager.Load<Texture2D>("ManaBarFullSprite");
            manaBarEmpty = DisplayManager.ContentManager.Load<Texture2D>("ManaBarEmptySprite");
            courierNew16 = DisplayManager.ContentManager.Load<SpriteFont>("CourierNew16");
        }

        // ************* Methods ************* //

        public void Initialize()
        {

        }

        public void Draw(SpriteBatch sb)
        {
            DrawHealth(sb);
            DrawMana(sb);

            if(EntityManager.Player.Weapon != null)
                sb.DrawString(courierNew16, string.Format("Primary: {0}", EntityManager.Player.Weapon.Name), new Vector2(12, 95), Color.White);
            if (EntityManager.Player.Secondary != null)
                sb.DrawString(courierNew16, string.Format("Secondary: {0}", EntityManager.Player.Secondary.Name), new Vector2(12, 115), Color.White);
        }

        // ************* Helper Methods ************* //

        private void DrawHealth(SpriteBatch sb)
        {
            sb.Draw(healthBarEmpty, new Rectangle(10, 5, 300, 45), Color.White);
            sb.Draw(healthBarFull, new Rectangle(10, 5, (int)(300 * EntityManager.Player.CurrHP / EntityManager.Player.MaxHP), 45), new Rectangle(0, 0,(int)(healthBarFull.Width * EntityManager.Player.CurrHP / EntityManager.Player.MaxHP), healthBarFull.Height), Color.White);
        }

        private void DrawMana(SpriteBatch sb)
        {
            sb.Draw(manaBarEmpty, new Rectangle(10, 55, 300, 45), Color.White);
            sb.Draw(manaBarFull, new Rectangle(10, 55, (int)((300 * EntityManager.Player.CurrMP / EntityManager.Player.MaxMP) + .5), 45), new Rectangle(0, 0, (int)(manaBarFull.Width * (EntityManager.Player.CurrMP / EntityManager.Player.MaxMP)), manaBarFull.Height), Color.White);
        }
    }
}