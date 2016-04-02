using System;

using Match3.Core;


namespace Match3.World.Animation
{
    public class ScaleUpAnimation : BlockAnimation
    {
        public const float DefaultSpeed = 4;
        public const float DefaultDelay = 0;

        private float speed;
        private float delay;
        private float scale;

        public ScaleUpAnimation(Action<Block> animationEndedCallback,
                                float speed = DefaultSpeed,
                                float delay = DefaultDelay)
            : base(animationEndedCallback)
        {
            this.speed = speed;
            this.delay = delay;
            this.scale = 0;
        }

        protected override Rect OnAnimationUpdate(Rect viewRect)
        {
            if (App.Time - loadedTime < delay)
                return viewRect;

            scale += App.DeltaTime * speed;

            if (scale >= 1)
                Animating = false;

            return viewRect.ScaleFromCenter(scale);
        }
    }
}
