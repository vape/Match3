using Match3.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3.World
{
    public class Chain
    {
        public static List<Block> GetLineBlocks(BlockField field, Block lineBlock)
        {
            var chainBlocks = new List<Block>();

            if (lineBlock.Bonus == BlockBonusType.HorizontalLine)
            {
                for (int x = 0; x < field.Width; ++x)
                {
                    var block = field[lineBlock.Y, x];
                    if (block.Usable() &&
                        block.GridPosition != lineBlock.GridPosition)

                        chainBlocks.Add(block);
                }
            }
            else if (lineBlock.Bonus == BlockBonusType.VerticalLine)
            {
                for (int y = 0; y < field.Height; ++y)
                {
                    var block = field[y, lineBlock.X];
                    if (block.Usable() &&
                        block.GridPosition != lineBlock.GridPosition)

                        chainBlocks.Add(field[y, lineBlock.X]);
                }
            }

            return chainBlocks;
        }

        public static List<Block> GetBombBlocks(BlockField field, Block bombBlock)
        {
            var chainBlocks = new List<Block>();

            for (int y = -1; y <= 1; ++y)
            {
                for (int x = -1; x <= 1; ++x)
                {
                    var block = field[bombBlock.Y + y, bombBlock.X + x];
                    if (bombBlock.X + x >= 0 && bombBlock.Y + y >= 0 &&
                        bombBlock.X + x < field.Width && bombBlock.Y + y < field.Height &&
                        block.Usable() && block.GridPosition != bombBlock.GridPosition)

                        chainBlocks.Add(block);
                }
            }

            return chainBlocks;
        }


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

                    if (vertical[v].IsIntersect(horizontal[h]))
                    {
                        intersections.Add(new Chain(vertical[v].Blocks.Concat(horizontal[h].Blocks),
                                          ChainType.Intersection));

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

        public int Length
        { get { return Blocks.Count; } }
        public Block IntersectionBlock
        { get; private set; }
        public List<Block> Blocks
        { get; private set; }

        public Chain(IEnumerable<Block> blocks, ChainType type)
        {
            #region Debug
#if DEBUG
            var _blocks = blocks.ToList();

            if (blocks.Any((x) => x.Type != _blocks[0].Type))
                throw new Exception("Blocks types are not the same.");

            if (type == ChainType.Horizontal)
            {
                if (blocks.Any((x) => x.GridPosition.Y != _blocks[0].GridPosition.Y))
                    throw new Exception("Incorrect chain type.");
            }
            else if (type == ChainType.Vertical)
            {
                if (blocks.Any((x) => x.GridPosition.X != _blocks[0].GridPosition.X))
                    throw new Exception("Incorrect chain type.");
            }
#endif
            #endregion

            Blocks = blocks.ToList();
            BlockType = Blocks[0].Type;
            ChainType = type;

            if (type == ChainType.Intersection)
            {
                IntersectionBlock = GetIntersectionBlock();

                #region Debug
#if DEBUG
                if (IntersectionBlock == null)
                    throw new Exception("Given blocks chain is not an intersection.");
#endif
                #endregion
            }
        }

        public bool IsIntersect(Chain other)
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

        private Block GetIntersectionBlock()
        {
            foreach (var first in Blocks)
            {
                foreach (var second in Blocks)
                {
                    if (first.X == second.X && first.Y == second.Y)
                        return first;
                }
            }

            return null;
        }
    }
}
