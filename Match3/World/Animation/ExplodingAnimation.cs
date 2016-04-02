using System;

using Match3.Core;


namespace Match3.World.Animation
{
    public class ExplodingAnimation : BlockAnimation
    {
        private float speed = 1f;
        private float scale = 1;
        private float delay = 0;

        public ExplodingAnimation(Action<Block> animationEndedCallback,
                                  float delay = 0.35f)
            : base(animationEndedCallback)
        {
            this.delay = delay;
        }

        protected override Rect OnAnimationUpdate(Rect viewRect)
        {
            if (App.Time - loadedTime < delay)
                return viewRect;

            scale -= App.DeltaTime * speed;

            if (scale <= 0)
                Animating = false;

            return viewRect.ScaleFromCenter(scale);
        }
    }
}
