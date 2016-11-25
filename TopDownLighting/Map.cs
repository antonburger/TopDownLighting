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
        public Map(VertexBuffer vertexBuffer, IndexBuffer wallIndexBuffer, IndexBuffer floorIndexBuffer, List<CellBoundary> cellBoundaries, Texture2D floor, Texture2D wall, Texture2D floorNormal, Texture2D wallNormal)
        {
            this.cellBoundaries = cellBoundaries;
            Floor = floor;
            Wall = wall;
            FloorNormal = floorNormal;
            WallNormal = wallNormal;
            VertexBuffer = vertexBuffer;
            WallIndexBuffer = wallIndexBuffer;
            FloorIndexBuffer = floorIndexBuffer;
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.SetVertexBuffer(VertexBuffer);
            graphicsDevice.Indices = WallIndexBuffer;
            graphicsDevice.Textures[1] = Wall;
            graphicsDevice.Textures[0] = WallNormal;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, WallIndexBuffer.IndexCount / 3);
            graphicsDevice.Indices = FloorIndexBuffer;
            graphicsDevice.Textures[1] = Floor;
            graphicsDevice.Textures[0] = FloorNormal;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, FloorIndexBuffer.IndexCount / 3);
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
        public IndexBuffer WallIndexBuffer { get; }
        public IndexBuffer FloorIndexBuffer { get; }
        public Texture2D Floor { get; }
        public Texture2D Wall { get; }
        public Texture2D FloorNormal { get; }
        public Texture2D WallNormal { get; }

        private readonly List<CellBoundary> cellBoundaries;
    }
}
