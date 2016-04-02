using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Match3.Utilities;


namespace Match3.World
{
    public class Chain
    {
        public static int GetMaxChainLength(BlockField field, Block block)
        {
            return Math.Max(GetHorizontalChainLength(field, block), 
                            GetVerticalChainLength(field, block));
        }

        public static int GetVerticalChainLength(BlockField field, Block block)
        {
            int length = 1;

            for (int y = block.Y + 1; y < field.Height; ++y)
                if (field[y, block.X].Usable() && block.Type == field[y, block.X].Type)
                    length++;
                else
                    break;

            for (int y = block.Y - 1; y >= 0; --y)
                if (field[y, block.X].Usable() && block.Type == field[y, block.X].Type)
                    length++;
                else
                    break;

            return length;
        }

        public static int GetHorizontalChainLength(BlockField field, Block block)
        {
            int length = 1;

            for (int x = block.X + 1; x < field.Width; ++x)
                if (field[block.Y, x].Usable() && block.Type == field[block.Y, x].Type)
                    length++;
                else
                    break;

            for (int x = block.X - 1; x >= 0; --x)
                if (field[block.Y, x].Usable() && block.Type == field[block.Y, x].Type)
                    length++;
                else
                    break;

            return length;
        }

        public static List<Chain> FindChains(BlockField field, int matchLength)
        {
            var vertical = FindVerticalChains(field, matchLength);
            var horizontal = FindHorizontalChains(field, matchLength);
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

        private static List<Chain> FindVerticalChains(BlockField field, int matchLength)
        {
            var chains = new List<Chain>();

            for (int x = 0; x < field.Width; ++x)
            {
                var y = 0;

                while (y < field.Height)
                {
                    if (!field[y, x].Usable())
                    {
                        y++;
                        continue;
                    }

                    var chain = new List<Block>();

                    do
                    {
                        chain.Add(field[y, x]);
                        y++;
                    }
                    while (y < field.Height && field[y, x].Usable() && field[y, x].Type == chain[0].Type);

                    if (chain.Count >= matchLength)
                        chains.Add(new Chain(chain, ChainType.Vertical));
                }
            }

            return chains;
        }

        private static List<Chain> FindHorizontalChains(BlockField field, int matchLength)
        {
            var chains = new List<Chain>();

            for (int y = 0; y < field.Height; ++y)
            {
                var x = 0;

                while (x < field.Width)
                {
                    if (!field[y, x].Usable())
                    {
                        x++;
                        continue;
                    }

                    var chain = new List<Block>();

                    do
                    {
                        chain.Add(field[y, x]);
                        x++;
                    }
                    while (x < field.Width && field[y, x].Usable() && field[y, x].Type == chain[0].Type);

                    if (chain.Count >= matchLength)
                        chains.Add(new Chain(chain, ChainType.Horizontal));
                }
            }

            return chains;
        }

        public BlockType BlockType
        { get; private set; }
        public ChainType ChainType
        { get; private set; }
        public List<Block> Blocks
        { get; private set; }
        public Block IntersectionBlock
        { get; private set; }
        public int Length
        { get; private set; }

        public Chain(Chain horizontal, Chain vertical)
            : this(horizontal.Blocks, vertical.Blocks)
        { }

        public Chain(IEnumerable<Block> horizontal, IEnumerable<Block> vertical)
            : this(horizontal.Concat(vertical), ChainType.Intersection)
        {
            IntersectionBlock = GetIntersectionBlock(horizontal, vertical);

            Debug.Assert(IntersectionBlock != null, "Given blocks chain is not an intersection.");
        }

        public Chain(IEnumerable<Block> blocks, ChainType type)
        {
            Blocks = blocks.ToList();

            Debug.Assert(Blocks.Count > 2, "Chain is too small.");
            Debug.Assert(!Blocks.Any((x) => x.Type != Blocks[0].Type), "Different block types in one chain.");

            if (type == ChainType.Horizontal)
                Debug.Assert(!Blocks.Any((b) => b.GridPosition.Y != Blocks[0].GridPosition.Y),
                             "Incorrect chain type. Blocks are not in the same column.");
            else if (type == ChainType.Vertical)
                Debug.Assert(!Blocks.Any((b) => b.GridPosition.X != Blocks[0].GridPosition.X),
                             "Incorrect chain type. Blocks are not in the same row.");

            BlockType = Blocks[0].Type;
            ChainType = type;
            Length = Blocks.Count;
        }

        public bool Intersect(Chain other)
        {
            if ((other.BlockType != BlockType) ||
                (other.ChainType == ChainType))
                return false;

            foreach (var first in Blocks)
            {
                foreach (var second in other.Blocks)
                {
                    if (first.X == second.X && first.Y == second.Y)
                        return true;
                }
            }

            return false;
        }

        private Block GetIntersectionBlock(IEnumerable<Block> horizontal,
                                           IEnumerable<Block> vertical)
        {
            foreach (var hor in horizontal)
            {
                foreach (var ver in vertical)
                {
                    if (hor.X == ver.X && hor.Y == ver.Y)
                        return hor;
                }
            }

            return null;
        }
    }
}
