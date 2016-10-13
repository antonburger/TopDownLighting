using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDownLighting
{
    public class Map
    {
        public Map(int triangleCount, VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            this.triangleCount = triangleCount;
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Indices = IndexBuffer;
            graphicsDevice.SetVertexBuffer(VertexBuffer);
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triangleCount);
        }

        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer IndexBuffer { get; }

        private readonly int triangleCount;
    }
}
