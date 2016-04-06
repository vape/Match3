using Match3.World;

namespace Match3.Utilities
{
    public static class BlockExtensions
    {
        public static bool Usable(this Block block)
        {
            return block != null && !block.Animating;
        }
    }
}
