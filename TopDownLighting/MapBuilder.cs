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
        public Map BuildMap(MapDescription description, GraphicsDevice graphicsDevice, Texture2D floor, Texture2D wall)
        {
            var vertices = new List<MapVertex>();
            var faces = new List<MapFace>();
            var cellBoundaries = new List<CellBoundary>();

            for (int y = 0; y < description.Height; y++)
            {
                for (int x = 0; x < description.Width; x++)
                {
                    // Don't need to generate faces for solid cells.
                    if (description.IsSolid(x, y)) continue;

                    var boundingBox = new BoundingBox(new Vector3(x, 0, y), new Vector3(x + 1, description.MapWorldSpaceDimensions.CeilingHeight, y + 1));
                    var boundary = new CellBoundary(boundingBox);
                    cellBoundaries.Add(boundary);

                    AddFloor(new Point(x, y), vertices, faces);

                    var solidSides = description.GetSolidSides(x, y);
                    if (solidSides.HasFlag(CellSides.Left))
                    {
                        AddWall(new Point(x, y + 1), new Point(x, y), NormalDirection.East, vertices, faces);
                        boundary.AddSolidSide(CellSides.Left);
                    }
                    if (solidSides.HasFlag(CellSides.Top))
                    {
                        AddWall(new Point(x, y), new Point(x + 1, y), NormalDirection.South, vertices, faces);
                        boundary.AddSolidSide(CellSides.Top);
                    }
                    if (solidSides.HasFlag(CellSides.Right))
                    {
                        AddWall(new Point(x + 1, y), new Point(x + 1, y + 1), NormalDirection.West, vertices, faces);
                        boundary.AddSolidSide(CellSides.Right);
                    }
                    if (solidSides.HasFlag(CellSides.Bottom))
                    {
                        AddWall(new Point(x + 1, y + 1), new Point(x, y + 1), NormalDirection.North, vertices, faces);
                        boundary.AddSolidSide(CellSides.Bottom);
                    }
                }
            }

            var geometry = GenerateGeometry(vertices, faces, description.MapWorldSpaceDimensions, graphicsDevice);
            return new Map(geometry.Item1, geometry.Item2, geometry.Item3, cellBoundaries, floor, wall);
        }

        private static Tuple<VertexBuffer, IndexBuffer, IndexBuffer> GenerateGeometry(List<MapVertex> vertices, List<MapFace> faces, MapWorldSpaceDimensions dimensions, GraphicsDevice graphicsDevice)
        {
            var vertexGeometry = GenerateVertices(vertices, dimensions);
            var wallIndices = GenerateIndices(FaceType.Wall, faces);
            var floorIndices = GenerateIndices(FaceType.Floor, faces);
            var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(MapGeometryVertex), vertices.Count, BufferUsage.WriteOnly);
            var wallIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, wallIndices.Count(), BufferUsage.WriteOnly);
            var floorIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, floorIndices.Count(), BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertexGeometry.ToArray());
            wallIndexBuffer.SetData(wallIndices.ToArray());
            floorIndexBuffer.SetData(floorIndices.ToArray());
            return Tuple.Create(vertexBuffer, wallIndexBuffer, floorIndexBuffer);
        }

        private static IEnumerable<MapGeometryVertex> GenerateVertices(IEnumerable<MapVertex> vertices, MapWorldSpaceDimensions dimensions)
        {
            return from vertex in vertices
                   select new MapGeometryVertex
                   {
                        Position = new Vector3(
                            x: vertex.X * dimensions.HorizontalSize,
                            y: vertex.Ceiling ? dimensions.CeilingHeight : 0f,
                            z: vertex.Y * dimensions.HorizontalSize
                        ),
                        Normal = GenerateNormalFromDirection(vertex.NormalDirection),
                        Tex = vertex.Tex,
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

        private static IEnumerable<MapGeometryIndex> GenerateIndices(FaceType faceType, IEnumerable<MapFace> faces)
        {
            foreach (var face in faces.Where(f => f.FaceType == faceType))
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
                new MapVertex(left.X, left.Y, true, normalDirection, new Vector2(left.X + left.Y, 0)),
                new MapVertex(left.X, left.Y, false, normalDirection, new Vector2(left.X + left.Y, 1)),
                new MapVertex(right.X, right.Y, false, normalDirection, new Vector2(right.X + right.Y, 1)),
                new MapVertex(right.X, right.Y, true, normalDirection, new Vector2(right.X + right.Y, 0)),
            };

            var wallVertexIndices = wallVertices.Select(v => FindOrAddVertex(v, vertices)).ToList();
            faces.Add(new MapFace(FaceType.Wall, wallVertexIndices[0], wallVertexIndices[2], wallVertexIndices[1]));
            faces.Add(new MapFace(FaceType.Wall, wallVertexIndices[2], wallVertexIndices[0], wallVertexIndices[3]));
        }

        private static void AddFloor(Point pt, List<MapVertex> vertices, List<MapFace> faces)
        {
            var floorVertices = new[]
            {
                new MapVertex(pt.X, pt.Y, false, NormalDirection.Up, pt.ToVector2()),
                new MapVertex(pt.X, pt.Y + 1, false, NormalDirection.Up, new Vector2(pt.X, pt.Y + 1)),
                new MapVertex(pt.X + 1, pt.Y + 1, false, NormalDirection.Up, new Vector2(pt.X + 1, pt.Y + 1)),
                new MapVertex(pt.X + 1, pt.Y, false, NormalDirection.Up, new Vector2(pt.X + 1, pt.Y)),
            };

            var floorVertexIndices = floorVertices.Select(v => FindOrAddVertex(v, vertices)).ToList();
            faces.Add(new MapFace(FaceType.Floor, floorVertexIndices[0], floorVertexIndices[2], floorVertexIndices[1]));
            faces.Add(new MapFace(FaceType.Floor, floorVertexIndices[2], floorVertexIndices[0], floorVertexIndices[3]));
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
            public MapFace(FaceType faceType, uint v1Index, uint v2Index, uint v3Index)
            {
                FaceType = faceType;
                V1Index = v1Index;
                V2Index = v2Index;
                V3Index = v3Index;
            }

            public override string ToString()
            {
                return $"{V1Index}, {V2Index}, {V3Index}";
            }

            public FaceType FaceType { get; }
            public uint V1Index { get; }
            public uint V2Index { get; }
            public uint V3Index { get; }
        }

        private enum FaceType
        {
            Wall,
            Floor
        }

        private class MapVertex: IEquatable<MapVertex>
        {
            public MapVertex(int x, int y, bool ceiling, NormalDirection normalDirection, Vector2 tex)
            {
                X = x;
                Y = y;
                Ceiling = ceiling;
                NormalDirection = normalDirection;
                Tex = tex;
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
            public Vector2 Tex { get; }
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
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            );

            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 Tex;

            VertexDeclaration IVertexType.VertexDeclaration
            {
                get { return VertexDeclaration; }
            }

            public override string ToString()
            {
                return $"P: ({Position}), N: ({Normal}), T: ({Tex})";
            }
        }

        private struct MapGeometryIndex
        {
            public uint Index;
        }
    }
}
