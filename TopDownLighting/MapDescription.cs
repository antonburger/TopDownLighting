using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopDownLighting
{
    [Flags]
    public enum CellSides
    {
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
    }

    public class MapWorldSpaceDimensions
    {
        public MapWorldSpaceDimensions(float horizontalSize, float ceilingHeight)
        {
            HorizontalSize = horizontalSize;
            CeilingHeight = ceilingHeight;
        }

        public float HorizontalSize { get; }
        public float CeilingHeight { get; }
    }

    public class MapDescription
    {
        public MapDescription(int width, int height, MapWorldSpaceDimensions mapWorldSpaceDimensions)
        {
            Width = width;
            Height = height;
            MapWorldSpaceDimensions = mapWorldSpaceDimensions;

            mapData = new int[width][];
            for (int i = 0; i < width; i++)
            {
                mapData[i] = new int[height];
            }
        }

        public bool IsFloor(int x, int y)
        {
            return mapData[x][y] == 1;
        }

        public bool IsSolid(int x, int y)
        {
            return mapData[x][y] == 0;
        }

        public void SetFloor(int x, int y)
        {
            mapData[x][y] = 1;
        }

        public void SetSolid(int x, int y)
        {
            mapData[x][y] = 0;
        }

        public CellSides GetSolidSides(int x, int y)
        {
            CellSides result = 0;
            if (x == 0 || IsSolid(x - 1, y)) result |= CellSides.Left;
            if (x == Width - 1 || IsSolid(x + 1, y)) result |= CellSides.Right;
            if (y == 0 || IsSolid(x, y - 1)) result |= CellSides.Top;
            if (y == Height - 1 || IsSolid(x, y + 1)) result |= CellSides.Bottom;

            return result;
        }

        public int Width { get; }
        public int Height { get; }
        public MapWorldSpaceDimensions MapWorldSpaceDimensions { get; }

        private int[][] mapData;
    }
}
