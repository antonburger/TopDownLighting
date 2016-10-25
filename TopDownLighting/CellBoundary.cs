using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDownLighting
{
    public class CellBoundary
    {
        public CellBoundary(BoundingBox boundingBox)
        {
            this.boundingBox = boundingBox;
            this.floorPlane = new Plane(Vector3.Up, Vector3.Dot(Vector3.Up, boundingBox.Min));
        }

        public void AddSolidSide(CellSides cellSide)
        {
            if (solidSides.ContainsKey(cellSide)) return;

            Vector3 normal;
            Vector3 pointOnPlane;
            switch (cellSide)
            {
            case CellSides.Left:
                normal = Vector3.Right;
                pointOnPlane = boundingBox.Min;
                break;
            case CellSides.Right:
                normal = Vector3.Left;
                pointOnPlane = boundingBox.Max;
                break;
            case CellSides.Top:
                normal = Vector3.Backward;
                pointOnPlane = boundingBox.Max;
                break;
            case CellSides.Bottom:
                normal = Vector3.Forward;
                pointOnPlane = boundingBox.Min;
                break;
            default:
                throw new Exception("Not a valid side.");
            }

            float d = Vector3.Dot(normal, pointOnPlane);

            var plane = new Plane(normal, d);

            solidSides[cellSide] = plane;
        }

        public float? Intersects(Ray ray)
        {
            if (ray.Intersects(boundingBox) == null) return null;
            var planes = solidSides.Values.Concat(new[] { floorPlane });
            var intersections = from plane in planes
                                let intersection = ray.Intersects(plane)
                                where intersection != null
                                    //&& boundingBox.Contains(ray.Position + (float)intersection * ray.Direction) != ContainmentType.Disjoint
                                orderby intersection ascending
                                select intersection;

            return intersections.FirstOrDefault();
        }

        private readonly BoundingBox boundingBox;
        private readonly Plane floorPlane;
        private readonly Dictionary<CellSides, Plane> solidSides = new Dictionary<CellSides, Plane>();
    }
}
