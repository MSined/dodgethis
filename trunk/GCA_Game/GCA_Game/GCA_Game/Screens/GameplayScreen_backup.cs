using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GCA_Game
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    /// 

    /*
     * BUGS
     * - Only move state comes up, not touch. This brings issues with the 'Switch' button
     * - If mouse held down on destination, sprite vibrates
     */

    public enum GameState
    {
        Control,
        Switch,
    }

    class GameplayScreen : GameScreen
    {
        ContentManager content;
        //SpriteFont gameFont;

        const int MAX_SPRITES = 3;
        const int cloudCount = 7;

        List<SceneSprite> ninjaList = new List<SceneSprite>(MAX_SPRITES);
        List<SceneSprite> cloudList = new List<SceneSprite>(cloudCount);
        //SceneSprite ninja;
        //SceneSprite ninja2;
        //SceneSprite ninja3;

        SceneSprite switchButton;
        //Texture2D ninja;
        SceneSprite currentSelection;
        SceneSprite ball;

        Vector2 unitVector;
        Vector2 velocity;

        Texture2D buttonOn;
        Texture2D buttonOff;
        Texture2D backgroundTexture;

        GameState state = GameState.Control;

        SpriteBatch spriteBatch;
        Camera camera;

        private float windowWidth;
        private float windowHeight;
        
        bool[] direction;

        /*
        Vector2 ninjaPosition = new Vector2(100, 100);
        Vector2 ninjaDirection = new Vector2(0, 0);
        Vector2 ninjaDestination = new Vector2(100, 100);
         * */
        //Vector2 enemyPosition = new Vector2(100, 100);

        Random random = new Random((int)DateTime.Now.Ticks);

        const int movementSpeed = 5;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            //this.EnabledGestures = GestureType.Tap;
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            //gameFont = content.Load<SpriteFont>("gamefont");

            windowWidth = ScreenManager.GraphicsDevice.Viewport.Width;
            windowHeight = ScreenManager.GraphicsDevice.Viewport.Height;

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            spriteBatch = ScreenManager.SpriteBatch;
            camera = new Camera(spriteBatch, new Rectangle(0, 0, (int) windowWidth,(int) windowHeight));

            backgroundTexture = content.Load<Texture2D>(@"Textures/bg");

            direction = new bool[cloudCount];
            for (int i = 0; i < cloudCount; i += 2)
            {
                direction[i] = true;
            }

            //Initialize ninjas
            for (int i = 0; i < MAX_SPRITES; i++)
            {
                ninjaList.Add(new SceneSprite(content.Load<Texture2D>(@"Sprites/ninja"), new Vector2((i + 1) * 200, backgroundTexture.Bounds.Height - ((i + 1) * 100)), SpriteType.Ninja, true));
                Thread.Sleep(200);
            }

            //Initialize cloud
            Random randNums = new Random();
            for (int i = 0; i < cloudCount; i++)
            {
                if (!direction[i])
                    cloudList.Add(new SceneSprite(content.Load<Texture2D>(@"Sprites/cloud2"), new Vector2(randNums.Next(1530), (randNums.Next(960) / 6) + 75), SpriteType.Cloud, false));
                else
                    cloudList.Add(new SceneSprite(content.Load<Texture2D>(@"Sprites/cloud"), new Vector2(randNums.Next(1530), (randNums.Next(960) / 6) + 75), SpriteType.Cloud, false));
                //Thread.Sleep(200);
            }

            ball = new SceneSprite(content.Load<Texture2D>(@"Sprites/ball"), new Vector2(1530/2, 960/2), SpriteType.Ball, false);

            

            //ninja = new SceneSprite(content.Load<Texture2D>(@"Sprites/ninja"), new Vector2(100, 100), SpriteType.Ninja);
            //ninja2 = new SceneSprite(content.Load<Texture2D>(@"Sprites/ninja"), new Vector2(300, 200), SpriteType.Ninja);
            //ninja3 = new SceneSprite(content.Load<Texture2D>(@"Sprites/ninja"), new Vector2(500, 300), SpriteType.Ninja);

            buttonOn = content.Load<Texture2D>(@"Sprites/switchon");
            buttonOff = content.Load<Texture2D>(@"Sprites/switchoff");

            switchButton = new SceneSprite(buttonOff, new Vector2(700, 440), SpriteType.Button, false);

            currentSelection = ninjaList[0];// new SceneSprite(content.Load<Texture2D>(@"Sprites/ninja"), new Vector2(100, 100)); ;
            currentSelection.AIstate = false;

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                //ninjaPosition += new Vector2(5f, 5f);

                // Apply some random jitter to make the enemy move around.
               // const float randomization = 10;

               // enemyPosition.X += (float)(random.NextDouble() - 0.5) * randomization;
               // enemyPosition.Y += (float)(random.NextDouble() - 0.5) * randomization;

                // Apply a stabilizing force to stop the enemy moving off the screen.
                /*Vector2 targetPosition = new Vector2(
                    ScreenManager.GraphicsDevice.Viewport.Width / 2 - gameFont.MeasureString("Insert Gameplay Here").X / 2, 
                    200);*/

                //enemyPosition = Vector2.Lerp(enemyPosition, targetPosition, 0.05f);

                // TODO: this game isn't very fun! You could probably improve
                // it by inserting something more interesting in this space :-)

                for (int i = 0; i < MAX_SPRITES; i++)
                {
                    ninjaList[i].MoveSprite();
                    ninjaList[i].SpriteAI();
                }

                camera.Position = currentSelection.Position - new Vector2(windowWidth / 2, windowHeight / 2); //Reposition camera
                switchButton.Position = currentSelection.Position + new Vector2(300, 200); //Hard coded button placement =[

                Random randNums = new Random();

                for (int i = 0; i < cloudCount; i++)
                {
                    if (direction[i])
                    {
                        cloudList[i].Position += new Vector2(.5f, 0);
                    }
                    else
                    {
                        cloudList[i].Position -= new Vector2(.5f, 0);
                    }

                    if (cloudList[i].Position.X < -200)
                    {
                        cloudList[i].Position = new Vector2(1730, (randNums.Next(960) / 6) + 75);
                    }
                    else if (cloudList[i].Position.X > 1730)
                    {
                        cloudList[i].Position = new Vector2(-200, (randNums.Next(960) / 6) + 75);
                    }
                }

                /*
                velocity += new Vector2(0f, (float)(0.098 * 5));
                // ball moving
                if (ball.Position.Y > 400)
                {
                    velocity.Y = -velocity.Y * 0.8f;
                }
                //else
                //{
                //    velocity.Y = velocity.Y * -0.8f;
                //}
                if (velocity.Y < 0 && velocity.Y > -2)
                {
                    velocity.Y = 0;
                }
                
                ball.Position += velocity;*/
                //ball.Position += unitVector;
                //unitVector.Y = unitVector.Y * 0.9f;
                //unitVector.X = unitVector.X * 0.975f;

                //Debug info
                System.Diagnostics.Debug.WriteLine("Cam: " + camera.Position);
                System.Diagnostics.Debug.WriteLine("Sprite: " + currentSelection.Position);
                System.Diagnostics.Debug.WriteLine("Switch: " + switchButton.Position);
            }

        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            /*
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // if the user pressed the back button, we return to the main menu
            PlayerIndex player;
            if (input.IsNewButtonPress(Buttons.Back, ControllingPlayer, out player))
            {
                LoadingScreen.Load(ScreenManager, false, ControllingPlayer, new BackgroundScreen(), new MainMenuScreen());
            }
            else
            {
                // Otherwise move the player position.
                Vector2 movement = Vector2.Zero;

                if (keyboardState.IsKeyDown(Keys.Left))
                    movement.X--;

                if (keyboardState.IsKeyDown(Keys.Right))
                    movement.X++;

                if (keyboardState.IsKeyDown(Keys.Up))
                    movement.Y--;

                if (keyboardState.IsKeyDown(Keys.Down))
                    movement.Y++;

                Vector2 thumbstick = gamePadState.ThumbSticks.Left;

                movement.X += thumbstick.X;
                movement.Y -= thumbstick.Y;

                if (movement.Length() > 1)
                    movement.Normalize();

                ninjaPosition += movement * 2;
            }
             * */

            // Process touch events

            foreach (GestureSample gs in input.Gestures) //Read TouchPanel Gestures (Tap, Doubletap, Flick, etc)
            {
                switch (gs.GestureType)
                {
                    case GestureType.Tap:
                        #region Screen Tapped
                        //Touch screen has been tapped

                        //Using xPos and yPos to get a directional vector, since gs.Position is relative to the screen itself
                        float xPos = gs.Position.X;
                        float yPos = gs.Position.Y;

                        xPos = xPos - (windowWidth / 2);
                        yPos = yPos - (windowHeight / 2);

                        //Check if 'Switch' button was pressed
                        if (switchButton.Bounds.Intersects(new Rectangle((int)(currentSelection.Position.X + xPos + (switchButton.Bounds.Width / 2)),
                                                                         (int)(currentSelection.Position.Y + yPos + (switchButton.Bounds.Height / 2)),
                                                                         1,
                                                                         1)))
                        {
                            //Toggle the state button
                            if (state == GameState.Control)
                            {
                                state = GameState.Switch;
                                switchButton.Texture = buttonOn;
                            }
                            else
                            {
                                state = GameState.Control;
                                switchButton.Texture = buttonOff;
                            }
                        }
                        else if (state == GameState.Switch)
                        {
                            //Game state in Switch mode
                            //Another sprite is being selected

                            //Loop through sprites list
                            for (int i = 0; i < MAX_SPRITES; i++)
                            {
                                if (ninjaList[i].Bounds.Intersects(new Rectangle((int)(currentSelection.Position.X + xPos + (ninjaList[i].Bounds.Width / 2)),
                                                                                 (int)(currentSelection.Position.Y + yPos + (ninjaList[i].Bounds.Height / 2)),
                                                                                 1,
                                                                                 1)))
                                {
                                    //Set the new current selection
                                    currentSelection.AIstate = true;
                                    currentSelection = ninjaList[i];
                                    currentSelection.AIstate = false;
                                    break;
                                }
                            }

                            //Switch the state back
                            state = GameState.Control;
                            switchButton.Texture = buttonOff;
                        }
                        break;
                    default:
                        break;
                        #endregion
                    case GestureType.Flick:

                        //soundEffect.Play();

                        //This CRAZY line of code gets the direction of the flick
                        unitVector = new Vector2((float)(gs.Delta.X / (System.Math.Sqrt(Math.Pow(gs.Delta.X, 2) + Math.Pow(gs.Delta.Y, 2)))), (float)(gs.Delta.Y / (System.Math.Sqrt(Math.Pow(gs.Delta.X, 2) + Math.Pow(gs.Delta.Y, 2)))));

                        if (unitVector.X < 0)
                            currentSelection.Spin();

                        //unitVector.X = unitVector.X * 30;
                        //unitVector.Y = unitVector.Y * 30;


                        break;
                } //switch (gs.GestureType)
            } //foreach (GestureSample gs in input.Gestures)

            //Read TouchPanel States (Pressed, Released, Etc)
            
            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation tl in touchCollection)
            {
                //Using xPos and yPos to get a directional vector, since gs.Position is relative to the screen itself
                float xPos = tl.Position.X;
                float yPos = tl.Position.Y;

                xPos = xPos - (windowWidth / 2);
                yPos = yPos - (windowHeight / 2);

                //Check if switch was tapped (Tap is a press)
                bool switchTapped = switchButton.Bounds.Intersects(new Rectangle((int)(currentSelection.Position.X + xPos + (switchButton.Bounds.Width / 2)),
                                                                                 (int)(currentSelection.Position.Y + yPos + (switchButton.Bounds.Height / 2)),
                                                                                 1,
                                                                                 1));

                if (((tl.State == TouchLocationState.Pressed)
                        || (tl.State == TouchLocationState.Moved)) && !switchTapped)
                {
                    if (state == GameState.Control)
                    {
                        //Game state in Control mode
                        //Sprite is being moved
                        //Set the sprite's destination
                        currentSelection.Destination = new Vector2(currentSelection.Position.X + xPos, currentSelection.Position.Y + yPos);
                        currentSelection.Direction = currentSelection.Destination - currentSelection.Position;
                    } //if (state == GameState.Control)
                }//if ((tl.State == TouchLocationState.Pressed) || (tl.State == TouchLocationState.Moved))
            }//foreach (TouchLocation tl in touchCollection)

            /*
             * 
             * */
            //if (gs.Delta.X > mySprite1.position.X && gs.Delta.X < mySprite1.position.X + 5.0f && gs.Delta.Y > mySprite1.position.Y && gs.Delta.Y < mySprite1.position.Y + 5.0f)
                            //{
                            //    selected = true;
                            //}
                            //else if (selected == true) 
                            //{
                            //    mySprite1.position = new Vector2(gs.Delta.X, gs.Delta.Y);
                            //    selected = false;
                            //}

                    //        mySprite1.position = new Vector2(gs.Delta.X, gs.Delta.Y);

                    //    }
                    //    break;
                    //case GestureType.VerticalDrag:
                    //    mySprite1.position += new Vector2(0, gs.Delta.Y);
                    //    break;

                    //case GestureType.HorizontalDrag:
                    //    mySprite1.position += new Vector2(gs.Delta.X, 0);
                    //    break;

                        /*
                    case GestureType.FreeDrag:
                        {
                            //mySprite1.position = new Vector2(gs.Position.X, gs.Position.Y) - new Vector2(mySprite1.size.X / 2, mySprite1.size.Y / 2);
                            //fired = true;

                            mySprite3.position += new Vector2(gs.Delta.X, gs.Delta.Y);
                        }
                        break;*/

                    //Touch screen has been pressed
                    
                    ////Check if 'Switch' button was pressed
                    //if (switchButton.Bounds.Intersects(new Rectangle((int)tl.Position.X, (int)tl.Position.Y, 1, 1)))
                    //{
                    //    //Toggle the state button
                    //    if (state == GameState.Control)
                    //    {
                    //        state = GameState.Switch;
                    //        switchButton.Texture = buttonOn;
                    //    }
                    //    else
                    //    {
                    //        state = GameState.Control;
                    //        switchButton.Texture = buttonOff;
                    //    }
                    //}
                    //else //Game world was pressed
                    //{
                        
                        //else
                        //{
                        //    //Game state in Switch mode
                        //    //Another sprite is being selected

                        //    //Loop through sprites list
                        //    for (int i = 0; i < MAX_SPRITES; i++)
                        //    {
                        //        if (ninjaList[i].Bounds.Intersects(new Rectangle((int)tl.Position.X, (int)tl.Position.Y, 1, 1)))
                        //        {
                        //            //Set the new current selection
                        //            currentSelection.AIstate = true;
                        //            currentSelection = ninjaList[i];
                        //            currentSelection.AIstate = false;
                        //            break;
                        //        }
                        //    }

                        //    //Switch the state back
                        //    state = GameState.Control;
                        //    switchButton.Texture = buttonOff;
                        //}
                    //}
                //}
            //}

        }//public override void HandleInput(InputState input)

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            // Our player and enemy are both actually just text strings.
            //SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.viewMatrix);

            //Draw background
            spriteBatch.Draw(backgroundTexture, backgroundTexture.Bounds, Color.White);

            //spriteBatch.Draw(ninja, ninjaPosition, Color.White);

            //Draw units
            foreach (SceneSprite ninja in ninjaList)
                camera.DrawSprite(ninja);

            //Draw clouds
            foreach (SceneSprite cloud in cloudList)
                camera.DrawSprite(cloud);

            camera.DrawSprite(ball);

            /*
            for (int i = 0; i < MAX_SPRITES; i++)
                ninjaList[i].Draw(spriteBatch, ninjaList[i].Position);
            */
            /*
            ninja.Draw(spriteBatch, ninja.Position);
            ninja2.Draw(spriteBatch, ninja2.Position);
            ninja3.Draw(spriteBatch, ninja3.Position);
             * */

            //Draw button
            camera.DrawSprite(switchButton);
            /*
            spriteBatch.DrawString(gameFont, "// TODO", playerPosition, Color.Green);

            spriteBatch.DrawString(gameFont, "Insert Gameplay Here",
                                   enemyPosition, Color.DarkRed);
            */
            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }
    }
}
