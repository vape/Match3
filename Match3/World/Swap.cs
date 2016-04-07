using System;
using System.Diagnostics;

using Match3.World.Animation;


namespace Match3.World
{
    public class Swap
    {
        public Block From
        { get; }
        public Block To
        { get; }
        public bool CanSwap
        { get; }

        public Swap(Block fromBlock, Block toBlock)
        {
            Debug.Assert(fromBlock != null, "\"From\" block is null.");
            Debug.Assert(toBlock != null, "\"To\" block is null");

            From = fromBlock;
            To = toBlock;
            CanSwap = CheckSwap();
        }

        public void Move(Action<Swap> animationEndedCallback = null)
        {
            Action<Block> animationEnded = (block) =>
            {
                if (From.Animating || To.Animating)
                    return;

                animationEndedCallback?.Invoke(this);
            };

            From.AttachAnimation(new MovingAnimation(To.ViewRect.Position, true, animationEnded));
            To.AttachAnimation(new MovingAnimation(From.ViewRect.Position, true, animationEnded));
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
