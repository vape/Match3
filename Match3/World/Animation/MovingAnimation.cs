using System;

using Microsoft.Xna.Framework;

using Match3.Core;


namespace Match3.World.Animation
{
    public class MovingAnimation : BlockAnimation
    {
        public const float DefaltSpeed = 4f;

        private Vector2 currentPosition;
        private Vector2 targetPosition;
        private bool changeActualPosition;
        private float speed;

        public MovingAnimation(Vector2 targetPosition,
                               bool changeActualPosition,
                               Action<Block> animationEndedCallback, 
                               float speed = DefaltSpeed)
            : base(animationEndedCallback)
        {
            this.targetPosition = targetPosition;
            this.changeActualPosition = changeActualPosition;
            this.speed = speed;
        }

        protected override void OnLoaded()
        {
            currentPosition = Block.ViewRect.Position;
        }

        protected override void OnStopping()
        {
            if (changeActualPosition)
                Block.ViewRect = new Rect(targetPosition, Block.ViewRect.Size);
        }

        protected override Rect OnUpdate(Rect viewRect)
        {
            var direction = targetPosition - currentPosition;
            var distanceToTarget = direction.Length();
            var moveDistance = 100 * speed * App.DeltaTime;

            if (moveDistance > distanceToTarget)
                moveDistance = distanceToTarget;

            currentPosition = Vector2.Add(Vector2.Normalize(direction) * moveDistance,
                                          currentPosition);

            if (currentPosition == targetPosition)
                Animating = false;

            return new Rect(currentPosition, viewRect.Size);
        }
    }
}
