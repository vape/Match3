using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Match3.Core;

namespace Match3.World.Animation
{
    public class AppearingAnimation : BlockAnimation
    {
        private float speed = 1;
        private float scale = 0;

        public AppearingAnimation(Action<Block> animationEndCallback)
            : base(animationEndCallback)
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
