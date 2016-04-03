using System;

using Match3.Core;


namespace Match3.World.Animation
{
    // TODO: Add special falling animation, instead of simple moving
    public abstract class BlockAnimation
    {
        public Block Block
        { get; private set; }
        public bool Animating
        { get; protected set; }

        protected float LoadedTime;
        protected Action<Block> AnimationEndedCallback;

        protected BlockAnimation(Action<Block> animationEndedCallback)
        {
            AnimationEndedCallback = animationEndedCallback;
        }

        public void Load(Block block)
        {
            Block = block;
            LoadedTime = App.Time;
            Animating = true;

            OnAnimationLoad();
        }

        public Rect Update(Rect viewRect)
        {
            if (!Animating)
                return viewRect;

            return OnAnimationUpdate(viewRect);
        }

        public void Stop()
        {
            Animating = false;

            if (AnimationEndedCallback != null)
            {
                AnimationEndedCallback(Block);
                AnimationEndedCallback = null;
            }
        }

        protected virtual void OnAnimationLoad() { }
        protected virtual Rect OnAnimationUpdate(Rect viewRect)
        {
            return viewRect;
        }
    }
}
