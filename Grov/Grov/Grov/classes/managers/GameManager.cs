using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

// Authors: Jake Zaia, Rachel Wong, Jack Hoffman

namespace Grov
{
    enum GameState
    {
        Game,
        Menu,
        PauseMenu,
        ConfirmationMenu, // For when player wants to restart or exit game
        Map,
        Help,
        GameOver
    }

    class GameManager
    {
        #region fields
        // ************* Fields ************* //

        private static GameManager instance;
        private bool devmode = false;
        private bool gameExit;
        private Random rng;
        private GameState gameState;
        private List<String> spawnedWeapons;

        private GamePadState previousGamePadState;
        private GamePadState currentGamePadState;
        private KeyboardState previousKeyboardState;
        private KeyboardState currentKeyboardState;
        private MouseState previousMouseState;
        private MouseState currentMouseState;
        
        #endregion

        #region properties
        // ************* Properties ************* //

        public static GameManager Instance { get => instance; }
        public static bool DEVMODE { get => instance.devmode; }
        public static Random RNG { get => instance.rng; }
        public static GameState GameState { get => instance.gameState; set => instance.gameState = value; }
        public static bool GameExit { get => instance.gameExit; set => instance.gameExit = value; }

        public static GamePadState PreviousGamePadState { get => instance.previousGamePadState; }
        public static GamePadState CurrentGamePadState { get => instance.currentGamePadState; }
        public static KeyboardState PreviousKeyboardState { get => instance.previousKeyboardState; }
        public static KeyboardState CurrentKeyboardState { get => instance.currentKeyboardState; }
        public static MouseState PreviousMouseState { get => instance.previousMouseState; }
        public static MouseState CurrentMouseState { get => instance.currentMouseState; }
        public List<String> SpawnedWeapons { get => instance.spawnedWeapons; }

        #endregion

        #region constructors
        // ************* Constructor ************* //
        private GameManager()
        {
            rng = new Random();
            gameState = GameState.Menu;
        }

