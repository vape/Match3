using Match3.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3.World.Animation
{
    public class ExplodingAnimation : BlockAnimation
    {
        private float speed = 1f;
        private float scale = 1;
        private float delay = 0;

        public ExplodingAnimation(Action<Block> animationEndCallback,
                                  float delay = 0.35f)
            : base(animationEndCallback)
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
