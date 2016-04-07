using System;

using Match3.Core;


namespace Match3.World.Animation
{
    public class ScaleDownAnimation : BlockAnimation
    {
        public const float DefaultSpeed = 4;
        public const float DefaultDelay = 0;

        private float speed;
        private float delay;
        private float scale;

        public ScaleDownAnimation(Action<Block> animationEndedCallback,
                                  float speed = DefaultSpeed,
                                  float delay = DefaultDelay)
            : base(animationEndedCallback)
        {
            this.speed = speed;
            this.delay = delay;
            this.scale = 1;
        }

        protected override Rect OnUpdate(Rect viewRect)
        {
            if (App.Time - LoadedTime < delay)
                return viewRect;

            scale -= App.DeltaTime * speed;

            if (scale <= 0)
                Animating = false;

            return viewRect.ScaleFromCenter(scale);
        }
    }
}
