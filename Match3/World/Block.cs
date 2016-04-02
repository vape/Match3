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

        public static void GetTexture(BlockType blockType, 
                                      out Texture2D texture, 
                                      out Texture2D selected)
        {
            var name = blockType.ToString();
            var index = Utils.GetRand(1, 3);

            texture = App.LoadContent<Texture2D>(String.Format("Blocks/{0}_{1}", name, index));
            selected = App.LoadContent<Texture2D>(String.Format("Blocks/{0}_{1}_Selected", name, index));
        }

        public static void GetBonusTexture(BlockBonusType bonusType,
                                           out Texture2D texture)
        {
            if (bonusType == BlockBonusType.None)
            {
                texture = null;
                return;
            }

            var name = bonusType.ToString();

            texture = App.LoadContent<Texture2D>(String.Format("Blocks/Bonus_{0}", name));
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

        private Texture2D bonusTexture;
        private BlockAnimation animation;
        private Rect drawRect;
        private Texture2D texture;
        private Texture2D selected;
        private Color color;

        public Block(Point gridPosition, Vector2 viewPosition, Point viewSize,
                     BlockType? blockType = null, BlockBonusType? blockBonus = null)
        {
            Type = blockType ?? GetRandomBlockType();
            Bonus = blockBonus ?? BlockBonusType.None;

            GridPosition = gridPosition;
            ViewRect = new Rect(viewPosition, viewSize);

            GetTexture(Type, out texture, out selected);
            GetBonusTexture(Bonus, out bonusTexture);

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
            if (Bonus != BlockBonusType.None)
            {
                sBatch.Draw(bonusTexture, drawRect);

                if (Selected)
                    sBatch.Draw(selected, drawRect.ScaleFromCenter(0.5f), color);
                else
                    sBatch.Draw(texture, drawRect.ScaleFromCenter(0.5f), color);
            }
            else
            {
                if (Selected)
                    sBatch.Draw(selected, drawRect, color);
                else
                    sBatch.Draw(texture, drawRect, color);
            }
        }
    }
}
