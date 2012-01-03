using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GCA_Game
{
    public enum SpriteType
    {
        Ninja,
        Pirate,
        Button,
        Cloud,
        Ball,
    }

    public class SceneSprite
    {
        private Texture2D texture;
        private Vector2 worldPosition;
        private Vector2 worldDestination;
        private Vector2 worldDirection;

        private int movementSpeed;

        private SpriteType type;

        Random random = new Random((int)DateTime.Now.Ticks);

        private bool AI_state;
        private int AIwander_timer;
        private int AIwander_cooldown_timer;
        private int AIwander_maxtime;
        private int AIwander_cooldown_maxtime;

        private bool spinning;
        private int spin_timer;
        private int spin_maxtime;
        private int spin_amount;

        private const int FPS = 30;

        private Vector2 ball_velocity;

        public Vector2 Position
        {
            //Position of the sprite on the world
            get { return worldPosition; }
            set { worldPosition = value; }// -new Vector2(texture.Width / 2, texture.Height / 2); }
        }

        public Vector2 Destination
        {
            //Destination of the sprite
            get { return worldDestination; }
            set { worldDestination = value; }// - new Vector2(texture.Width / 2, texture.Height / 2); }
        }

        public Vector2 Direction
        {
            //Direction the sprite is moving
            get { return worldDirection; }
            set
            {
                worldDirection = value;

                if (worldDirection.Length() > 1)
                    worldDirection.Normalize();
            }
        }

        public Rectangle Bounds
        {
            //Sprite's bounding box
            get 
            {
                return new Rectangle((int) worldPosition.X,
                                     (int) worldPosition.Y,
                                     texture.Bounds.Width,
                                     texture.Bounds.Height
                                    );
            }
        }

        public Texture2D Texture
        {
            //Texture of the sprite
            get { return texture; }
            set { texture = value; }
        }

        public bool AIstate
        {
            //Texture of the sprite
            get { return AI_state; }
            set
            {
                AI_state = value;

                if (value == true)
                    ResetAI();
                else
                    this.Direction = new Vector2(0, 0);
            }
        }

        public SceneSprite(Texture2D texture, Vector2 position, SpriteType type, bool AIstate)
        {
            this.texture = texture;
            this.Position = position;
            this.Destination = position;
            this.Direction = new Vector2(0, 0);

            this.movementSpeed = 5;
            this.type = type;

            this.AIwander_maxtime = FPS * 3; //30 FPS is default on Windows 7 Phone IIRC
            this.AIwander_cooldown_maxtime = FPS * 2; 
            this.AIwander_timer = 0;
            this.AIwander_cooldown_timer = 0;

            this.spinning = false;
            this.spin_maxtime = FPS * 2;
            this.spin_timer = 0;
            this.spin_amount = 3; //Hardcoded, needs to change

            this.AIstate = AIstate; //Also Resets AI

            this.ball_velocity = new Vector2(0f, 0f);
        }

        public void SpriteAI()
        {
            //Controls the sprite's AI

            //Check if sprite is under AI control
            if (AI_state == true)
            {
                if (AIwander_timer < AIwander_maxtime) //Sprite is moving
                    AIwander_timer++;
                else                                   //Sprite is waiting
                {
                    this.Direction = new Vector2(0, 0);
                    AIwander_cooldown_timer++;

                    if (AIwander_cooldown_timer >= AIwander_cooldown_maxtime) //Cooldown has expired
                        ResetAI();
                }
            }
        }

        private void ResetAI()
        {
            //Resets sprite's AI
            AIwander_cooldown_timer = 0;
            AIwander_timer = random.Next(0, AIwander_maxtime / 4);

            this.Direction = new Vector2(random.Next(-10, 10), random.Next(-10, 10));
        }

        public void MoveSprite()
        {
            //Moves the sprite to its destination

            //Unit
            if (type == SpriteType.Ninja || type == SpriteType.Pirate)
            {
                if ((worldDestination.X - worldPosition.X) * worldDirection.X >= 0 || (worldDestination.Y - worldPosition.Y) * worldDirection.Y >= 0)
                {
                    //The sprite has not reached its destination

                    worldPosition += worldDirection * movementSpeed;
                }
                else //Destination reached
                    worldDirection = new Vector2(0f, 0f);
            }

            //Ball
            if (type == SpriteType.Ball)
            {
                ball_velocity += new Vector2(0f, (float)(0.098 * 5));

                if (this.Position.Y > 400)
                {
                    ball_velocity.Y = -ball_velocity.Y * 0.8f;
                }

                if (ball_velocity.Y < 0 && ball_velocity.Y > -2)
                    ball_velocity.Y = 0;

                this.Position += ball_velocity;
            }

            //worldPosition = worldDestination;
        }

        public void Spin()
        {
            //Needs conditionals
            this.spin_timer = 0;
            spinning = true;
        }

        /// <summary>
        /// called by our camera class.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="drawPosition"></param>
        public void Draw(SpriteBatch renderer, Vector2 drawPosition)
        {
            SpriteEffects effect = SpriteEffects.None;

            if (this.type == SpriteType.Ninja || this.type == SpriteType.Pirate)
            {
                if (this.worldDirection.X >= 0)
                    effect = SpriteEffects.FlipHorizontally;

                if (this.spinning)
                {
                    this.spin_timer++;
                    for (int i = 0; i < this.spin_timer; i++)
                    {
                        if (effect == SpriteEffects.None)
                            effect = SpriteEffects.FlipHorizontally;
                        else
                            effect = SpriteEffects.None;
                    }

                    if (this.spin_timer >= spin_maxtime)
                    {
                        this.spin_timer = 0;
                        this.spinning = false;
                    }
                }
            }

            renderer.Draw(texture, this.Bounds, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), effect, 0f);
        }
    }
}
