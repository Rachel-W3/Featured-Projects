using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


// Authors: Rachel Wong


namespace Grov
{
    enum MenuButtons
    {
        Start,
        Help,
        Exit
    }

    enum PauseButtons
    {
        Resume,
        Restart,
        ReturnMenu,
        Exit
    }

    enum ConfirmationButtons
    {
        Yes,
        No
    }

    class Button
    {
        #region fields
        private Texture2D noHover;
        private Texture2D hover;
        private Rectangle rect;
        private bool isHighlighted;
        #endregion

        #region properties
        public Texture2D NoHover { get => noHover; set => noHover = value; }
        public Texture2D Hover { get => hover; set => hover = value; }
        public Rectangle Rect { get => rect; }
        public bool IsHighlighted { get => isHighlighted; set => isHighlighted = value; }
        #endregion

        #region constructor
        public Button(Rectangle rect)
        {
            this.rect = rect;
            isHighlighted = false;
        }
        #endregion

        #region methods
        //Draws button image depending on whether the pointer is on it or not
        public void Draw(SpriteBatch spriteBatch)
        {
            if (isHighlighted)
            {
                spriteBatch.Draw(hover, rect, Color.White);
            }
            else
            {
                spriteBatch.Draw(noHover, rect, Color.White);
            }
        }
        #endregion
    }
}
