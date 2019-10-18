using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// Authors: Jake Zaia

namespace Grov
{
    class AnimatedTexture : ICloneable
    {
        #region fields
        // ************* Fields ************* //

        private List<Texture2D> textures;
        private int currframe;
        private int frameTime;

        #endregion

        #region properties
        // ************* Properties ************* //
        public int Width { get => textures[CurrFrame].Width; }
        public int Height { get => textures[CurrFrame].Height; }
        public int NumFrames { get => textures.Count; }
        private int CurrFrame { get => currframe / frameTime; set => currframe = (currframe + value) % (NumFrames * FrameTime); }
        public int FrameTime { get => frameTime; set => frameTime = value; }

        #endregion

        #region constructors
        // ************* Constructors ************* //

        public AnimatedTexture()
        {
            textures = new List<Texture2D>();
            frameTime = 30;
        }

        public AnimatedTexture(Texture2D firstTexture) : this()
        {
            textures.Add(firstTexture);
        }
        #endregion

        #region methods
        // ************* Methods ************* //

        /// <summary>
        /// Creates an independant copy of this texture, useful for when multiple of the same texture must be used
        /// </summary>
        /// <returns> A copy of this object </returns>
        public object Clone()
        {
            AnimatedTexture newTexture = new AnimatedTexture();
            foreach (Texture2D texture in textures)
            {
                newTexture.AddTexture(texture);
            }
            return newTexture;
        }

        /// <summary>
        /// Returns the next texture in the sequence
        /// </summary>
        /// <returns>The next texture</returns>
        public Texture2D GetNextTexture()
        {
            CurrFrame++;
            return textures[CurrFrame];
        }

        /// <summary>
        /// Adds a new texture to the end of the animation sequence
        /// </summary>
        /// <param name="texture">The texture to add</param>
        public void AddTexture(Texture2D texture)
        {
            textures.Add(texture);
        }

        #endregion
    }
}
