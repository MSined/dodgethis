using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
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
        SpriteFont gameFont;

        const int MAX_SPRITES = 3;
        const int cloudCount = 7;

        List<SceneSprite> ninjaList = new List<SceneSprite>(MAX_SPRITES);
        List<SceneSprite> pirateList = new List<SceneSprite>(MAX_SPRITES);
        List<SceneSprite> cloudList = new List<SceneSprite>(cloudCount);
        List<SceneSprite> arrowList;

        SceneSprite switchButton;
        SceneSprite currentSelection;
        SceneSprite ball;
        SceneSprite cursor;

//        Vector2 unitVector;
//        Vector2 velocity;

        Texture2D buttonOn;
        Texture2D buttonOff;
        Texture2D backgroundTexture;

        GameState state = GameState.Control;

        SpriteBatch spriteBatch;
        Camera camera;

        SoundEffect wooshSoundEffect;
        SoundEffect hitSoundEffect;
        SoundEffect catchSoundEffect;

        private float windowWidth;
        private float windowHeight;
        private float leftBounds = 410;
        private float rightBounds = 1645;
        private float topBounds = 345;
        private float bottomBounds = 790;
        private float leftMiddleBounds = 985;
        private float rightMiddleBounds = 1045;

        private SpriteType lastHolder = SpriteType.Ball;
        private SceneSprite holder;
        private Vector2 throwBall = new Vector2(80, 0);
        private Vector2 toHandX = new Vector2(15, 0);
        private Vector2 toHandY = new Vector2(0, 25);

        bool[] direction;

        private const int MAX_GAME_TIME = 120; //In seconds
        private int gametime;

        private Vector2 HUD_spinsLeftPos;
        private Vector2 HUD_enemyCountPos;
        private Vector2 HUD_timeTextPos;
        private Vector2 HUD_timeLeftPos;

        private int enemyCount;

        private const int FPS = 30; //30 by default on windows phone

        Random random = new Random((int)DateTime.Now.Ticks);

        //const int movementSpeed = 5;

        SoundEffect woosh;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            windowWidth = ScreenManager.GraphicsDevice.Viewport.Width;
            windowHeight = ScreenManager.GraphicsDevice.Viewport.Height;

            spriteBatch = ScreenManager.SpriteBatch;
            camera = new Camera(spriteBatch, new Rectangle(0, 0, (int)windowWidth, (int)windowHeight));

            backgroundTexture = content.Load<Texture2D>(@"textures/back");
            gameFont = content.Load<SpriteFont>(@"Fonts/gameFont");

            woosh = content.Load<SoundEffect>(@"Sounds/woosh");

            HUD_spinsLeftPos = new Vector2(0f, 0f);
            HUD_timeTextPos = new Vector2(0f, 0f);
            HUD_enemyCountPos = new Vector2(0f, 0f);
            HUD_timeLeftPos = new Vector2(0f, 0f);

            direction = new bool[cloudCount];
            for (int i = 0; i < cloudCount; i += 2)
            {
                direction[i] = true;
            }

            //Initialize units
            for (int i = 0; i < MAX_SPRITES; i++)
            {
                pirateList.Add(new SceneSprite(content.Load<Texture2D>(@"Sprites/pirate"), new Vector2(random.Next(420, 965), random.Next(365, 770)), SpriteType.Pirate, true));
                Thread.Sleep(200);
                ninjaList.Add(new SceneSprite(content.Load<Texture2D>(@"Sprites/ninja"), new Vector2(random.Next(1065, 1625), random.Next(365, 770)), SpriteType.Ninja, true));
            }

            currentSelection = pirateList[0];
            currentSelection.AIstate = false;

            //Initialize cloud
            for (int i = 0; i < cloudCount; i++)
            {
                if (!direction[i])
                    cloudList.Add(new SceneSprite(content.Load<Texture2D>(@"Sprites/cloud2"), new Vector2(random.Next(1530), (random.Next(960) / 6) + 75), SpriteType.Cloud, false));
                else
                    cloudList.Add(new SceneSprite(content.Load<Texture2D>(@"Sprites/cloud"), new Vector2(random.Next(1530), (random.Next(960) / 6) + 75), SpriteType.Cloud, false));
            }

            //Initialize arrows

            if (currentSelection.Type == SpriteType.Ninja)
            {
                for (int i = 0; i < MAX_SPRITES; i++)
                {
                    if (pirateList[i].IsAlive())
                        enemyCount++;
                }
            }
            else
            {
                for (int i = 0; i < MAX_SPRITES; i++)
                {
                    if (ninjaList[i].IsAlive())
                        enemyCount++;
                }
            }

            arrowList = new List<SceneSprite>(enemyCount);

            for (int i = 0; i < enemyCount; i++)
            {
                arrowList.Add(new SceneSprite(content.Load<Texture2D>(@"Sprites/nArrow"), new Vector2(random.Next(1530), (random.Next(960) / 6) + 75), SpriteType.Arrow, false));
            }

            ball = new SceneSprite(content.Load<Texture2D>(@"Sprites/ball"), new Vector2(1530 / 2, 960 / 2), SpriteType.Ball, false);
            cursor = new SceneSprite(content.Load<Texture2D>(@"Sprites/playerArrow"), new Vector2(1530 / 2, 960 / 2), SpriteType.Cursor, false);

            wooshSoundEffect = content.Load<SoundEffect>(@"Sounds/woosh");
            hitSoundEffect = content.Load<SoundEffect>(@"Sounds/hit");
            catchSoundEffect = content.Load<SoundEffect>(@"Sounds/catch");

            ball.Ball_UnitVector = new Vector2(0, 20);
            holder = ball;

            buttonOn = content.Load<Texture2D>(@"Sprites/switchon");
            buttonOff = content.Load<Texture2D>(@"Sprites/switchoff");

            switchButton = new SceneSprite(buttonOff, new Vector2(700, 440), SpriteType.Button, false);

            gametime = MAX_GAME_TIME * FPS;

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
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                foreach (SceneSprite ns in ninjaList)
                {
                    ns.MoveSprite(rightMiddleBounds, rightBounds, topBounds, bottomBounds);
                    ns.SpriteAI(rightMiddleBounds, rightBounds, topBounds, bottomBounds);
                }

                foreach (SceneSprite ps in pirateList)
                {
                    ps.MoveSprite(leftBounds, leftMiddleBounds, topBounds, bottomBounds);
                    ps.SpriteAI(leftBounds, leftMiddleBounds, topBounds, bottomBounds);
                }

                if (ball.getHitSoundPlay())
                {
                    hitSoundEffect.Play();
                    ball.setHitSoundPlay(false);
                }

                ball.MoveSprite(leftBounds, rightBounds, topBounds, bottomBounds + (currentSelection.Texture.Bounds.Height / 2) - (ball.Texture.Bounds.Height / 2));

                camera.Position = currentSelection.Position - new Vector2(windowWidth / 2, windowHeight / 2);

                switchButton.Position = currentSelection.Position + new Vector2(300, 200); //Hard coded button placements =[
                HUD_spinsLeftPos = currentSelection.Position + new Vector2(-320, 210);
                HUD_timeTextPos = currentSelection.Position + new Vector2(-0, -205);
                HUD_enemyCountPos = currentSelection.Position + new Vector2(-300, -205);
                HUD_timeLeftPos = currentSelection.Position + new Vector2(-0, -175);

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
                        cloudList[i].Position = new Vector2(1730, (random.Next(960) / 6) + 75);
                    }
                    else if (cloudList[i].Position.X > 1730)
                    {
                        cloudList[i].Position = new Vector2(-200, (random.Next(960) / 6) + 75);
                    }
                }

                cursor.Position = camera.Position + new Vector2(400, 150);
                cursor.Position += new Vector2(0f, (float)(Math.Sin(10 * gameTime.TotalGameTime.TotalSeconds) * 5));

                foreach (SceneSprite ss in pirateList)
                {
                    if (ball.collideWith(ss) && holder.Type != SpriteType.Pirate)
                    {
                        //code for if collide
                        if (ball.Ball_UnitVector.Length() > 10 && lastHolder == SpriteType.Ninja)
                        { //hard hit

                        }
                        else
                        { //soft hit or pass
                            holder = ss;
                            lastHolder = ss.Type;
                        }
                    }
                }
                foreach (SceneSprite ss in ninjaList)
                {
                    if (ball.collideWith(ss) && holder.Type != SpriteType.Ninja)
                    {
                        if (ball.Ball_UnitVector.Length() > 10 && lastHolder == SpriteType.Pirate)
                        { //hard hit

                        }
                        else
                        { //soft hit or pass
                            holder = ss;
                            lastHolder = ss.Type;
                        }
                    }
                }
                if (holder.Type != SpriteType.Ball)
                {
                    if (holder.Direction.X < 0)
                    {
                        ball.Position = holder.Position + toHandX + toHandY;
                    }
                    else
                    {
                        ball.Position = holder.Position - toHandX + toHandY;
                    }
                }

                for (int i = 0; i < enemyCount; i++)
                {
                    if (camera.PositionX + 800 < ninjaList[i].Position.X)
                    {
                        arrowList[i].Position = new Vector2(camera.PositionX + 750, pirateList[i].Position.Y);
                    }
                    else
                        arrowList[i].Position = new Vector2(100000f, 0);
                }

                //velocity += new Vector2(0f, (float)(0.098 * 5));
                //// ball moving
                //if (ball.Position.Y > 400)
                //{
                //    velocity.Y = -velocity.Y * 0.8f;
                //}

                //if (velocity.Y < 0 && velocity.Y > -2)
                //{
                //    velocity.Y = 0;
                //}
                //ball.Position += velocity;
                //ball.Position += unitVector;
                //unitVector.Y = unitVector.Y * 0.9f;
                //unitVector.X = unitVector.X * 0.975f;

                gametime--;
                System.Diagnostics.Debug.WriteLine("Cursor: " + cursor.Position);
            }

        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
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
                            if (currentSelection.Type == SpriteType.Pirate)
                            {
                                //Loop through sprites list
                                for (int i = 0; i < MAX_SPRITES; i++)
                                {
                                    if (pirateList[i].Bounds.Intersects(new Rectangle((int)(currentSelection.Position.X + xPos + (pirateList[i].Bounds.Width / 2)),
                                                                                     (int)(currentSelection.Position.Y + yPos + (pirateList[i].Bounds.Height / 2)),
                                                                                     1,
                                                                                     1)))
                                    {
                                        //Set the new current selection
                                        currentSelection.AIstate = true;
                                        currentSelection = pirateList[i];
                                        currentSelection.AIstate = false;
                                        break;
                                    }
                                }
                            }
                            else if (currentSelection.Type == SpriteType.Ninja)
                            {
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
                        woosh.Play();
                        //if (holder == currentSelection)
                        //{
                        //    ball.Ball_UnitVector = new Vector2((float)(gs.Delta.X / (System.Math.Sqrt(Math.Pow(gs.Delta.X, 2) + Math.Pow(gs.Delta.Y, 2)))), (float)(gs.Delta.Y / (System.Math.Sqrt(Math.Pow(gs.Delta.X, 2) + Math.Pow(gs.Delta.Y, 2)))));
                        //    ball.Ball_UnitVector *= 30;
                        //    holder = ball;
                        //}

                        //if (ball.Ball_UnitVector.X < 0)
                        //    currentSelection.Spin();

                        if (ball.Ball_UnitVector.X < 5 && holder != currentSelection)
                            currentSelection.Spin();

                        //soundEffect.Play();
                        if (holder == currentSelection)
                        {
                            if (holder.Direction.X < 0)
                            {
                                ball.Position -= throwBall;
                            }
                            else
                            {
                                ball.Position += throwBall;
                            }
                            ball.Ball_UnitVector = new Vector2((float)(gs.Delta.X / (System.Math.Sqrt(Math.Pow(gs.Delta.X, 2) + Math.Pow(gs.Delta.Y, 2)))), (float)(gs.Delta.Y / (System.Math.Sqrt(Math.Pow(gs.Delta.X, 2) + Math.Pow(gs.Delta.Y, 2)))));
                            ball.Ball_UnitVector *= 30;
                            holder = ball;
                        }

                        break;
                }
            }

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
                    }
                }
            }
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, camera.viewMatrix);

            //Draw background
            spriteBatch.Draw(backgroundTexture, backgroundTexture.Bounds, Color.White);

            //Draw units
            foreach (SceneSprite ninja in ninjaList)
                camera.DrawSprite(ninja);
            foreach (SceneSprite pirate in pirateList)
                camera.DrawSprite(pirate);

            //Draw clouds
            foreach (SceneSprite cloud in cloudList)
                camera.DrawSprite(cloud);

            //Draw arrows
            foreach (SceneSprite arrow in arrowList)
                camera.DrawSprite(arrow);

            camera.DrawSprite(ball);
            camera.DrawSprite(cursor);

            //Draw button
            camera.DrawSprite(switchButton);

            //Draw text HUD
            String output = "Spins: " + currentSelection.SpinsLeft();
            Vector2 fontOrigin = gameFont.MeasureString(output) / 2;
            spriteBatch.DrawString(gameFont, output, HUD_spinsLeftPos, Color.Gold,
                0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);

            output = "Enemies: " + enemyCount;
            fontOrigin = gameFont.MeasureString(output) / 2;

            spriteBatch.DrawString(gameFont, output, HUD_enemyCountPos, Color.Gold,
                0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);

            output = "Time Remaining:";// +(int)(gametime / FPS);
            fontOrigin = gameFont.MeasureString(output) / 2;

            spriteBatch.DrawString(gameFont, output, HUD_timeTextPos, Color.Gold,
                0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);

            output = (int)(gametime / (FPS * 60)) + ":";

            if ((int)((gametime / FPS) % 60) < 10)
                output += "0";

            output += (int)((gametime / FPS) % 60);

            fontOrigin = gameFont.MeasureString(output) / 2;

            spriteBatch.DrawString(gameFont, output, HUD_timeLeftPos, Color.Gold,
                0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }
    }
}
