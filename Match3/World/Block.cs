using Microsoft.Xna.Framework;
using MonoGame.Extended.InputListeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Match3.Utilities;
using Match3.Core;
using System.Diagnostics;

namespace Match3.World
{
    public class Block
    {
        private enum AnimationType
        {
            None,
            Appearing,
            Disappearing,
        }

        private const float defaultMovingSpeed = 500;
        private const float defaultAnimatingSpeed = 1f;

        public static BlockType GetRandomBlockType()
        {
            var types = Enum.GetValues(typeof(BlockType));
            var type = (BlockType)types.GetValue(Utils.GetRand(0, types.Length));

            return type;
        }

        public static Texture2D GetTexture(BlockType blockType)
        {
            Color color = Color.White;
            Color[] colors = new Color[] { Color.Red, Color.Green, Color.Blue };

            switch (blockType)
            {
                case BlockType.Red:
                    color = Color.Red;
                    break;
                case BlockType.Green:
                    color = Color.Green;
                    break;
                case BlockType.Blue:
                    color = Color.Blue;
                    break;
                case BlockType.Violet:
                    color = Color.Violet;
                    break;
                case BlockType.Black:
                    color = Color.Black;
                    break;
            }

            return Utils.GetSolidRectangleTexture(64, 64, color);
        }

        public bool IsStill
        {
            get
            {
                return !IsMoving && !IsAnimating;
            }
        }

        public bool IsMoving
        {
            get
            {
                return moving;
            }
        }

        public bool IsAnimating
        {
            get
            {
                return animation != AnimationType.None;
            }
        }

        public BlockBonusType Bonus
        { get; private set; }
        public BlockType Type
        { get; private set; }
        public Texture2D Texture
        { get; private set; }
        public Color Color
        { get; private set; }

        public int X { get { return GridPosition.X; } }
        public int Y { get { return GridPosition.Y; } }

        public Point GridPosition
        { get; set; }
        public Rect ViewRect
        { get; private set; }
        public bool Selected
        { get; set; }

        private bool moving;
        private AnimationType animation;

        // Moving
        private Action<Block, Point, Point> movedCallback;
        private Vector2 targetViewPosition;
        private Point targetGridPosition;
        private Point originGridPosition;
        private float movingSpeed;

        // Appearing / Disappearing animation
        private Action<Block> appearedCallback;
        private Action<Block> disappearedCallback;
        private float alpha;
        private float scale;

        public Block(Point gridPosition, Vector2 viewPosition, Point viewSize,
                     BlockType? blockType = null, BlockBonusType? blockBonus = null)
        {
            GridPosition = gridPosition;
            ViewRect = new Rect(viewPosition, viewSize);

            Type = blockType ?? GetRandomBlockType();
            Bonus = blockBonus ?? BlockBonusType.None;

            Texture = GetTexture(Type);
            Color = Color.White;

            alpha = 1;
            scale = 1;
        }

        public void MoveTo(Vector2 targetViewPosition, Point targetGridPosition,
                           Action<Block, Point, Point> movedCallback = null,
                           bool setGridPositionImmediately = false,
                           float speed = defaultMovingSpeed)
        {
            this.originGridPosition = GridPosition;
            this.targetViewPosition = targetViewPosition;
            this.targetGridPosition = targetGridPosition;
            this.movedCallback = movedCallback;

            movingSpeed = speed;
            moving = true;

            if (setGridPositionImmediately)
                GridPosition = targetGridPosition;
        }

        public void MoveTo(Block block, 
                           Action<Block, Point, Point> movedCallback = null,
                           bool setGridPositionImmediately = false,
                           float speed = defaultMovingSpeed)
        {
            MoveTo(block.ViewRect.Position, block.GridPosition, movedCallback, 
                 setGridPositionImmediately, speed);
        }

        // TODO: Animate falling on refill
        public void AnimateFalling()
        {
            throw new NotImplementedException();
        }

        public void AnimateAppearing(Action<Block> appearedCallback = null)
        {
            animation = AnimationType.Appearing;

            alpha = 0;
            scale = 0;

            this.appearedCallback = appearedCallback;
        }

        public void AnimateDisappearing(Action<Block> disappearedCallback = null)
        {
            animation = AnimationType.Disappearing;

            scale = 1;

            this.disappearedCallback = disappearedCallback;
        }

        public void Draw(SpriteBatch sBatch)
        {
            sBatch.Draw(Texture, ViewRect.ScaleFromCenter(scale), Color);


            if (Selected)
            {
                sBatch.Draw(Utils.GetSolidRectangleTexture(1, 1, Color.AliceBlue), 
                            ViewRect.ScaleFromCenter(0.5f));
            }
        }

        public void UpdateMoving()
        {
            var direction = targetViewPosition - ViewRect.Position;
            var distanceToTarget = direction.Length();
            var moveDistance = movingSpeed * App.DeltaTime;

            if (moveDistance > distanceToTarget)
                moveDistance = distanceToTarget;

            var newPosition = Vector2.Add(Vector2.Normalize(direction) * moveDistance, 
                                          ViewRect.Position);

            ViewRect = new Rect(newPosition, ViewRect.Size);

            if (ViewRect.Position == targetViewPosition)
            {
                GridPosition = targetGridPosition;
                moving = false;

                if (movedCallback != null)
                    movedCallback(this, originGridPosition, targetGridPosition);
            }
        }

        public void UpdateAnimation()
        {
            switch (animation)
            {
                case AnimationType.Appearing:
                    UpdateAppearing();
                    break;
                case AnimationType.Disappearing:
                    UpdateDisappearing();
                    break;
            }
        }

        private void UpdateAppearing()
        {
            if (alpha < 1 && scale < 1)
            {
                alpha += App.DeltaTime * defaultAnimatingSpeed;
                scale += App.DeltaTime * defaultAnimatingSpeed;
            }
            else
            {
                animation = AnimationType.None;
                alpha = 1;
                scale = 1;

                if (appearedCallback != null)
                    appearedCallback(this);
            }
        }

        private void UpdateDisappearing()
        {
            if (scale > 0)
            {
                scale -= App.DeltaTime * defaultAnimatingSpeed;
            }
            else
            {
                animation = AnimationType.None;
                scale = 0;

                if (disappearedCallback != null)
                    disappearedCallback(this);
            }
        }
    }
}
