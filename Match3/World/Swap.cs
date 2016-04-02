using System;
using System.Collections.Generic;
using System.Diagnostics;

using Match3.Utilities;
using Match3.World.Animation;


namespace Match3.World
{
    public class Swap
    {
        public static List<Swap> FindSwaps(BlockField field, int matchLength)
        {
            var swaps = new List<Swap>();

            for (int y = 0; y < field.Height; ++y)
            {
                for (int x = 0; x < field.Width; ++x)
                {
                    if (!field[y, x].Usable())
                        continue;

                    if (x < field.Width - 1 && field[y, x + 1].Usable())
                    {
                        field.SwapBlocks(x, y, x + 1, y);

                        var firstChain = Chain.GetMaxChainLength(field, field[y, x]);
                        var secondChain = Chain.GetMaxChainLength(field, field[y, x + 1]);

                        if (firstChain >= matchLength || secondChain >= matchLength)
                            swaps.Add(new Swap(field[y, x], field[y, x + 1]));

                        field.SwapBlocks(x, y, x + 1, y);
                    }

                    if (y < field.Height - 1 && field[y + 1, x].Usable())
                    {
                        field.SwapBlocks(x, y, x, y + 1);

                        var firstChain = Chain.GetMaxChainLength(field, field[y, x]);
                        var secondChain = Chain.GetMaxChainLength(field, field[y + 1, x]);

                        if (firstChain >= matchLength || secondChain >= matchLength)
                            swaps.Add(new Swap(field[y, x], field[y + 1, x]));

                        field.SwapBlocks(x, y, x, y + 1);
                    }
                }
            }

            return swaps;
        }

        public Block From
        { get; private set; }
        public Block To
        { get; private set; }
        public bool CanSwap
        { get; private set; }

        public Swap(Block fromBlock, Block toBlock)
        {
            Debug.Assert(fromBlock != null, "\"From\" block is null.");
            Debug.Assert(toBlock != null, "\"To\" block is null");

            From = fromBlock;
            To = toBlock;
            CanSwap = CheckSwap();
        }

        public void Make(Action<Swap> swappedCallback = null)
        {
            Debug.Assert(CanSwap, "Trying to make invalid swap.");

            Action<Block> onMoved = (block) =>
            {
                if (From.IsAnimating || To.IsAnimating)
                    return;

                if (swappedCallback != null)
                    swappedCallback(this);
            };

            From.AttachAnimation(new MovingAnimation(To.ViewRect.Position, To.GridPosition, onMoved));
            To.AttachAnimation(new MovingAnimation(From.ViewRect.Position, From.GridPosition, onMoved));
        }

        private bool CheckSwap()
        {
            if (From.X == To.X)
                return Math.Abs(From.Y - To.Y) == 1;

            if (From.Y == To.Y)
                return Math.Abs(From.X - To.X) == 1;

            return false;
        }
    }
}
