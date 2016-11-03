using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDownLighting
{
    public class LittleBox
    {
        public LittleBox(Vector3 centre, float sideLength)
        {
            BoundingBox = CalculateBounds(centre, sideLength);
        }

        public Matrix CreateWorldMatrix()
        {
            var scale = Matrix.CreateScale(SideLength / 2f);
            var toCentre = Matrix.CreateTranslation(Centre);

            return scale * toCentre;
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.SetVertexBuffer(VertexBuffer);
            graphicsDevice.Indices = IndexBuffer;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
        }

        public static void SetBuffers(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            if (vertexBuffer == null) throw new ArgumentNullException(nameof(vertexBuffer));
            if (indexBuffer == null) throw new ArgumentNullException(nameof(indexBuffer));

            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;

            GenerateGeometry();
        }

        private static BoundingBox CalculateBounds(Vector3 centre, float sideLength)
        {
            Vector3 diagonal = new Vector3(sideLength / 2f);
            return new BoundingBox(centre - diagonal, centre + diagonal);
        }

        private static void GenerateGeometry()
        {
            var vertices = new[]
            {
                new VertexPositionNormalTexture(new Vector3(-1, 1, 1), Vector3.Backward, Vector2.Zero),
                new VertexPositionNormalTexture(new Vector3(1, 1, 1), Vector3.Backward, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, 1), Vector3.Backward, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 1), Vector3.Backward, Vector2.One),

                new VertexPositionNormalTexture(new Vector3(1, 1, 1), Vector3.Right, Vector2.Zero),
                new VertexPositionNormalTexture(new Vector3(1, 1, -1), Vector3.Right, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 1), Vector3.Right, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(1, -1, -1), Vector3.Right, Vector2.One),

                new VertexPositionNormalTexture(new Vector3(1, 1, -1), Vector3.Forward, Vector2.Zero),
                new VertexPositionNormalTexture(new Vector3(-1, 1, -1), Vector3.Forward, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, -1), Vector3.Forward, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, -1), Vector3.Forward, Vector2.One),

                new VertexPositionNormalTexture(new Vector3(-1, 1, -1), Vector3.Left, Vector2.Zero),
                new VertexPositionNormalTexture(new Vector3(-1, 1, 1), Vector3.Left, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, -1), Vector3.Left, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, 1), Vector3.Left, Vector2.One),

                new VertexPositionNormalTexture(new Vector3(-1, 1, -1), Vector3.Up, Vector2.Zero),
                new VertexPositionNormalTexture(new Vector3(1, 1, -1), Vector3.Up, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, 1, 1), Vector3.Up, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(1, 1, 1), Vector3.Up, Vector2.One),

                new VertexPositionNormalTexture(new Vector3(-1, -1, 1), Vector3.Down, Vector2.Zero),
                new VertexPositionNormalTexture(new Vector3(1, -1, 1), Vector3.Down, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, -1), Vector3.Down, new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(1, -1, -1), Vector3.Down, Vector2.One),
            };

            var indices = new ushort[]
            {
                0, 1, 2,
                2, 1, 3,

                4, 5, 6,
                6, 5, 7,

                8, 9, 10,
                10, 9, 11,

                12, 13, 14,
                14, 13, 15,

                16, 17, 18,
                18, 17, 19,

                20, 21, 22,
                22, 21, 23
            };

            VertexBuffer.SetData(vertices);
            IndexBuffer.SetData(indices);
        }

        public Vector3 Centre
        {
            get { return (BoundingBox.Min + BoundingBox.Max) / 2f; }
            set
            {
                BoundingBox = CalculateBounds(value, SideLength);
            }
        }

        public float SideLength
        {
            get { return (BoundingBox.Max - BoundingBox.Min).X; }
            set
            {
                BoundingBox = CalculateBounds(Centre, value);
            }
        }

        public BoundingBox BoundingBox { get; private set; }

        private static VertexBuffer VertexBuffer;
        private static IndexBuffer IndexBuffer;
    }
}
