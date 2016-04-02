using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Match3.Core;
using Match3.Utilities;
using Match3.World.Animation;


namespace Match3.World
{
    public class Block
    {
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

        public bool IsAnimating
        {
            get
            {
                return animation != null && animation.Animating;
            }
        }

        public BlockBonusType Bonus
        { get; private set; }
        public BlockType Type
        { get; private set; }

        public int X { get { return GridPosition.X; } }
        public int Y { get { return GridPosition.Y; } }

        public Point GridPosition
        { get; set; }
        public Rect ViewRect
        { get; set; }
        public bool Selected
        { get; set; }

        private BlockAnimation animation;
        private Rect drawRect;
        private Texture2D texture;
        private Color color;

        public Block(Point gridPosition, Vector2 viewPosition, Point viewSize,
                     BlockType? blockType = null, BlockBonusType? blockBonus = null)
        {
            Type = blockType ?? GetRandomBlockType();
            Bonus = blockBonus ?? BlockBonusType.None;

            GridPosition = gridPosition;
            ViewRect = new Rect(viewPosition, viewSize);

            drawRect = ViewRect;
            texture = GetTexture(Type);
            color = Color.White;
        }

        public void AttachAnimation(BlockAnimation animation)
        {
            if (IsAnimating)
                return;

            this.animation = animation;
            this.animation.Load(this);
        }

        public void Update()
        {
            if (IsAnimating)
            {
                drawRect = animation.Update(ViewRect);

                if (!animation.Animating)
                    animation.Stop();
            }
            else
            {
                if (drawRect != ViewRect)
                    drawRect = ViewRect;
            }
        }

        public void Draw(SpriteBatch sBatch)
        {
            sBatch.Draw(texture, drawRect, color);

            // TODO: Get rid of this
            if (Bonus == BlockBonusType.Bomb)
            {
                sBatch.DrawRect(drawRect.ToMonogameRectangle(), color);
                sBatch.Draw(Utils.GetSolidRectangleTexture(1, 1, Color.AliceBlue),
                            drawRect.ScaleFromCenter(0.65f), Color.Yellow);
            }
            else if (Bonus == BlockBonusType.HorizontalLine ||
                     Bonus == BlockBonusType.VerticalLine)
            {
                sBatch.Draw(Utils.GetSolidRectangleTexture(1, 1, Color.AliceBlue),
                            drawRect.ScaleFromCenter(0.65f), Color.Purple);
            }

            if (Selected)
            {
                sBatch.Draw(Utils.GetSolidRectangleTexture(1, 1, Color.AliceBlue), 
                            drawRect.ScaleFromCenter(0.35f));
            }
        }
    }
}
