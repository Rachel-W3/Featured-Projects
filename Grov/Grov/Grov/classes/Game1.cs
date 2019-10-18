using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;


namespace Grov
{

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Customizable display options
            //TODO: when we make options, these should be properties/in another class
        public int windowHeight = 1080;
        public int windowWidth = 1920;
        public bool fullScreen = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            GameManager.Initialize(Content, GraphicsDevice);

            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.PreferredBackBufferWidth = windowWidth;
            //Set the game to Fullscreen mode
            graphics.IsFullScreen = fullScreen;
            //This makes it a "soft" fullscreen, AKA borderless fullscreen. Less efficient but faster to initialize
            graphics.HardwareModeSwitch = fullScreen;
            graphics.ApplyChanges();

            IsMouseVisible = false;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            EntityManager.Player.Texture = new AnimatedTexture(Content.Load<Texture2D>("PlayerSprite"));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {}

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //If you tab out of the game, put it in pause and stop updating
            if (!this.IsActive && (GameManager.GameState == GameState.Game || GameManager.GameState == GameState.Map))
            {
                GameManager.GameState = GameState.PauseMenu;
            }
            else
                GameManager.Instance.Update(gameTime);

            if (GameManager.GameExit) Exit();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.ForestGreen);
            spriteBatch.Begin();

            GameManager.Instance.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
