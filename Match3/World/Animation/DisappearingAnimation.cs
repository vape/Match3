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
        private const float scaleSpeed = 1f;

        public override bool Animating
        { get { return scale > 0; } }

        private float scale = 1;

        public DisappearingAnimation(Action<Block> animationEndCallback)
            : base(animationEndCallback)
        { }

        protected override Rect OnAnimationUpdate(Rect viewRect)
        {
            scale -= App.DeltaTime * scaleSpeed;

            return viewRect.ScaleFromCenter(scale);
        }
    }
}
