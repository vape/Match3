using System;


namespace Match3.World
{
    public class BlockCollectedEventArgs : EventArgs
    {
        public BlockType Type;
        public BlockBonusType Bonus;
        public int Multiplier;

        public BlockCollectedEventArgs(Block block, int multiplier)
            : this(block.Type, block.Bonus, multiplier)
        { }

        public BlockCollectedEventArgs(BlockType type, BlockBonusType bonus, int multiplier)
        {
            Type = type;
            Bonus = bonus;
            Multiplier = multiplier;
        }
    }
}
