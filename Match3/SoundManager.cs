using Microsoft.Xna.Framework.Audio;

using Match3.Core;


namespace Match3
{
    public class SoundManager : GameObject
    {
        private SoundEffect blockCollectedSound;
        private SoundEffect chainCollectedSound;

        public SoundManager()
        {
            blockCollectedSound = App.LoadContent<SoundEffect>("Sound/BlockCollected");
            chainCollectedSound = App.LoadContent<SoundEffect>("Sound/ChainCollected");
        }

        public void PlayChainCollected(int multiplier)
        {
            var pitch = (multiplier / 7f);
            pitch = pitch > 1 ? 1 : pitch;

            chainCollectedSound.Play(1, pitch, 0);
        }

        public void PlayBlockCollected()
        {
            blockCollectedSound.Play(0.25f, 0, 0);
        }
    }
}
