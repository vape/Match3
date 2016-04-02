using Match3.Utilities;
using Match3.World.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                        field.Swap(x, y, x + 1, y);

                        var firstChain = Chain.GetMaxChainLength(field, field[y, x]);
                        var secondChain = Chain.GetMaxChainLength(field, field[y, x + 1]);

                        if (firstChain >= matchLength || secondChain >= matchLength)
                            swaps.Add(new Swap(field[y, x], field[y, x + 1]));

                        field.Swap(x, y, x + 1, y);
                    }

                    if (y < field.Height - 1 && field[y + 1, x].Usable())
                    {
                        field.Swap(x, y, x, y + 1);

                        var firstChain = Chain.GetMaxChainLength(field, field[y, x]);
                        var secondChain = Chain.GetMaxChainLength(field, field[y + 1, x]);

                        if (firstChain >= matchLength || secondChain >= matchLength)
                            swaps.Add(new Swap(field[y, x], field[y + 1, x]));

                        field.Swap(x, y, x, y + 1);
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
            #region Debug
#if DEBUG
            if (fromBlock == null)
                throw new ArgumentNullException(nameof(fromBlock));
            if (toBlock == null)
                throw new ArgumentNullException(nameof(toBlock));
#endif
            #endregion

            From = fromBlock;
            To = toBlock;

            CanSwap = CheckSwap();
        }

        public void Make(Action<Swap> swappedCallback = null)
        {
            if (!CanSwap)
                throw new InvalidOperationException("Can not swap this blocks.");

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
