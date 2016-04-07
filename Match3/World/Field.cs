using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using Match3.Utilities;

namespace Match3.World
{
    public class Field : IEnumerable<Block>
    {
        public Block this[Point position]
        {
            get
            {
                return blocks[position.Y, position.X];
            }
            set
            {
                blocks[position.Y, position.X] = value;
            }
        }
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
                return blocks.Any((block) => block != null && block.Animating);
            }
        }

        public int Width { get; }
        public int Height { get; }

        private Block[,] blocks;

        public Field(Block[,] blocks)
        {
            this.blocks = blocks;

            Width = blocks.GetLength(1);
            Height = blocks.GetLength(0);
        }

        public List<Swap> FindSwaps(int matchLength)
        {
            var swaps = new List<Swap>();

            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    if (!blocks[y, x].Usable())
                        continue;

                    if (x < Width - 1 && blocks[y, x + 1].Usable())
                    {
                        Swap(x, y, x + 1, y);

                        var firstChain = GetMaxChainLength(blocks[y, x]);
                        var secondChain = GetMaxChainLength(blocks[y, x + 1]);

                        if (firstChain >= matchLength || secondChain >= matchLength)
                            swaps.Add(new Swap(blocks[y, x], blocks[y, x + 1]));

                        Swap(x, y, x + 1, y);
                    }

                    if (y < Height - 1 && blocks[y + 1, x].Usable())
                    {
                        Swap(x, y, x, y + 1);

                        var firstChain = GetMaxChainLength(blocks[y, x]);
                        var secondChain = GetMaxChainLength(blocks[y + 1, x]);

                        if (firstChain >= matchLength || secondChain >= matchLength)
                            swaps.Add(new Swap(blocks[y, x], blocks[y + 1, x]));

                        Swap(x, y, x, y + 1);
                    }
                }
            }

            return swaps;
        }

        public List<Chain> FindChains(int matchLength)
        {
            var vertical = FindVerticalChains(matchLength);
            var horizontal = FindHorizontalChains(matchLength);
            var intersections = new List<Chain>();

            for (int v = 0; v < vertical.Count; ++v)
            {
                for (int h = 0; h < horizontal.Count; ++h)
                {
                    if (horizontal[h] == null)
                        continue;

                    if (vertical[v].Intersect(horizontal[h]))
                    {
                        intersections.Add(new Chain(horizontal[h], vertical[v]));

                        vertical[v] = horizontal[h] = null;
                        break;
                    }
                }
            }

            return vertical.Where((x) => x != null)
                           .Concat(horizontal.Where((x) => x != null))
                           .Concat(intersections).ToList();
        }

        public void Swap(Swap swap)
        {
            Swap(swap.From.X, swap.From.Y, swap.To.X, swap.To.Y);
        }

        public void Swap(Point from, Point to)
        {
            Swap(from.X, from.Y, to.X, to.Y);
        }

        public void Swap(int fromX, int fromY, int toX, int toY)
        {
            var tempBlock = blocks[fromY, fromX];

            blocks[fromY, fromX] = blocks[toY, toX];
            blocks[toY, toX] = tempBlock;

            var tempGrid = blocks[fromY, fromX].GridPosition;
            // var tempView = blocks[fromY, fromX].ViewRect;

            blocks[fromY, fromX].GridPosition = blocks[toY, toX].GridPosition;
            // blocks[fromY, fromX].ViewRect = blocks[toY, toX].ViewRect;

            blocks[toY, toX].GridPosition = tempGrid;
            // blocks[toY, toX].ViewRect = tempView;
        }

        private List<Chain> FindVerticalChains(int matchLength)
        {
            var chains = new List<Chain>();

            for (int x = 0; x < Width; ++x)
            {
                var y = 0;

                while (y < Height)
                {
                    if (!blocks[y, x].Usable())
                    {
                        y++;
                        continue;
                    }

                    var chain = new List<Block>();

                    do
                    {
                        chain.Add(blocks[y, x]);
                        y++;
                    }
                    while (y < Height && blocks[y, x].Usable() && blocks[y, x].Type == chain[0].Type);

                    if (chain.Count >= matchLength)
                        chains.Add(new Chain(chain, ChainType.Vertical));
                }
            }

            return chains;
        }

        private List<Chain> FindHorizontalChains(int matchLength)
        {
            var chains = new List<Chain>();

            for (int y = 0; y < Height; ++y)
            {
                var x = 0;

                while (x < Width)
                {
                    if (!blocks[y, x].Usable())
                    {
                        x++;
                        continue;
                    }

                    var chain = new List<Block>();

                    do
                    {
                        chain.Add(blocks[y, x]);
                        x++;
                    }
                    while (x < Width && blocks[y, x].Usable() && blocks[y, x].Type == chain[0].Type);

                    if (chain.Count >= matchLength)
                        chains.Add(new Chain(chain, ChainType.Horizontal));
                }
            }

            return chains;
        }

        private int GetMaxChainLength(Block block)
        {
            return Math.Max(GetHorizontalChainLength(block),
                            GetVerticalChainLength(block));
        }

        private int GetVerticalChainLength(Block block)
        {
            int length = 1;

            for (int y = block.Y + 1; y < Height; ++y)
                if (blocks[y, block.X].Usable() && block.Type == blocks[y, block.X].Type)
                    length++;
                else
                    break;

            for (int y = block.Y - 1; y >= 0; --y)
                if (blocks[y, block.X].Usable() && block.Type == blocks[y, block.X].Type)
                    length++;
                else
                    break;

            return length;
        }

        private int GetHorizontalChainLength(Block block)
        {
            int length = 1;

            for (int x = block.X + 1; x < Width; ++x)
                if (blocks[block.Y, x].Usable() && block.Type == blocks[block.Y, x].Type)
                    length++;
                else
                    break;

            for (int x = block.X - 1; x >= 0; --x)
                if (blocks[block.Y, x].Usable() && block.Type == blocks[block.Y, x].Type)
                    length++;
                else
                    break;

            return length;
        }

        public IEnumerator<Block> GetEnumerator()
        {
            return blocks.Cast<Block>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return blocks.GetEnumerator();
        }
    }
}
