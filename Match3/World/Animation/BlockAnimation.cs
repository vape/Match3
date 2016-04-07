using System;

using Match3.Core;


namespace Match3.World.Animation
{
    public abstract class BlockAnimation
    {
        public Block Block
        { get; private set; }
        public bool Animating
        { get; protected set; }

        protected float LoadedTime;
        protected Action<Block> AnimationEndedCallback;

        private bool stopped;

        protected BlockAnimation(Action<Block> animationEndedCallback)
        {
            AnimationEndedCallback = animationEndedCallback;
        }

        public void Load(Block block)
        {
            OnLoading();

            Block = block;
            LoadedTime = App.Time;
            Animating = true;

            OnLoaded();
        }

        public Rect Update(Rect viewRect)
        {
            if (!Animating)
                return viewRect;

            return OnUpdate(viewRect);
        }

        public void Stop()
        {
            if (!stopped)
            {
                OnStopping();

                Animating = false;
                AnimationEndedCallback?.Invoke(Block);
                stopped = true;

                OnStopped();
            }
        }

        protected virtual void OnLoading() { }
        protected virtual void OnLoaded() { }
        protected virtual void OnStopping() { }
        protected virtual void OnStopped() { }

        protected virtual Rect OnUpdate(Rect viewRect)
        {
            return viewRect;
        }
    }
}
