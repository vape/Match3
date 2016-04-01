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
                return !moving && !appearing && !disappearing;
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
                return appearing || disappearing;
            }
        }

        public BlockBonusType Bonus
        { get; private set; }
        public BlockType Type
        { get; private set; }
        public Texture2D Texture
        { get; private set; }
        public Color Color
        { get { return new Color(color, alpha); } }

        public int X { get { return GridPosition.X; } }
        public int Y { get { return GridPosition.Y; } }

        public Point GridPosition
        { get; set; }
        public Rect ViewRect
        { get; private set; }
        public bool Selected
        { get; set; }

        private bool moving;
        private bool appearing;
        private bool disappearing;

        private Action<Block> appearedCallback;
        private Action<Block> disappearedCallback;

        // Moving animation
        private Action<Block, Point, Point> movedCallback;
        private Vector2 targetViewPosition;
        private Point targetGridPosition;
        private Point originGridPosition;
        private float movingSpeed;

        private Color color;
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

            color = Color.White;
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
            if (disappearing)
                disappearing = false;

            alpha = 0;
            scale = 0;
            appearing = true;

            this.appearedCallback = appearedCallback;
        }

        public void AnimateDisappearing(Action<Block> disappearedCallback = null)
        {
            if (appearing)
                appearing = false;

            scale = 1;
            disappearing = true;

            this.disappearedCallback = disappearedCallback;
        }

        public void Draw(SpriteBatch sBatch)
        {
            sBatch.Draw(Texture, ViewRect.ScaleFromCenter(scale).ToMonogameRectangle(), Color);

            if (Selected)
                sBatch.Draw(Utils.GetSolidRectangleTexture(15, 15, Color.AliceBlue), ViewRect.Position + new Vector2(15, 15), Color.White);
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
            if (appearing)
                UpdateAppearing();
            else if (disappearing)
                UpdateDisappearing();
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
                appearing = false;
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
                disappearing = false;
                scale = 0;

                if (disappearedCallback != null)
                    disappearedCallback(this);
            }
        }
    }
}
