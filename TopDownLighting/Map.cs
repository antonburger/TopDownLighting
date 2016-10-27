using Microsoft.Xna.Framework;
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
        public Map(int triangleCount, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, List<CellBoundary> cellBoundaries, Texture2D floor, Texture2D wall)
        {
            this.triangleCount = triangleCount;
            this.cellBoundaries = cellBoundaries;
            this.Floor = floor;
            this.Wall = wall;
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Indices = IndexBuffer;
            graphicsDevice.SetVertexBuffer(VertexBuffer);
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triangleCount);
        }

        public float? Intersects(Ray ray)
        {
            var intersections = from boundary in cellBoundaries
                                let intersection = boundary.Intersects(ray)
                                where intersection != null
                                orderby intersection ascending
                                select intersection;

            return intersections.FirstOrDefault();
        }

        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer IndexBuffer { get; }
        public Texture2D Floor { get; }
        public Texture2D Wall { get; }

        private readonly int triangleCount;
        private readonly List<CellBoundary> cellBoundaries;
    }
}
