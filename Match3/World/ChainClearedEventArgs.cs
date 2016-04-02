using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3.World
{
    public class ChainClearedEventArgs : EventArgs
    {
        public int ChainLength
        { get; private set; }
        public int Multiplier
        { get; private set; }

        public ChainClearedEventArgs(int chainLength, int multiplier)
        {
            ChainLength = chainLength;
            Multiplier = multiplier;
        }
    }
}
