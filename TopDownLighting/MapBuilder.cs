using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TopDownLighting
{
    public class MapBuilder
    {
        public Map BuildMap(MapDescription description, GraphicsDevice graphicsDevice)
        {
            var vertices = new List<MapVertex>();
            var faces = new List<MapFace>();

            for (int y = 0; y < description.Height; y++)
            {
                for (int x = 0; x < description.Width; x++)
                {
                    // Don't need to generate faces for solid cells.
                    if (description.IsSolid(x, y)) continue;

                    var solidSides = description.GetSolidSides(x, y);
                    if (solidSides.HasFlag(CellSides.Left))
                    {
                        AddWall(new Point(x, y + 1), new Point(x, y), NormalDirection.East, vertices, faces);
                    }
                    if (solidSides.HasFlag(CellSides.Top))
                    {
                        AddWall(new Point(x, y), new Point(x + 1, y), NormalDirection.South, vertices, faces);
                    }
                    if (solidSides.HasFlag(CellSides.Right))
                    {
                        AddWall(new Point(x + 1, y), new Point(x + 1, y + 1), NormalDirection.West, vertices, faces);
                    }
                    if (solidSides.HasFlag(CellSides.Bottom))
                    {
                        AddWall(new Point(x + 1, y + 1), new Point(x, y + 1), NormalDirection.North, vertices, faces);
                    }
                }
            }

            var geometry = GenerateGeometry(vertices, faces, description.MapWorldSpaceDimensions, graphicsDevice);
            return new Map(faces.Count, geometry.Item1, geometry.Item2);
        }

        private static Tuple<VertexBuffer, IndexBuffer> GenerateGeometry(List<MapVertex> vertices, List<MapFace> faces, MapWorldSpaceDimensions dimensions, GraphicsDevice graphicsDevice)
        {
            var vertexGeometry = GenerateVertices(vertices, dimensions);
            var indexGeometry = GenerateIndices(faces);
            var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(MapGeometryVertex), vertices.Count, BufferUsage.WriteOnly);
            var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, faces.Count * 3, BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertexGeometry.ToArray());
            indexBuffer.SetData(indexGeometry.ToArray());
            return Tuple.Create(vertexBuffer, indexBuffer);
        }

        private static IEnumerable<MapGeometryVertex> GenerateVertices(IEnumerable<MapVertex> vertices, MapWorldSpaceDimensions dimensions)
        {
            return from vertex in vertices
                   select new MapGeometryVertex
                   {
                        Position = new Vector4(
                            x: vertex.X * dimensions.HorizontalSize,
                            y: vertex.Ceiling ? dimensions.CeilingHeight : 0f,
                            z: vertex.Y * dimensions.HorizontalSize,
                            w: 1f
                        ),
                        Normal = GenerateNormalFromDirection(vertex.NormalDirection)
                   };
        }

        private static Vector3 GenerateNormalFromDirection(NormalDirection direction)
        {
            switch (direction)
            {
            case NormalDirection.North: return Vector3.Forward;
            case NormalDirection.East: return Vector3.Right;
            case NormalDirection.South: return Vector3.Backward;
            case NormalDirection.West: return Vector3.Left;
            case NormalDirection.Up: return Vector3.Up;
            }

            throw new ArgumentException();
        }

        private static IEnumerable<MapGeometryIndex> GenerateIndices(IEnumerable<MapFace> faces)
        {
            foreach (var face in faces)
            {
                yield return new MapGeometryIndex { Index = face.V1Index };
                yield return new MapGeometryIndex { Index = face.V2Index };
                yield return new MapGeometryIndex { Index = face.V3Index };
            }
        }

        private static void AddWall(Point left, Point right, NormalDirection normalDirection, List<MapVertex> vertices, List<MapFace> faces)
        {
            var wallVertices = new[]
            {
                new MapVertex(left.X, left.Y, true, normalDirection),
                new MapVertex(left.X, left.Y, false, normalDirection),
                new MapVertex(right.X, right.Y, false, normalDirection),
                new MapVertex(right.X, right.Y, true, normalDirection),
            };

            var wallVertexIndices = wallVertices.Select(v => FindOrAddVertex(v, vertices)).ToList();
            faces.Add(new MapFace(wallVertexIndices[0], wallVertexIndices[1], wallVertexIndices[2]));
            faces.Add(new MapFace(wallVertexIndices[2], wallVertexIndices[3], wallVertexIndices[0]));
        }

        private static uint FindOrAddVertex(MapVertex vertex, List<MapVertex> vertices)
        {
            var index = vertices.IndexOf(vertex);
            if (index >= 0) return (uint)index;

            vertices.Add(vertex);
            return (uint)vertices.Count - 1;
        }

        private class MapFace
        {
            public MapFace(uint v1Index, uint v2Index, uint v3Index)
            {
                V1Index = v1Index;
                V2Index = v2Index;
                V3Index = v3Index;
            }

            public override string ToString()
            {
                return $"{V1Index}, {V2Index}, {V3Index}";
            }

            public uint V1Index { get; }
            public uint V2Index { get; }
            public uint V3Index { get; }
        }

        private class MapVertex: IEquatable<MapVertex>
        {
            public MapVertex(int x, int y, bool ceiling, NormalDirection normalDirection)
            {
                X = x;
                Y = y;
                Ceiling = ceiling;
                NormalDirection = normalDirection;
            }

            public bool Equals(MapVertex other)
            {
                return X == other.X && Y == other.Y && Ceiling == other.Ceiling && NormalDirection == other.NormalDirection;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is MapVertex)) return false;
                return Equals((MapVertex)obj);
            }

            public override int GetHashCode()
            {
                return X.GetHashCode() ^ Y.GetHashCode() ^ Ceiling.GetHashCode() ^ NormalDirection.GetHashCode();
            }

            public override string ToString()
            {
                return $"{X}, {Y}, {Ceiling}, {NormalDirection}";
            }

            public int X { get; }
            public int Y { get; }
            public bool Ceiling { get; }
            public NormalDirection NormalDirection { get; }
        }

        private enum NormalDirection
        {
            North,
            East,
            South,
            West,
            Up
        }

        private struct MapGeometryVertex: IVertexType
        {
            private static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
            );

            public Vector4 Position;
            public Vector3 Normal;

            VertexDeclaration IVertexType.VertexDeclaration
            {
                get { return VertexDeclaration; }
            }

            public override string ToString()
            {
                return $"P: ({Position}), N: ({Normal})";
            }
        }

        private struct MapGeometryIndex
        {
            public uint Index;
        }
    }
}
