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

        public bool Animating
        {
            get
            {
                return animation != null && animation.Animating;
            }
        }

        public BlockBonusType Bonus
        { get; }
        public BlockType Type
        { get; }

        public int X => GridPosition.X;
        public int Y => GridPosition.Y;

        public Point GridPosition
        { get; set; }
        public Rect ViewRect
        { get; set; }
        public bool Selected
        { get; set; }

        private Texture2D bonusTexture;
        private Texture2D blockTexture;
        private Texture2D blockSelectedTexture;

        private BlockAnimation animation;
        private Rect drawRect;
        private Color color;

        public Block(Point gridPosition, Vector2 viewPosition, Point viewSize,
                     BlockType? type = null, BlockBonusType? bonus = null)
        {
            Type = type ?? GetRandomBlockType();
            Bonus = bonus ?? BlockBonusType.None;

            GridPosition = gridPosition;
            ViewRect = new Rect(viewPosition, viewSize);

            LoadBlockTexture();
            LoadBonusTexture();

            color = Color.White;
        }

        public void AttachAnimation(BlockAnimation animation)
        {
            if (Animating)
                return;

            this.animation = animation;
            this.animation.Load(this);
        }

        public void Update()
        {
            if (Animating)
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
            if (Bonus != BlockBonusType.None)
            {
                sBatch.Draw(bonusTexture, drawRect);
                sBatch.Draw(Selected ? blockSelectedTexture : blockTexture, drawRect.ScaleFromCenter(0.5f), color);
            }
            else
            {
                sBatch.Draw(Selected ? blockSelectedTexture : blockTexture, drawRect, color);
            }
        }

        private void LoadBlockTexture()
        {
            var name = Type.ToString();

            blockTexture = App.LoadContent<Texture2D>($"Blocks/{name}");
            blockSelectedTexture = App.LoadContent<Texture2D>($"Blocks/{name}_Selected");
        }

        public void LoadBonusTexture()
        {
            if (Bonus == BlockBonusType.None)
                bonusTexture = null;
            else
                bonusTexture = App.LoadContent<Texture2D>($"Blocks/Bonus_{Bonus.ToString()}");
        }
    }
}
