using System;

using Microsoft.Xna.Framework;

using Match3.Core;


namespace Match3.World.Animation
{
    public class MovingAnimation : BlockAnimation
    {
        public const float DefaltSpeed = 4;

        private Vector2 currentViewPosition;
        private Vector2 targetViewPosition;
        private Point targetGridPosition;
        private float speed;

        public MovingAnimation(Vector2 targetViewPosition,
                               Point targetGridPosition,
                               Action<Block> animationEndedCallback,
                               float speed = DefaltSpeed)
            : base(animationEndedCallback)
        {
            this.targetViewPosition = targetViewPosition;
            this.targetGridPosition = targetGridPosition;
            this.speed = speed;

            this.AnimationEndedCallback = (block) =>
            {
                block.GridPosition = this.targetGridPosition;
                block.ViewRect = new Rect(this.targetViewPosition, block.ViewRect.Size);

                animationEndedCallback?.Invoke(block);
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
            var moveDistance = 100 * speed * App.DeltaTime;

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
