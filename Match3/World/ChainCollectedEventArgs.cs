using System;

namespace Match3.World
{
    public class ChainCollectedEventArgs : EventArgs
    {
        public int ChainLength
        { get; private set; }
        public int Multiplier
        { get; private set; }

        public ChainCollectedEventArgs(int chainLength, int multiplier)
        {
            ChainLength = chainLength;
            Multiplier = multiplier;
        }
    }
}