        public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice)
        {
            if (instance == null)
            {
                instance = new GameManager();
                DisplayManager.Initialize(content, graphicsDevice);
                EntityManager.Initialize();
                FloorManager.Initialize();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        #endregion

        #region methods
        // ************* Methods ************* //

        public void Update(GameTime gameTime)
        {
            currentMouseState = Mouse.GetState();
            currentGamePadState = GamePad.GetState(0);
            currentKeyboardState = Keyboard.GetState();

            switch (gameState)
            {
                case GameState.Game:
                    if ((currentKeyboardState.IsKeyDown(Keys.Escape) && !previousKeyboardState.IsKeyDown(Keys.Escape)) || (currentGamePadState.IsButtonDown(Buttons.Start) && !previousGamePadState.IsButtonDown(Buttons.Start)))
                        gameState = GameState.PauseMenu;
                    if ((currentKeyboardState.IsKeyDown(Keys.Tab)) || (currentGamePadState.IsButtonDown(Buttons.Back) && !previousGamePadState.IsButtonDown(Buttons.Back)))
                        gameState = GameState.Map;
                    if ((EntityManager.Player.CurrHP <= 0))
                        gameState = GameState.GameOver;
                    EntityManager.Instance.Update();
                    FloorManager.Instance.Update();
                    break;
                case GameState.Menu:
                    // Moving the pointer
                    if (DisplayManager.MenuPointer < DisplayManager.MenuButtons.Count - 1 && ((currentKeyboardState.IsKeyDown(Keys.Down) && !previousKeyboardState.IsKeyDown(Keys.Down)) || (currentGamePadState.ThumbSticks.Left.Y < 0 && previousGamePadState.ThumbSticks.Left.Y >= 0)))
                    {
                        DisplayManager.MenuPointer += 1;
                    }
                    else if (DisplayManager.MenuPointer > 0 && ((currentKeyboardState.IsKeyDown(Keys.Up) && !previousKeyboardState.IsKeyDown(Keys.Up)) || (currentGamePadState.ThumbSticks.Left.Y > 0 && previousGamePadState.ThumbSticks.Left.Y <= 0)))
                    {
                        DisplayManager.MenuPointer -= 1;
                    }

                    for (int i = 0; i < DisplayManager.MenuButtons.Count; i++)
                    {
                        // Checking which button the pointer is on
                        if (DisplayManager.MenuPointer == i || DisplayManager.MenuButtons[i].Rect.Contains(currentMouseState.Position))
                        {
                            DisplayManager.MenuPointer = i;
                            DisplayManager.MenuButtons[i].IsHighlighted = true;
                        }
                        else
                        {
                            DisplayManager.MenuButtons[i].IsHighlighted = false;
                        }

                        // If pointer is on button and Enter/Left mouse button is pressed, do whatever the button is supposed to do
                        if (((currentMouseState.LeftButton.Equals(ButtonState.Released) && previousMouseState.LeftButton.Equals(ButtonState.Pressed)) || ((!previousKeyboardState.IsKeyDown(Keys.Enter) && currentKeyboardState.IsKeyDown(Keys.Enter)) || (previousGamePadState.Buttons.A == ButtonState.Released && currentGamePadState.Buttons.A == ButtonState.Pressed) && DisplayManager.MenuButtons[i].IsHighlighted)))
                        {
                            if (DisplayManager.MenuPointer == DisplayManager.MenuButtons.Count - 1)
                            {
                                Exit();
                            }
                            else if (DisplayManager.MenuPointer == 1)
                            {
                                gameState = GameState.Help;
                            }
                            else
                            {
                                EntityManager.Instance.ClearEntities();
                                spawnedWeapons = new List<String>();
                                EntityManager.Instance.ResetPlayer();
                                FloorManager.Instance.FloorNumber = 1;
                                FloorManager.Instance.GenerateFloor();
                                gameState = GameState.Game;
                            }
                        }
                    }
                    break;
                case GameState.Help:
                    if ((currentKeyboardState.IsKeyDown(Keys.Enter) && !previousKeyboardState.IsKeyDown(Keys.Enter)) || (currentKeyboardState.IsKeyDown(Keys.Escape) && !previousKeyboardState.IsKeyDown(Keys.Escape)) || (currentGamePadState.IsButtonDown(Buttons.Y) && !previousGamePadState.IsButtonDown(Buttons.Y)) || (currentGamePadState.IsButtonDown(Buttons.Start) && !previousGamePadState.IsButtonDown(Buttons.Start)))
                        gameState = GameState.Menu;
                    break;
                case GameState.PauseMenu:
                    if ((currentKeyboardState.IsKeyDown(Keys.Escape) && !previousKeyboardState.IsKeyDown(Keys.Escape)) || (currentGamePadState.IsButtonDown(Buttons.Start) && !previousGamePadState.IsButtonDown(Buttons.Start)))
                        gameState = GameState.Game;

                    if (DisplayManager.PausePointer < DisplayManager.PauseButtons.Count - 1 && ((currentKeyboardState.IsKeyDown(Keys.Down) && !previousKeyboardState.IsKeyDown(Keys.Down)) || (currentGamePadState.ThumbSticks.Left.Y < 0 && previousGamePadState.ThumbSticks.Left.Y >= 0)))
                    {
                        DisplayManager.PausePointer += 1;
                    }
                    else if (DisplayManager.PausePointer > 0 && ((currentKeyboardState.IsKeyDown(Keys.Up) && !previousKeyboardState.IsKeyDown(Keys.Up)) || (currentGamePadState.ThumbSticks.Left.Y > 0 && previousGamePadState.ThumbSticks.Left.Y <= 0)))
                    {
                        DisplayManager.PausePointer -= 1;
                    }

                    for (int i = 0; i < DisplayManager.PauseButtons.Count; i++)
                    {
                        // Checking which button the pointer is on
                        if (DisplayManager.PausePointer == i || DisplayManager.PauseButtons[i].Rect.Contains(currentMouseState.Position))
                        {
                            DisplayManager.PausePointer = i;
                            DisplayManager.PauseButtons[i].IsHighlighted = true;
                        }
                        else
                        {
                            DisplayManager.PauseButtons[i].IsHighlighted = false;
                        }

                        // If pointer is on button and Enter/Left mouse button is pressed, do whatever the button is supposed to do
                        if (((currentMouseState.LeftButton.Equals(ButtonState.Released) && previousMouseState.LeftButton.Equals(ButtonState.Pressed)) || ((!previousKeyboardState.IsKeyDown(Keys.Enter) && currentKeyboardState.IsKeyDown(Keys.Enter) || (previousGamePadState.Buttons.A == ButtonState.Released && currentGamePadState.Buttons.A == ButtonState.Pressed)) && DisplayManager.PauseButtons[i].IsHighlighted)))
                        {
                            if (DisplayManager.PausePointer == 0)
                            {
                                gameState = GameState.Game;
                            }
                            else if(DisplayManager.PausePointer == 1)
                            {
                                gameState = GameState.ConfirmationMenu;
                            }
                            else if(DisplayManager.PausePointer == 2)
                            {
                                gameState = GameState.Menu;
                            }
                            else if (DisplayManager.PausePointer == DisplayManager.PauseButtons.Count - 1)
                            {
                                Exit();
                            }
                        }
                    }
                    break;
                case GameState.ConfirmationMenu:
                    if (DisplayManager.ConfirmationPointer < DisplayManager.ConfirmationButtons.Count - 1 && ((currentKeyboardState.IsKeyDown(Keys.Down) && !previousKeyboardState.IsKeyDown(Keys.Down)) || (currentGamePadState.ThumbSticks.Left.Y < 0 && previousGamePadState.ThumbSticks.Left.Y >= 0)))
                    {
                        DisplayManager.ConfirmationPointer += 1;
                    }
                    else if (DisplayManager.ConfirmationPointer > 0 && ((currentKeyboardState.IsKeyDown(Keys.Up) && !previousKeyboardState.IsKeyDown(Keys.Up)) || (currentGamePadState.ThumbSticks.Left.Y > 0 && previousGamePadState.ThumbSticks.Left.Y <= 0)))
                    {
                        DisplayManager.ConfirmationPointer -= 1;
                    }

                    for (int i = 0; i < DisplayManager.ConfirmationButtons.Count; i++)
                    {
                        if (DisplayManager.ConfirmationPointer == i || DisplayManager.ConfirmationButtons[i].Rect.Contains(currentMouseState.Position))
                        {
                            DisplayManager.ConfirmationPointer = i;
                            DisplayManager.ConfirmationButtons[i].IsHighlighted = true;
                        }
                        else
                        {
                            DisplayManager.ConfirmationButtons[i].IsHighlighted = false;
                        }

                        if (((currentMouseState.LeftButton.Equals(ButtonState.Released) && previousMouseState.LeftButton.Equals(ButtonState.Pressed)) || ((!previousKeyboardState.IsKeyDown(Keys.Enter) && currentKeyboardState.IsKeyDown(Keys.Enter) || (previousGamePadState.Buttons.A == ButtonState.Released && currentGamePadState.Buttons.A == ButtonState.Pressed)) && DisplayManager.PauseButtons[i].IsHighlighted)))
                        {
                            if(DisplayManager.ConfirmationPointer == 0) // Yes
                            {
                                if (DisplayManager.PausePointer == 1) // Restart
                                {
                                    EntityManager.Instance.ClearEntities();
                                    EntityManager.Instance.ResetPlayer();
                                    FloorManager.Instance.FloorNumber = 1;
                                    FloorManager.Instance.GenerateFloor();

                                    gameState = GameState.Game;
                                }
                            }
                            else
                            {
                                gameState = GameState.PauseMenu;
                            }
                        }
                    }
                    break;
                case GameState.Map:
                    if ((!currentKeyboardState.IsKeyDown(Keys.Tab) && previousKeyboardState.IsKeyDown(Keys.Tab)) || (currentGamePadState.IsButtonDown(Buttons.Back) && !previousGamePadState.IsButtonDown(Buttons.Back)))
                        gameState = GameState.Game;
                    break;
                case GameState.GameOver:
                    if (currentKeyboardState.IsKeyDown(Keys.Enter))
                        gameState = GameState.Menu;
                    break;
            }

            previousKeyboardState = currentKeyboardState;
            previousGamePadState = currentGamePadState;
            previousMouseState = currentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DisplayManager.Instance.Draw(spriteBatch);
        }

        /// <summary>
        /// Helper method to allow for easier exit syntax
        /// </summary>
        public void Exit()
        {
            gameExit = true;
        }
        #endregion
    }
}
