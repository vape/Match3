using System;

using Match3.Core;


namespace Match3.World.Animation
{
    public class ScaleDownAnimation : BlockAnimation
    {
        private float speed = 4f;
        private float scale = 1;

        public ScaleDownAnimation(Action<Block> animationEndedCallback)
            : base(animationEndedCallback)
        { }

        protected override Rect OnAnimationUpdate(Rect viewRect)
        {
            scale -= App.DeltaTime * speed;

            if (scale <= 0)
                Animating = false;

            return viewRect.ScaleFromCenter(scale);
        }
    }
}
