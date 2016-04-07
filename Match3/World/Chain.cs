using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Match3.World
{
    public class Chain : IEnumerable<Block>
    {
        public Block this[int index]
        {
            get
            {
                return blocks[index];
            }
        }

        public BlockType BlockType
        { get; }
        public ChainType ChainType
        { get; }
        public Block IntersectionBlock
        { get; }
        public int Length
        { get; }

        private List<Block> blocks;

        public Chain(IEnumerable<Block> horizontal, IEnumerable<Block> vertical)
            : this(horizontal.Concat(vertical), ChainType.Intersection)
        {
            IntersectionBlock = GetIntersectionBlock(horizontal, vertical);

            Debug.Assert(IntersectionBlock != null, "Given blocks chain is not an intersection.");
        }

        public Chain(IEnumerable<Block> blocks, ChainType type)
        {
            this.blocks = blocks.ToList();

            Debug.Assert(this.blocks.Count > 2, "Chain is too small.");
            Debug.Assert(this.blocks.All((x) => x.Type == this.blocks[0].Type), 
                         "Different block types in one chain.");

            if (type == ChainType.Horizontal)
                Debug.Assert(this.blocks.All((b) => b.GridPosition.Y == this.blocks[0].GridPosition.Y),
                             "Incorrect chain type. Blocks are not in the same column.");
            else if (type == ChainType.Vertical)
                Debug.Assert(this.blocks.All((b) => b.GridPosition.X == this.blocks[0].GridPosition.X),
                             "Incorrect chain type. Blocks are not in the same row.");

            BlockType = this.blocks[0].Type;
            ChainType = type;
            Length = this.blocks.Count;
        }

        public bool Intersect(Chain other)
        {
            if ((other.BlockType != BlockType) ||
                (other.ChainType == ChainType))
                return false;

            return GetIntersectionBlock(blocks, other.blocks) != null;
        }

        private Block GetIntersectionBlock(IEnumerable<Block> horizontal,
                                           IEnumerable<Block> vertical)
        {
            foreach (var hor in horizontal)
                foreach (var ver in vertical)
                    if (hor.X == ver.X && hor.Y == ver.Y)
                        return hor;

            return null;
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
