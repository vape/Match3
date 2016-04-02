using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using Match3.Utilities;


namespace Match3.World
{
    public class BlockField : IEnumerable<Block>
    {
        public Block this[Block block]
        {
            get
            {
                return blocks[block.Y, block.X];
            }
            set
            {
                blocks[block.Y, block.X] = value;
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

        public bool Animating
        {
            get
            {
                return blocks.Any((block) => block != null && block.IsAnimating);
            }
        }

        public int Width
        { get; private set; }
        public int Height
        { get; private set; }

        private Block[,] blocks;

        public BlockField(Block[,] blocks)
        {
            this.blocks = blocks;

            Width = blocks.GetLength(1);
            Height = blocks.GetLength(0);
        }

        public void SwapBlocks(Point fromIndexes, Point toIndexes)
        {
            SwapBlocks(fromIndexes.X, fromIndexes.Y, toIndexes.X, toIndexes.Y);
        }

        public void SwapBlocks(int x1, int y1, int x2, int y2)
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
