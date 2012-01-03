using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GCA_Game
{
    public class Camera{
        private SpriteBatch spriteRenderer;
        private Vector2 cameraPosition; // top left corner of the camera
        private Vector2 viewSize;

        public Matrix viewMatrix;
        public Vector2 Position
        {
            get { return cameraPosition; }
            set 
            {
                cameraPosition = value;// +new Vector2(viewSize.X / 2, viewSize.Y / 2); ;
                UpdateViewMatrix();
            }
        }
        public float PositionX
        {
            get { return cameraPosition.X; }
            set { cameraPosition.X = value; }
        }
        public float PositionY
        {
            get { return cameraPosition.Y; }
            set { cameraPosition.Y = value; }
        }

        public Camera(SpriteBatch spriteBatch, Rectangle clientRect)
        {
            this.spriteRenderer = spriteBatch;
            viewSize = new Vector2(0, 0);//clientRect.Width / 2, clientRect.Height / 2);
            UpdateViewMatrix();
        }

        public void DrawSprite(SceneSprite node)
        {   
            // get the screen position of the node
            Vector2 drawPosition = ApplyTransformations(node.Position);
            node.Draw(spriteRenderer, drawPosition);
        }

        private Vector2 ApplyTransformations(Vector2 nodePosition)
        {
            // apply translation
            Vector2 finalPosition = nodePosition - cameraPosition;
            // you can apply scaling and rotation here also
            //.....
            return finalPosition;
        }

        public void Translate(Vector2 moveVector)
        {
            cameraPosition += moveVector;
        }

        private void UpdateViewMatrix()
        {
            viewMatrix = Matrix.CreateTranslation(viewSize.X - cameraPosition.X, viewSize.Y - cameraPosition.Y, 0.0f);
        }

    }
}
