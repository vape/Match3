using Match3.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3.World.Animation
{
    public class MovingAnimation : BlockAnimation
    {
        private const float speed = 500;

        private Vector2 targetViewPosition;
        private Vector2 currentViewPosition;
        private Point targetGridPosition;

        public MovingAnimation(Vector2 targetPosition,
                               Point targetGridPoint,
                               Action<Block> animationEndCallback)
            : base(animationEndCallback)
        {
            targetViewPosition = targetPosition;
            targetGridPosition = targetGridPoint;

            this.animationEndCallback = (block) =>
            {
                block.GridPosition = targetGridPosition;
                block.ViewRect = new Rect(targetViewPosition, block.ViewRect.Size);

                if (animationEndCallback != null)
                    animationEndCallback(block);
            };
        }

        protected override void OnAnimationLoad()
        {
            currentViewPosition = Block.ViewRect.Position;
        }

        protected override Rect OnAnimationUpdate(Rect viewRect)
        {
            var direction = targetViewPosition - currentViewPosition;
            var distanceToTarget = direction.Length();
            var moveDistance = speed * App.DeltaTime;

            if (moveDistance > distanceToTarget)
                moveDistance = distanceToTarget;

            currentViewPosition = Vector2.Add(Vector2.Normalize(direction) * moveDistance,
                                              currentViewPosition);

            if (currentViewPosition == targetViewPosition)
                Animating = false;

            return new Rect(currentViewPosition, viewRect.Size);
        }
    }
}
