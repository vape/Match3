using Match3.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3.World
{
    public class Field : IEnumerable<Block>
    {
        public Block this[Block block]
        {
            get
            {
                return blocks[block.Y, block.X];
            }
            set
            {
                blocks[block.Y, block.X] = block;
            }
        }
        public Block this[int y, int x]
        {
            get
            {
                return blocks[y, x];
            }
            set
            {
                blocks[y, x] = value;
            }
        }

        public int Width
        { get; private set; }
        public int Height
        { get; private set; }

        private Block[,] blocks;

        public Field(Block[,] blocks)
        {
            this.blocks = blocks;

            Width = blocks.GetLength(1);
            Height = blocks.GetLength(0);
        }

        public bool AnyBlocksActive()
        {
            return blocks.Any((block) => block != null && !block.IsStill);
        }

        public bool AnyBlocksMoving()
        {
            return blocks.Any((block) => block != null && block.IsMoving);
        }

        public bool AnyBlocksAnimating()
        {
            return blocks.Any((block) => block != null && block.IsAnimating);
        }

        public void Move(Point from, Point to)
        {
            Move(from.X, from.Y, to.X, to.Y);
        }

        public void Move(int x1, int y1, int x2, int y2)
        {
            blocks[y2, x2] = blocks[y1, x1];
            blocks[y1, x1] = null;
        }

        public void Swap(Point from, Point to)
        {
            Swap(from.X, from.Y, to.X, to.Y);
        }

        public void Swap(int x1, int y1, int x2, int y2)
        {
            var temp = blocks[y1, x1];

            blocks[y1, x1] = blocks[y2, x2];
            blocks[y2, x2] = temp;

            var tempX = blocks[y1, x1].X;
            var tempY = blocks[y1, x1].Y;

            blocks[y1, x1].GridPosition = new Point(blocks[y2, x2].X, blocks[y2, x2].Y);
            blocks[y2, x2].GridPosition = new Point(tempX, tempY);
        }

        public void SwapValues(Point from, Point to)
        {
            SwapValues(from.X, from.Y, to.X, to.Y);
        }

        public void SwapValues(int x1, int y1, int x2, int y2)
        {
            var temp = blocks[y1, x1];

            blocks[y1, x1] = blocks[y2, x2];
            blocks[y2, x2] = temp;
        }

        public int GetMaxChainLength(int x, int y)
        {
            return GetMaxChainLength(blocks[y, x]);
        }

        public int GetMaxChainLength(Block block)
        {
            int horizontal = 1;
            int vertical = 1;

            for (int x = block.X + 1; x < Width; ++x)
                if (blocks[block.Y, x].Usable() && block.Type == blocks[block.Y, x].Type)
                    horizontal++;
                else
                    break;

            for (int x = block.X - 1; x >= 0; --x)
                if (blocks[block.Y, x].Usable() && block.Type == blocks[block.Y, x].Type)
                    horizontal++;
                else
                    break;

            for (int y = block.Y + 1; y < Height; ++y)
                if (blocks[y, block.X].Usable() && block.Type == blocks[y, block.X].Type)
                    vertical++;
                else
                    break;

            for (int y = block.Y - 1; y >= 0; --y)
                if (blocks[y, block.X].Usable() && block.Type == blocks[y, block.X].Type)
                    vertical++;
                else
                    break;

            return Math.Max(horizontal, vertical);
        }

        public IEnumerator<Block> GetEnumerator()
        {
            foreach (var block in blocks)
                yield return block;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return blocks.GetEnumerator();
        }
    }
}
