using System;

using Match3.Core;


namespace Match3.World.Animation
{
    public class ScaleUpAnimation : BlockAnimation
    {
        private float speed = 5;
        private float scale = 0;

        public ScaleUpAnimation(Action<Block> animationEndedCallback)
            : base(animationEndedCallback)
        { }

        protected override Rect OnAnimationUpdate(Rect viewRect)
        {
            scale += App.DeltaTime * speed;

            if (scale >= 1)
                Animating = false;

            return viewRect.ScaleFromCenter(scale);
        }
    }
}
