using Match3.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3.World.Animation
{
    public class DisappearingAnimation : BlockAnimation
    {
        private float speed = 1;
        private float scale = 1;

        public DisappearingAnimation(Action<Block> animationEndCallback)
            : base(animationEndCallback)
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
