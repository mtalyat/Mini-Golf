using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    /// <summary>
    /// Lays out all of its children into a grid.
    /// </summary>
    internal class LayoutObject : SpriteObject
    {
        public enum Orientation
        {
            /// <summary>
            /// Cells prefer to be vertically laid out.
            /// </summary>
            Vertical,

            /// <summary>
            /// Cells prefer to be horizontally laid out.
            /// </summary>
            Horizontal,

            /// <summary>
            /// Cells prefer to be in a grid.
            /// </summary>
            Grid
        }

        /// <summary>
        /// The number of cells in each row/column. Zero implies best fit.
        /// </summary>
        public Point CellCount { get; set; } = Point.Zero;

        /// <summary>
        /// The size of each cell in each row/column. Zero implies stretch to fill.
        /// </summary>
        public Vector2 CellSize { get; set; } = Vector2.Zero;

        public Orientation CellOrientation { get; set; } = Orientation.Grid;

        public LayoutObject(Vector2 size, Scene scene) : this(size, null, scene)
        { }

        public LayoutObject(Vector2 size, Sprite sprite, Scene scene) : base(sprite, scene)
        {
            LocalSize = size;
        }

        private int GetCount(int count, float cellSize, float localSize, Orientation orientation)
        {
            if (orientation == Orientation.Grid)
            {
                throw new NotImplementedException();
            }

            int c;

            if (CellOrientation == Orientation.Grid)
            {
                if(cellSize == 0.0f)
                {
                    c= (int)MathF.Ceiling(MathF.Sqrt(Children.Count));
                }
                else
                {
                    c = (int)MathF.Floor(localSize / cellSize);
                }
            }
            else
            {
                if (orientation == CellOrientation)
                {
                    c = Children.Count;
                }
                else
                {
                    c = 1;
                }
            }

            // ignore count if it is zero
            if (count == 0) return c;

            // if c is not zero, return the min of the calculated count and the given
            return Math.Min(count, c);
        }

        private float GetSize(float cellSize, float localSize, int cellCount, Orientation orientation)
        {
            if (orientation == Orientation.Grid)
            {
                throw new NotImplementedException();
            }

            if (CellOrientation == Orientation.Grid)
            {
                if(cellSize == 0.0f)
                {
                    if (cellCount == 0)
                    {
                        return 0.0f;
                    }
                    else
                    {
                        return localSize / cellCount;
                    }
                }
                else
                {
                    return cellSize;
                }
            }

            if (orientation == CellOrientation)
            {
                if (cellSize == 0.0f)
                {
                    if (cellCount == 0)
                    {
                        // size = 0, count = 0

                        // stretch to fill direction using all children
                        return localSize / Children.Count;
                    }
                    else
                    {
                        // size = 0, count > 0

                        // fit best it can given the size of this object
                        return localSize / cellCount;
                    }
                }
                else
                {
                    // given size works
                    return cellSize;
                }
            }
            else
            {
                // not the same direction
                // eg. CellOrientation is vertical, this is for the horizontal direction, which is only 1 unit wide

                if (cellSize == 0.0f)
                {
                    return localSize;
                }
                else
                {
                    return cellSize;
                }
            }
        }

        private float GetPosition(int index, float size, int countX, int countY, Orientation orientation)
        {
            if (orientation == Orientation.Grid)
            {
                throw new NotImplementedException();
            }

            // do not show if over the count, since this is a linear orientation
            if (index >= countX * countY) return -1.0f;

            if (CellOrientation == Orientation.Grid)
            {
                if (orientation == Orientation.Vertical)
                {
                    // vertical
                    return index / countX * size;
                }
                else
                {
                    // horizontal
                    return index % countX * size;
                }
            }

            if (orientation == CellOrientation)
            {
                // return the position based on the index and size
                return index * size;
            }
            else
            {
                // not the same direction
                // eg. CellOrientation is vertical, this is for the horizontal direction, which is only 1 unit wide

                // just return zero since only one unit
                return 0.0f;
            }
        }

        public void Refresh()
        {
            // calculate cell counts based on CellSize, CellOrientation and CellCounts
            int cellCountX = GetCount(CellCount.X, CellSize.X, LocalSize.X, Orientation.Horizontal);
            int cellCountY = GetCount(CellCount.Y, CellSize.Y, LocalSize.Y, Orientation.Vertical);

            float cellSizeX = GetSize(CellSize.X, LocalSize.X, cellCountX, Orientation.Horizontal);
            float cellSizeY = GetSize(CellSize.Y, LocalSize.Y, cellCountY, Orientation.Vertical);

            // manage child sizes
            for (int i = 0; i < Children.Count; i++)
            {
                GameObject child = Children[i];

                float positionX = GetPosition(i, cellSizeX, cellCountX, cellCountY, Orientation.Horizontal);
                float positionY = GetPosition(i, cellSizeY, cellCountX, cellCountY, Orientation.Vertical);

                if (positionX < 0.0f || positionY < 0.0f)
                {
                    // ignore child
                    child.LocalPosition = Vector2.Zero;
                    child.LocalSize = Vector2.Zero;

                    continue;
                }

                // use child
                child.LocalPosition = new Vector2(positionX, positionY);
                child.LocalSize = new Vector2(cellSizeX, cellSizeY);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
