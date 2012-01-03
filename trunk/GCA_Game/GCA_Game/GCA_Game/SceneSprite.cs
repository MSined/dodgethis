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
        Cursor,
        Arrow,
    }

    public class SceneSprite
    {
        private Texture2D texture;
        private Vector2 worldPosition;
        private Vector2 worldDestination;
        private Vector2 worldDirection;

        private int movementSpeed;

        private SpriteType type;

        private bool hitSoundPlay;

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

        private Vector2 ball_unitVector;

        private bool alive;

        // Variables for ai purposes (set in moveTo) to keep characters from getting stuck against a wall
        bool atTop = false, atBottom = false, atLeft = false, atRight = false, destinationReached = true;

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
                return new Rectangle((int)worldPosition.X,
                                     (int)worldPosition.Y,
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

        public Vector2 Ball_UnitVector
        {
            //Texture of the sprite
            get { return ball_unitVector; }
            set { ball_unitVector = value; }
        }

        public int SpinsLeft()
        {
            return spin_amount;
        }

        public bool IsAlive()
        {
            return alive;
        }

        public SpriteType Type
        {
            //Type of the sprite
            get { return type; }
            set { type = value; }
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

            this.movementSpeed = 2;
            this.type = type;

            this.AIwander_maxtime = 0;// FPS * 3; //30 FPS is default on Windows 7 Phone IIRC
            this.AIwander_cooldown_maxtime = 0;//FPS * 2;
            this.AIwander_timer = 0;
            this.AIwander_cooldown_timer = 0;

            this.spinning = false;
            this.spin_maxtime = FPS;
            this.spin_timer = 0;
            this.spin_amount = 3; //Hardcoded, needs to change

            this.alive = true;

            this.AIstate = AIstate; //Also Resets AI
        }

        public void SpriteAI(float leftBounds, float rightBounds, float topBounds, float bottomBounds)
        {
            //Controls the sprite's AI

            //Check if sprite is under AI control
            if (AI_state == true)
            {
                if (destinationReached)
                    TryingToLookStrategicAI(leftBounds, rightBounds, topBounds, bottomBounds);
            }
        }

        private void ResetAI()
        {
            //Resets sprite's AI
            AIwander_cooldown_timer = 0;
            AIwander_timer = random.Next(0, AIwander_maxtime / 4);

            this.Direction = new Vector2(random.Next(-10, 10), random.Next(-10, 10));
        }

        private void TryingToLookStrategicAI(float leftBounds, float rightBounds, float topBounds, float bottomBounds)
        {
            //Resets sprite's AI
            AIwander_cooldown_timer = 0;
            AIwander_timer = random.Next(0, AIwander_maxtime / 4);

            if (atLeft)
            {
                this.Direction = new Vector2(random.Next(0, 60),
                                             random.Next(-30, 30));
                atLeft = false;
                destinationReached = false;
            }
            if (atRight)
            {
                this.Direction = new Vector2(random.Next(-60, 0),
                                             random.Next(-30, 30));
                atRight = false;
                destinationReached = false;
            }
            if (atTop)
            {
                this.Direction = new Vector2(random.Next(-30, 30),
                                             random.Next(0, 60));
                atTop = false;
                destinationReached = false;
            }
            if (atBottom)
            {
                this.Direction = new Vector2(random.Next(-30, 30),
                                             random.Next(-60, 0));
                atBottom = false;
                destinationReached = false;
            }
            this.Direction = new Vector2(random.Next(-30, 30),
                                         random.Next(-30, 30));
            destinationReached = false;
        }

        public void MoveSprite(float leftBounds, float rightBounds, float topBounds, float bottomBounds)
        {
            //Moves the sprite to its destination
            if ((worldDestination.X - worldPosition.X) * worldDirection.X >= 0 || (worldDestination.Y - worldPosition.Y) * worldDirection.Y >= 0)
            {
                //Ball
                if (type == SpriteType.Ball)
                {
                    worldPosition += ball_unitVector;
                    ball_unitVector *= 0.95f; //hardcoded slowdown multiplier
                    //worldPosition += worldDirection;
                }

                destinationReached = false;
                // Check left bound
                if ((worldPosition.X += worldDirection.X * movementSpeed) < leftBounds)
                {
                    worldDirection = new Vector2(0, 0);
                    worldPosition.X = leftBounds;
                    atLeft = true;
                    destinationReached = true;
                    hitSoundPlay = true;
                }
                // Check right bound
                if ((worldPosition.X += worldDirection.X * movementSpeed) > rightBounds)
                {
                    worldDirection = new Vector2(0, 0);
                    worldPosition.X = rightBounds;
                    atRight = true;
                    destinationReached = true;
                    hitSoundPlay = true;
                }
                // Check top bound
                if ((worldPosition.Y += worldDirection.Y * movementSpeed) < topBounds)
                {
                    worldDirection = new Vector2(0, 0);
                    worldPosition.Y = topBounds;
                    atTop = true;
                    destinationReached = true;
                    hitSoundPlay = true;
                }
                // Check bottom bound
                if ((worldPosition.Y += worldDirection.Y * movementSpeed) > bottomBounds)
                {
                    worldDirection = new Vector2(0, 0);
                    worldPosition.Y = bottomBounds;
                    atBottom = true;
                    destinationReached = true;
                    hitSoundPlay = true;
                }
                //The sprite has not reached its destination
                worldPosition += worldDirection * movementSpeed;
            }
            else //Destination reached
            {
                worldDirection = new Vector2(0f, 0f);
                destinationReached = true;
                hitSoundPlay = false;
            }
        }

        public bool getHitSoundPlay()
        {
            return hitSoundPlay;
        }

        public void setHitSoundPlay(bool done)
        {
            hitSoundPlay = done;
        }

        public Boolean collideWith(SceneSprite that)
        {
            if (this.Position.X < that.Position.X + that.texture.Width &&
                this.Position.X + this.texture.Width > that.Position.X &&
                this.Position.Y < that.Position.Y + that.texture.Height &&
                this.Position.Y + this.texture.Height > that.Position.X) { return true; }
            else { return false; }
        }
        public void followHolder(SceneSprite holder)
        {
            if (holder.type != SpriteType.Ball)
            {
                this.Position = holder.Position;
            }
        }

        public void Spin()
        {
            if (this.spin_amount > 0)
            {
                //Needs conditionals
                this.spin_timer = 0;
                spinning = true;
                this.spin_amount--;
            }
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
